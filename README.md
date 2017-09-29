# Trackable Entities for EF Core Cross Platform Sample

Sample solution using Trackable Entities with ASP.NET Core and Entity Framework Core with SQLite.

> **Note**: This solution was created using [Visual Studio for Mac](https://www.visualstudio.com/vs/visual-studio-mac/), but you can also use [Visual Studio for Windows](https://www.visualstudio.com/vs/community/) or [Visual Studio Code](https://code.visualstudio.com/).

## Prerequisites

- [.NET Core SDK](https://www.microsoft.com/net/download/core) 2.0 or greater.

## Steps

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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=northwindslim.db");
        }

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

8. Generate _client-side_ trackable entities in a traditional class library project.

9. Add a traditional .NET client console application that uses the **TrackableEntities.Client** NuGet package to perform client-side change tracking, sending object graphs of changed entities to the Web API service where they are saved to the database in a single transaction.

