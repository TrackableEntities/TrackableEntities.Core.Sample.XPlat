using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace TrackableEntities.Core.Sample.XPlat.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddJsonOptions(
                options => options.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.All);
            services.AddDbContext<NorthwindSlimContext>(
                options => options.UseSqlite("Data Source=northwindslim.db"));
        }

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, NorthwindSlimContext context)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				context.EnsureSeedData();
			}

			app.UseMvc();
		}
    }
}
