using System.IO;
using System.Linq;
using System.Reflection;
using DbUp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Service.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            var assembly = Assembly.GetEntryAssembly();
            var connectionString = Configuration.GetConnectionString("Default");
            
            EnsureDatabase.For.SqlDatabase(connectionString);
            DeployChanges.To.SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssemblies(
                    Directory.GetFiles(Path
                        .GetDirectoryName(assembly.Location))
                        .Where(f => Path.GetExtension(f).Equals(".dll") && 
                                    Path.GetFileNameWithoutExtension(f).EndsWith(".Migrations"))
                        .Select(Assembly.LoadFrom)
                        .Union(new[] { assembly })
                        .ToArray())
                .Build()
                .PerformUpgrade();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}