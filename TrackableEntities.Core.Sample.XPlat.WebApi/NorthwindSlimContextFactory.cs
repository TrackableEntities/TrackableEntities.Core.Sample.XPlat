using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TrackableEntities.Core.Sample.XPlat.WebApi
{
	public class NorthwindSlimContextFactory : IDesignTimeDbContextFactory<NorthwindSlimContext>
	{
		public NorthwindSlimContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<NorthwindSlimContext>();
			optionsBuilder.UseSqlite("Data Source=northwindslim.db");
			return new NorthwindSlimContext(optionsBuilder.Options);
		}
	}
}
