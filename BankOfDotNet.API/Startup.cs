using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BankOfDotNet.API.Models;
using EventFlow;
using EventFlow.AspNetCore.Extensions;
using EventFlow.AspNetCore.MetadataProviders;
using EventFlow.Extensions;
using EventFlow.MsSql;
using EventFlow.MsSql.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BankOfDotNet.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            //var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
            //   .AddJsonFile("appSettings.json", false)
            //   .Build();

            //string connectionString = config.GetSection("connectionString").Value;

            string connectionString = Configuration.GetValue<string>("connectionString");
           
            var container = EventFlowOptions.New
                .AddAspNetCore().AddMetadataProvider<AddUserClaimsMetadataProvider>()
                .ConfigureMsSql(MsSqlConfiguration.New.SetConnectionString(connectionString));

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "bankOfDotNetApi";
                });

            services.AddDbContext<BankContext>(opts =>
             opts.UseSqlServer(connectionString, e =>
           {
               e.MigrationsAssembly(typeof(BankContext).GetTypeInfo().Assembly.GetName().Name);
           })
                       );

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info { Title = "BankOfDotNet API", Version = "v1" });
                options.OperationFilter<CheckAuthorizeOperationFilter>();
                options.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = "http://localhost:5000/connect/authorize",
                    TokenUrl = "http://localhost:5000/connect/token",
                    Scopes = new Dictionary<string, string>()
                    {
                        {
                            "bankOfDotNetApi" , "Customer API for bankOfDotNet"
                        }
                    }
                });
            });

            container.RegisterServices(s =>
            {
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

           

            app.UseAuthentication();
            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "BankOfDotNet API v1");
                options.OAuthClientId("swaggerapiui");
                options.OAuthAppName("Swagger API UI");
            });
        }
    }

    internal class CheckAuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            //Check for any existing Authorize attribute
            var authorizeAttributeExists = context.ApiDescription.ControllerAttributes().OfType<AuthorizeAttribute>().Any()
                || context.ApiDescription.ActionAttributes().OfType<AuthorizeAttribute>().Any();

            if (authorizeAttributeExists)
            {
                operation.Responses.Add("401", new Response { Description = "Unauthorized" });
                operation.Responses.Add("403", new Response { Description = "Forbidden" });
                operation.Security = new List<IDictionary<string, IEnumerable<string>>>();
                operation.Security.Add(new Dictionary<string, IEnumerable<string>>
                {
                    { "oauth2", new [] { "bankOfDotNetApi" } }
                }
                );
            }
        }
    }


}
