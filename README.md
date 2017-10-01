# Cross Platform Sample for Trackable Entities with EF Core

Cross-platform sample for Trackable Entities with ASP.NET Core and Entity Framework Core with SQLite.

> **Note**: This solution was created using [Visual Studio for Mac](https://www.visualstudio.com/vs/visual-studio-mac/), but you can also use [Visual Studio for Windows](https://www.visualstudio.com/vs/community/) or [Visual Studio Code](https://code.visualstudio.com/).

## Prerequisites

- [.NET Core SDK](https://www.microsoft.com/net/download/core) 2.0 or greater.

## Steps

1. Create a new ASP.NET Core Web API project in Visual Studio.
2. Add a class library with _server-side_ trackable entities.
3. Add Entity Framework with SQLite to the Web API project.
4. Add a data context to the Web API project and use data migrations to create the database.
5. Install Trackable Entities for EF Core and add Web API controllers with GET, POST, PUT and DELETE actions.
6. Register the `DbContext` with dependency injection in the Web API project.
7. Seed the database with intitial data.
8. Generate _client-side_ trackable entities in a .NET Standard class library project.
9. Add a .NET Core console application that uses the **TrackableEntities.Client** NuGet package.

## ASP.NET Core Web API Project with Trackable Entities

1. Create a new ASP.NET Core Web API project in Visual Studio.
    - You can run the Web API from a Terminal with the .NET Core CLI.

    ```bash
    dotnet run
    ```

2. Add a class library with _server-side_ trackable entities.
    - Add a NetStandard 2.0 library project.
    - Add the **TrackableEntities.Common.Core** NuGet package (prerelease).
    - Add the **System.ComponentModel.Annotations** NuGet package.
    - Add classes that implement the `ITrackable` and `IMergeable` interfaces.
        + Add the import: `using System.ComponentModel.DataAnnotations.Schema`
        + Decorate interface properties with a `[NotMapped]` attribute.

    ```csharp
    public class Product : ITrackable, IMergeable
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? CategoryId { get; set; }
        public decimal? UnitPrice { get; set; }
        public bool Discontinued { get; set; }
        public byte[] RowVersion { get; set; }
        public Category Category { get; set; }

        [NotMapped]
        public TrackingState TrackingState { get; set; }

        [NotMapped]
        public ICollection<string> ModifiedProperties { get; set; }

        [NotMapped]
        public Guid EntityIdentifier { get; set; }
    }
    ```

## EF Core Migrations

3. Add Entity Framework with SQLite to the Web API project.

    - Add the package: **Microsoft.EntityFrameworkCore.Sqlite**
    - Add the package: **Microsoft.EntityFrameworkCore.Design**
    - Manually edit _.csproj_ file to add a **DotNetCliToolReference**
        + Change the target framework from `netstandard2.0` to `netcoreapp2.0`.

        > **Note**: The project needs to target `netcoreapp2.0` in order for the Ef migration tools to function.

    ```xml
    <ItemGroup>
        <DotNetCliToolReference Include="Microsoft.EntityFrameworkCore.Tools.DotNet" Version="2.0.0" />
    </ItemGroup>
    ```

    - Run `dotnet restore` to install the new packages.

4. Add a data context to the Web API project and use data migrations to create the database.
    - Add a reference to the **Entities.WebApi** project.
    - Add a `NorthwindSlimContext` class to that extends `DbContext`.
    - Add a constructor that accepts `DbContextOptions`.
    - Override `OnConfiguring` to use SqLite.

    ```csharp
    public class NorthwindSlimContext : DbContext
    {
	public NorthwindSlimContext(DbContextOptions<NorthwindSlimContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Product> Products { get; set; }
    }
    ```

    - Add a NorthwindSlimContextFactory class to the Web project.

    ```csharp
    public class NorthwindSlimContextFactory : IDesignTimeDbContextFactory<NorthwindSlimContext>
    {
        public NorthwindSlimContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NorthwindSlimContext>();
            optionsBuilder.UseSqlite("Data Source=northwindslim.db");
            return new NorthwindSlimContext(optionsBuilder.Options);
        }
    }
    ```

    - Run `dotnet ef migrations add initial` to scaffold a migration and create the initial set of tables for the model.
    - Run `dotnet ef database update` to apply the migration and create the database.
        + A _northwindslim.db_ file will appear in the project directory.

## Web API Controllers

5. Install Trackable Entities for EF Core and add Web API controllers with GET, POST, PUT and DELETE actions.
    - Add the package (prerelease): **TrackableEntities.EF.Core**.
    - Add a `CustomerController` class to the _Controllers_ folder.

    ```csharp
    [Produces("application/json")]
    [Route("api/Customer")]
    public class CustomerController : Controller
    {
        private readonly NorthwindSlimContext _context;

        public CustomerController(NorthwindSlimContext context)
        {
            _context = context;
        }

        // GET: api/Customer
        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _context.Customers
                .ToListAsync();
            return Ok(customers);
        }

        // GET: api/Customer/ALFKI
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = await _context.Customers.SingleOrDefaultAsync(m => m.CustomerId == id);

            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }
    }
    ```

    - Add an `OrderController` class to the _Controllers_ folder.

    ```csharp
    [Produces("application/json")]
    [Route("api/Order")]
    public class OrderController : Controller
    {
        private readonly NorthwindSlimContext _context;

        public OrderController(NorthwindSlimContext context)
        {
            _context = context;
        }

        // GET: api/Order
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.Orders
                .Include(m => m.Customer)
                .Include(m => m.OrderDetails)
                .ThenInclude(m => m.Product)
                .ToListAsync();
            return Ok(orders);
        }

        // GET: api/Order/ALFKI
        [HttpGet("{customerId:alpha}")]
        public async Task<IActionResult> GetOrders([FromRoute] string customerId)
        {
            var orders = await _context.Orders
                .Include(m => m.Customer)
                .Include(m => m.OrderDetails)
                .ThenInclude(m => m.Product)
                .Where(m => m.CustomerId == customerId)
                .ToListAsync();
            return Ok(orders);
        }

        // GET: api/Order/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var order = await _context.Orders
                .Include(m => m.Customer)
                .Include(m => m.OrderDetails)
                .ThenInclude(m => m.Product)
                .SingleOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        // PUT: api/Order
        [HttpPut]
        public async Task<IActionResult> PutOrder([FromBody] Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Apply changes to context
            _context.ApplyChanges(order);

            try
            {
                // Persist changes
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Orders.Any(o => o.OrderId == order.OrderId))
                {
                    return NotFound();
                }
                throw;
            }

            // Populate reference properties
            await _context.LoadRelatedEntitiesAsync(order);

            // Reset tracking state to unchanged
            _context.AcceptChanges(order);

            //return NoContent();
            return Ok(order);
        }

        // POST: api/Order
        [HttpPost]
        public async Task<IActionResult> PostOrder([FromBody] Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Set state to added
            order.TrackingState = TrackingState.Added;

            // Apply changes to context
            _context.ApplyChanges(order);

            // Persist changes
            await _context.SaveChangesAsync();

            // Populate reference properties
            await _context.LoadRelatedEntitiesAsync(order);

            // Reset tracking state to unchanged
            _context.AcceptChanges(order);

            return CreatedAtAction("GetOrder", new { id = order.OrderId }, order);
        }

        // DELETE: api/Order/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Retrieve order with details
            var order = await _context.Orders
                .Include(m => m.OrderDetails)
                .SingleOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            // Set tracking state to deleted
            order.TrackingState = TrackingState.Deleted;

            // Detach object graph
            _context.DetachEntities(order);

            // Apply changes to context
            _context.ApplyChanges(order);

            // Persist changes
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
    ```

## Configure Web API to use EF Core

6. Register the `DbContext` with dependency injection in the Web API project.
    - Add an import to the `Startup` class: `using Microsoft.EntityFrameworkCore`.
    - Add code to the `ConfigureServices` method for using Sqlite.
    - Configure the JSON serializer to handle cyclical references.

    ```csharp
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc().AddJsonOptions(
            options => options.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.All);
        services.AddDbContext<NorthwindSlimContext>(
            options => options.UseSqlite("Data Source=northwindslim.db"));
    }
    ```

7. Seed the database with intitial data.
    - Add a `NorthwindSlimContextExtensions` class with an `EnsureSeedData` extension method.
    - See class in repository for complete code.

    ```csharp
    public static class NorthwindSlimContextExtensions
    {
        public static void EnsureSeedData(this NorthwindSlimContext context)
        {
            context.Database.OpenConnection();
            try
            {
                if (!context.Categories.Any())
                {
                    AddCategories(context);
                    context.SaveChanges();
                }

                if (!context.Products.Any())
                {
                    AddProducts(context);
                    context.SaveChanges();
                }

                if (!context.Customers.Any())
                {
                    AddCustomers(context);
                    context.SaveChanges();
                }

                if (!context.Orders.Any())
                {
                    AddOrders(context);
                    context.SaveChanges();
                }
            }
            finally
            {
                context.Database.CloseConnection();
            }
        }
    }
    ```

    - Update the `Configure` method in `Startup` to call `context.EnsureSeedData` when in dev mode.

    ```csharp
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, NorthwindSlimContext context)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            context.EnsureSeedData();
        }

        app.UseMvc();
    }
    ```

    - Test the service by running it and requesting customers and orders.


    ```bash
    dotnet run
    ```

## Client Entities and Console Application

8. Generate _client-side_ trackable entities in a .NET Standard class library project.
    - Add a .NET Standard class library project to the solution.
    - Install the package: **TrackableEntities.Client**

    > **Note**: It is possible to use packages built for earlier version of .NET because .NET Standard 2.0 and Core 2.0 apps share a common API surface.

    - Add classes that extend `EntityBase` and use `ChangeTrackingCollection` for reference and collection propeties.
    - Property setters need to call `NotifyPropetyChanged`.

    > **Note**: You can generate these classes from an existing database using a Windows .NET class library and then copy the classes over to the cross-platform project.

    ```csharp
    public partial class Product : EntityBase
    {
        public Product()
        {
            OrderDetails = new ChangeTrackingCollection<OrderDetail>();
        }

		public int ProductId
		{ 
			get { return _ProductId; }
			set
			{
				if (Equals(value, _ProductId)) return;
				_ProductId = value;
				NotifyPropertyChanged();
			}
		}
		private int _ProductId;

		public string ProductName
		{ 
			get { return _ProductName; }
			set
			{
				if (Equals(value, _ProductName)) return;
				_ProductName = value;
				NotifyPropertyChanged();
			}
		}
		private string _ProductName;

		public int? CategoryId
		{ 
			get { return _CategoryId; }
			set
			{
				if (Equals(value, _CategoryId)) return;
				_CategoryId = value;
				NotifyPropertyChanged();
			}
		}
		private int? _CategoryId;

		public decimal? UnitPrice
		{ 
			get { return _UnitPrice; }
			set
			{
				if (Equals(value, _UnitPrice)) return;
				_UnitPrice = value;
				NotifyPropertyChanged();
			}
		}
		private decimal? _UnitPrice;

		public bool Discontinued
		{ 
			get { return _Discontinued; }
			set
			{
				if (Equals(value, _Discontinued)) return;
				_Discontinued = value;
				NotifyPropertyChanged();
			}
		}
		private bool _Discontinued;

		public byte[] RowVersion
		{ 
			get { return _RowVersion; }
			set
			{
				if (Equals(value, _RowVersion)) return;
				_RowVersion = value;
				NotifyPropertyChanged();
			}
		}
		private byte[] _RowVersion;


		public Category Category
		{
			get { return _Category; }
			set
			{
				if (Equals(value, _Category)) return;
				_Category = value;
				CategoryChangeTracker = _Category == null ? null
					: new ChangeTrackingCollection<Category> { _Category };
				NotifyPropertyChanged();
			}
		}
		private Category _Category;
		private ChangeTrackingCollection<Category> CategoryChangeTracker { get; set; }

		public ChangeTrackingCollection<OrderDetail> OrderDetails
		{
			get { return _OrderDetails; }
			set
			{
				if (Equals(value, _OrderDetails)) return;
				_OrderDetails = value;
				NotifyPropertyChanged();
			}
		}
		private ChangeTrackingCollection<OrderDetail> _OrderDetails;
    }
    ```

9. Add a .NET Core console application that uses the **TrackableEntities.Client** NuGet package to perform client-side change tracking, sending object graphs of changed entities to the Web API service where they are saved to the database in a single transaction.
    - Install the package: **TrackableEntities.Client**
    - Install the package: **System.Net.Http**
    - Install the package: **System.Net.Http.Formatting**
    - Add a reference to the **Entities.Client** project

    > **Note**: Complete code for the client app can be found in the ConsoleClient project of the provided solution.

    - Add private helper methods.
    - Add code to retrieve and update entities.
        + Retrieve customers
        + Retrieve customer orders
        + Create an order with details
        + Update an existing order with unchanged, added, modified and deleted details
        + Delete an order and verify that it was deleted

    > **Note**: When you run the console client you may receive NU1701 warnings stating that certain packages may not be fully compatible.  These warnings can be safely ignored because .NET 4.6.1 is generally compatible with .NET Standard 2.0.

