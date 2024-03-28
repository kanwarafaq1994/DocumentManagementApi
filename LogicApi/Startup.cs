
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Linq;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;
using DocumentManagement.Data.Common;
using DocumentManagement.Data;

namespace LogicAPI
{
    public class Startup
    {

        private readonly string corsPolicy = "CorsDocumentApi";
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();
            BuildAppSettingsProvider();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            try
            {

                var connectionString = Configuration.GetValue<string>("ConnectionStrings:DocumentManagement");

                services.AddDistributedMemoryCache();

                services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromHours(10);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                });

                services.AddHttpContextAccessor();
                services.AddDbContext<DocumentManagement.Data.DocumentManagementContext>(options => options.UseSqlServer(connectionString, x => x.MigrationsAssembly("DocumentManagement.Data")));
                //services.AddScoped<IEmailRepository, EmailRepository>();
                //services.AddScoped<IUserRepository, UserRepository>();
                //services.AddScoped<IOrganizationRepository, OrganizationRepository>();
                //services.AddScoped<IUnitOfWork, UnitOfWork>();
                //services.AddScoped<IReportRepository, ReportRepository>();
                //services.AddScoped<ICountyCourtsRepository, CountyCourtsRepository>();
                //services.AddScoped<IExpressionOfInterestInterfaceRepository, ExpressionOfInterestInterfaceRepository>();
                //services.AddScoped<IBicRepository, BicRepository>();
                //services.AddScoped<IZipCodeRepository, ZipCodeRepository>();
                //services.AddScoped<AttachContext>();
                services.AddScoped<IAuthContext, AuthContext>();
                services.AddSingleton<HtmlEncoder>(HtmlEncoder.Create(allowedRanges: new[]
                {
                    UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs
                }));
                //AddFilterAttributes(services);

                services
                    .AddControllers()
                    .AddNewtonsoftJson(options => {
                        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    });

                AddSwaggerConfig(services);

                var allowedCorsResources = Configuration.GetSection("corsAllowedOrigins").AsEnumerable().Select(pair => pair.Value).Where(v => v != null).ToArray();
                services.AddCors(confg =>
                {
                    confg.AddPolicy("AllowAll", p =>
                        p.AllowAnyOrigin().
                          AllowAnyMethod().
                          AllowAnyHeader().
                          WithExposedHeaders("*"));

                    confg.AddPolicy(corsPolicy, p =>
                        p.WithOrigins(allowedCorsResources)
                        .AllowCredentials()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                    );
                }
                 );

                JWTAuthentificationConfig(services); //Configure JWT Authentication

               
                // Validators 
                //services.AddScoped<IDocumentValidator, DocumentValidator>();

                services.AddMvc().AddNewtonsoftJson();
            }
            catch (Exception x)
            {
            }
        }

        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("v1/swagger.json", "Document Managament API V1");
                });
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(corsPolicy);
            app.UseSession();

            //app.UseMiddleware<LoggingMiddleware>();

            //app.UseWhen(context => context.Request.Path.StartsWithSegments("/api")
            //                       && !(context.Request.RouteValues["controller"].Equals("Account")
            //                       || context.Request.RouteValues["controller"].Equals("Password")
            //                       || (context.Request.RouteValues["controller"].Equals("Resource")
            //                       && context.Request.RouteValues["action"].Equals("GetCurrent"))), applicationBuilder =>
            //                       {
            //                           applicationBuilder.UseMiddleware<ActivityMiddleware>();
            //                       });
            //var whiteListedEndPoints = Configuration.GetSection("WhiteListedEndPoints:EndPoints").Get<List<string>>();
            //app.UseWhen(ctx => (ctx.Request.Method == "PUT" || ctx.Request.Method == "POST")
            //                   && !whiteListedEndPoints.Contains(ctx.Request.Path.Value)
            //                   && !(ctx.Request.ContentType?.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0), appBuilder =>
            //                   {
            //                       appBuilder.UseAntiXssMiddleware();
            //                   });
            //HttpHelper.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
            app.UseAuthorization();
            app.UseStatusCodePagesWithReExecute("/error/{0}");
            //app.UseHttpException();

            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void BuildAppSettingsProvider()
        {
            var appSettingsSection = Configuration.GetSection("AppSettings");
            var appSettings = appSettingsSection.Get<AppSettings>();
            JwtTokenGenerator.SecretKey = appSettings.PublicDocumentSecret;
        }
        public void AddSwaggerConfig(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Seal Document Management API",
                    Description = "This Api is use for testing purpse only",
                    TermsOfService = new Uri("https://www.swaggger.com/"),
                    Contact = new OpenApiContact { Name = "Kanwar", Email = "test@test.de" },
                    License = new OpenApiLicense { Name = "Document Management Open License", Url = new Uri("https://www.swagger.com/"), }
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "Bearer"}
                        }, new string[] { }
                    }
                });
            });
        }

        public void JWTAuthentificationConfig(IServiceCollection services)
        {
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = System.Text.Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    };
                });
        }

    }
}
