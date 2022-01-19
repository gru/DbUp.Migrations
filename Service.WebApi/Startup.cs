#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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

            var assembly = Assembly.GetEntryAssembly()!;
            var connectionString = Configuration.GetConnectionString("Default");

            EnsureDatabase.For.SqlDatabase(connectionString);
            DeployChanges.To.SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssemblies(
                    Directory.GetFiles(Path
                            .GetDirectoryName(assembly.Location)!)
                        .Where(f => Path.GetExtension(f).Equals(".dll") && 
                                    Path.GetFileNameWithoutExtension(f).EndsWith(".Migrations"))
                        .Select(Assembly.LoadFrom)
                        .Union(new[] { assembly })
                        .ToArray())
                .WithScriptNameComparer(new ScriptNameComparer())
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

    public class ScriptNameComparer : IComparer<string>
    {
        private readonly Regex _regex = new Regex(".(?<date>[0-9]+)_");
        
        public int Compare(string? x, string? y)
        {
            if (x == null) throw new NullReferenceException(nameof(x));
            if (y == null) throw new NullReferenceException(nameof(y));

            var xd = _regex.Match(x).Groups["date"].Value;
            var yd = _regex.Match(y).Groups["date"].Value;
            return StringComparer.OrdinalIgnoreCase.Compare(xd, yd);
        }
    }
}