using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EventFlow;
using EventFlow.AspNetCore.Extensions;
using EventFlow.AspNetCore.MetadataProviders;
using EventFlow.Extensions;
using EventFlow.MsSql;
using EventFlow.MsSql.Extensions;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankOfDotNet.Identity
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            //var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("appSettings.json", false)
            //    .Build();

            //string connectionString = config.GetSection("connectionString").Value;
            //string connectionString = Configuration.GetValue<string>("connectionString");
            string connectionString = Configuration.GetValue<string>("connectionString");
            var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            var container = EventFlowOptions.New
                .AddAspNetCore().AddMetadataProvider<AddUserClaimsMetadataProvider>()
                .ConfigureMsSql(MsSqlConfiguration.New.SetConnectionString(connectionString));

            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                //.AddInMemoryIdentityResources(Config.GetIdentityResources())
                //.AddInMemoryApiResources(Config.GetAllApiResources())
                //.AddInMemoryClients(Config.GetClients())
                .AddTestUsers(Config.GetUsers())
                //Configuration Store : clients and resources
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationAssembly));
                })
                //Operational Store: tokens, consents, codes, etc
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b =>
                    b.UseSqlServer(connectionString,
                    sql => sql.MigrationsAssembly(migrationAssembly));
                });
           
            services.AddAuthentication();
            container.RegisterServices(s =>
            {
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //InitializeIdentityServerDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }

       

        private void InitializeIdentityServerDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                //serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                //context.Database.Migrate();

                //Seed the data
                //Seed the clients
                if (context.Clients.Any())
                {
                    foreach (var client in Config.GetClients().Where(c => c.ClientId == "swaggerapiui"))
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                //if (!context.Clients.Any())
                //{
                //    foreach (var client in Config.GetClients())
                //    {
                //        context.Clients.Add(client.ToEntity());
                //    }
                //    context.SaveChanges();
                //}

                //Seed the identity resources
                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                //Seed the api resources
                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.GetAllApiResources())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}
