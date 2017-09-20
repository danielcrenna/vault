using System.IO;
using hq.chassis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;

namespace NaiveCoin
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHq();

            Dependencies.Register(services, Configuration);
            
            services.AddCors(o =>
            {
                o.AddPolicy("DefaultPolicy", builder => builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());

                o.DefaultPolicyName = "DefaultPolicy";
            });

            if (!Environment.IsEnvironment("InteractionTest"))
            {
                services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new Info
                    {
                        Version = "v1",
                        Title = "NaiveCoin",
                        Description = "A cryptocurrency implementation in less than 1500 lines of code.",
                        TermsOfService = "https://github.com/danielcrenna/naivecoin/LICENSE"
                    });
                    options.IncludeXmlComments(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, $"{PlatformServices.Default.Application.ApplicationName}.xml"));
                    options.DescribeAllEnumsAsStrings();
                });
            }
           
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseHq();

            app.UseCors("DefaultPolicy");

            if (!env.IsEnvironment("InteractionTest"))
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NaiveCoin");
                });
            }
           
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
