using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Infrastructure.MultiTenancy.Extension;
using Broker.ApplicationExtension.Extension;
using Broker.Persistence.Extension;
using AHI.Infrastructure.MultiTenancy.Middleware;
using AHI.Infrastructure.UserContext;
using AHI.Infrastructure.UserContext.Extension;
using AHI.Infrastructure.Validation.Extension;
using AHI.Infrastructure.Exception.Filter;
using Prometheus;
using Prometheus.SystemMetrics;
namespace Broker.Api
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
            // Add application service from Application layer
            services.AddApplicationServices();
            // Add persistence service
            services.AddPersistenceService();
            services.AddMultiTenantService();
            services.AddDynamicValidation();
            services.AddUserContextService();
            services.AddControllers(config =>
            {
                config.ExceptionHandling();
            }).AddNewtonsoftJson(option =>
            {
                option.SerializerSettings.NullValueHandling = Constant.JsonSerializerSetting.NullValueHandling;
                option.SerializerSettings.DateFormatString = Constant.JsonSerializerSetting.DateFormatString;
                option.SerializerSettings.ReferenceLoopHandling = Constant.JsonSerializerSetting.ReferenceLoopHandling;
                option.SerializerSettings.DateParseHandling = Constant.JsonSerializerSetting.DateParseHandling;
            });
            services.AddAuthentication()
            .AddIdentityServerAuthentication("oidc",
                jwtTokenOption =>
                {
                    jwtTokenOption.Authority = Configuration["Authentication:Authority"];
                    jwtTokenOption.RequireHttpsMetadata = Configuration["Authentication:Authority"].StartsWith("https");
                    jwtTokenOption.TokenValidationParameters.ValidateAudience = false;
                    jwtTokenOption.ClaimsIssuer = Configuration["Authentication:Issuer"];
                }, referenceTokenOption =>
                {
                    referenceTokenOption.IntrospectionEndpoint = Configuration["Authentication:IntrospectionEndpoint"];
                    referenceTokenOption.ClientId = Configuration["Authentication:ApiScopeName"];
                    referenceTokenOption.ClientSecret = Configuration["Authentication:ApiScopeSecret"];
                    referenceTokenOption.ClaimsIssuer = Configuration["Authentication:Issuer"];
                    referenceTokenOption.SaveToken = true;
                });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", Configuration["Authentication:ApiScopeName"]);
                });
            });
            //services.AddApplicationInsightsTelemetry();
            services.AddSystemMetrics();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseHttpMetrics();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseWhen(
            context => !(context.Request.Path.HasValue && context.Request.Path.Value.StartsWith("/metrics")),
            builder =>
            {
                builder.UseMiddleware<MultiTenancyMiddleware>();
                builder.UseMiddleware<UserContextMiddleware>();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMetrics();
                endpoints.MapControllers()
                        .RequireAuthorization("ApiScope");
            });
        }
    }
}
