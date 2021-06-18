using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace ReferenceBot
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
            services
                .AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            services
                .AddApiVersioning(o =>
                {
                    o.ReportApiVersions = true;
                    o.DefaultApiVersion = new ApiVersion(1, 0);
                    o.ApiVersionReader = new UrlSegmentApiVersionReader();
                })
                .AddVersionedApiExplorer(options =>
                {
                    options.SubstituteApiVersionInUrl = true;
                });

            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                c.TagActionsBy(api =>
                {
                    if (!(api.ActionDescriptor is ControllerActionDescriptor actionDescriptor)) throw new InvalidOperationException("Unable to determine tag for endpoint.");
                    var tag = (DescriptionAttribute)Attribute.GetCustomAttribute(actionDescriptor.MethodInfo, typeof(DescriptionAttribute));
                    if (tag != null) return new[] { tag.Description };
                    return new[] { actionDescriptor.ControllerName };
                });
                c.OrderActionsBy(x => x.RelativePath);

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Reference Bot",
                    Version = "v1",
                    Description = "See [Bot API version 1](https://github.com/magnuswiderberg/rpsbots/wiki/Bot-API-version-1.0)"
                });

                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var actionApiVersionModel = apiDesc.ActionDescriptor.Properties.Where((kvp) => (Type)kvp.Key == typeof(ApiVersionModel)).Select(kvp => kvp.Value as ApiVersionModel).FirstOrDefault();

                    // would mean this action is unversioned and should be included everywhere
                    if (actionApiVersionModel == null) return true;
                    if (actionApiVersionModel.DeclaredApiVersions.Any()) return actionApiVersionModel.DeclaredApiVersions.Any(v => $"v{v.MajorVersion}" == docName);
                    return actionApiVersionModel.ImplementedApiVersions.Any(v => $"v{v.MajorVersion}" == docName);
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ReferenceBot v1");
            });
        }
    }
}
