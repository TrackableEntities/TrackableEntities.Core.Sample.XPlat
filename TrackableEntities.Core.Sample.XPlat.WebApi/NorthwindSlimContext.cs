using Microsoft.EntityFrameworkCore;
using NetCoreSample.Entities.WebApi;

namespace TrackableEntities.Core.Sample.XPlat.WebApi
{
    public class NorthwindSlimContext : DbContext
    {
		public NorthwindSlimContext(DbContextOptions<NorthwindSlimContext> options) : base(options) { }
		
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Product> Products { get; set; }
    }
}