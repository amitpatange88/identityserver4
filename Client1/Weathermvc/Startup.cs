using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Okta.AspNetCore;
using Weathermvc.Services;
using WeatherMVC.Services;

namespace Weathermvc
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
            services.AddControllersWithViews();

            AddOpenIDConnectLayer(services);

            //AddOktaClientLayer(services);
        }

        private void AddOktaClientLayer(IServiceCollection services)
        {
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
            })
                        .AddAuthentication(options =>
                        {
                            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                        })
                       .AddCookie()
                       .AddOktaMvc(new OktaMvcOptions
                       {
                           // Replace these values with your Okta configuration
                           OktaDomain = Configuration.GetValue<string>("Okta:OktaDomain"),
                           AuthorizationServerId = Configuration.GetValue<string>("Okta:AuthorizationServerId"),
                           ClientId = Configuration.GetValue<string>("Okta:ClientId"),
                           ClientSecret = Configuration.GetValue<string>("Okta:ClientSecret"),
                           Scope = new List<string> { "openid", "profile", "email" },
                       });

            services.AddAuthorization();
        }

        private void AddOpenIDConnectLayer(IServiceCollection services)
        {
            services.AddAuthentication(configureOptions: options =>
            {
                options.DefaultScheme = "cookie";
                options.DefaultChallengeScheme = "oidc";
            })
                        .AddCookie("cookie")
                        .AddOpenIdConnect(authenticationScheme: "oidc", configureOptions: options =>
                        {
                            options.Authority = Configuration["InteractiveServiceSettings:AuthorityUrl"];
                            options.ClientId = Configuration["InteractiveServiceSettings:ClientId"];
                            options.ClientSecret = Configuration["InteractiveServiceSettings:ClientSecret"];

                            options.ResponseType = "code";
                            options.UsePkce = true;
                            options.ResponseMode = "query";

                            options.Scope.Add(Configuration["InteractiveServiceSettings:Scopes:0"]);
                            options.SaveTokens = true;
                        });

            services.Configure<IdentityServerSettings>(Configuration.GetSection(key: "IdentityServerSettings"));
            services.AddSingleton<ITokenService, TokenService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
