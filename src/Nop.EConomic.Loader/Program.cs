using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

using Newtonsoft.Json;

using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Migrations;

namespace Nop.EConomic.Loader
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var dir = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\Presentation\Nop.Web");
            var cat = 17; // "External products"

            var repo = ConfigureRepository(dir, cat);

            using var httpClient = new HttpClient();

            DateTime? lastTime = null; // TODO
            var thisTime = DateTime.UtcNow;

            // TODO in reverse order ??

            var url = MakeUri(lastTime, thisTime, 0);
            while (url != null)
            {
                var str = await httpClient.GetStringAsync(url);
                var page = JsonConvert.DeserializeObject<ProductPage>(str);

                foreach (var item in page.Collection)
                {
                    await repo.AddProduct(item);
                }

                url = page.Pagination.NextPage;
            }
        }

        private static ProductsRepository ConfigureRepository(string directory, int category)
        {
            var dataProvider = DataProviderManager.GetDataProvider(DataProviderType.PostgreSQL);

            var env = new WebHostEnvironment(directory);
            var fileProvider = new NopFileProvider(env);
            CommonHelper.DefaultFileProvider = fileProvider;

            Singleton<AppSettings>.Instance = new AppSettings();

            var services = new ServiceCollection()
                .AddSingleton(Singleton<AppSettings>.Instance)
                .AddSingleton<ITypeFinder, AppDomainTypeFinder>();

            services
                .AddFluentMigratorCore()
                .AddScoped<IProcessorAccessor, NopProcessorAccessor>()
                .AddScoped<IMigrationManager>(s => new MigrationManager(null, null, null,
                    s.GetRequiredService<IMigrationContext>(),
                    s.GetRequiredService<ITypeFinder>()))
                .ConfigureRunner(rb => rb
                    .WithVersionTable(new MigrationVersionInfo())
                    .AddPostgres());

            var provider = services.BuildServiceProvider();
            EngineContext.Replace(new Engine(provider));

            return new ProductsRepository(dataProvider, fileProvider, category);
        }

        private sealed class WebHostEnvironment : IWebHostEnvironment
        {
            public string WebRootPath { get; set; }
            public IFileProvider WebRootFileProvider { get; set; }
            public IFileProvider ContentRootFileProvider { get; set; }
            public string ContentRootPath { get; set; }
            public string ApplicationName { get; set; }
            public string EnvironmentName { get; set; }

            public WebHostEnvironment(string rootPath)
            {
                WebRootPath = Path.Combine(rootPath, "wwwroot");
                WebRootFileProvider = new PhysicalFileProvider(WebRootPath);

                ContentRootPath = rootPath;
                ContentRootFileProvider = new PhysicalFileProvider(ContentRootPath);
            }
        }

        private sealed class Engine : NopEngine
        {
            public Engine(IServiceProvider prov)
            {
                ServiceProvider = prov;
            }
        }

        private static string MakeUri(DateTime? from, DateTime to, int skipPages, int pageSize = 20)
        {
            static string Iso(DateTime d) => d.ToUniversalTime().ToString("s") + "Z";

            var filter = $"lastUpdated$lt:{Iso(to)}";
            if (from.HasValue)
                filter = $"lastUpdated$gte:{Iso(from.Value)}$and:{filter}";

            var uri = $"/products?demo=true&filter={filter}&pagesize={pageSize}&skippages={skipPages}";
            return "https://restapi.e-conomic.com" + uri;
        }
    }

    public class ProductPage
    {
        public ProductItem[] Collection { get; set; }
        public PaginationInfo Pagination { get; set; }
    }

    public class ProductItem
    {
        public string ProductNumber { get; set; } // use for sku
        public string Description { get; set; }
        public string Name { get; set; }

        // TODO which price?
        public double CostPrice { get; set; }
        public double RecommendedPrice { get; set; }
        public double SalesPrice { get; set; }

        public DateTime LastUpdated { get; set; } // TODO or string ?
        public object ProductGroup { get; set; } // not sure if we need
        public object Inventory { get; set; } // counts - but all=0 everywhere ?
    }

    public class PaginationInfo
    {
        public int Results { get; set; }

        public string PreviousPage { get; set; }
        public string NextPage { get; set; }
    }
}
