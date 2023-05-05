using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Reflection;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using WebApiProveedorPagos.Repository.IRepository;
using WebApiProveedorPagos.Data;
using WebApiProveedorPagos.Repository;
using Microsoft.OpenApi.Models;
using WebApiProveedorPagos.helpers;

namespace WebApiProveedorPagos
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
            services.AddDbContext<ApplicationDbContext>(Options => Options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            
            
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();

            /*Agregar dependencia del token*/
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero

                    };
                });

           services.AddAutoMapper(typeof(UsuariosMappers));

            //De aquí en adelante configuración de documentación de nuestra API
            services.AddSwaggerGen(options =>
            {
                

                options.SwaggerDoc("ApiUsuarios", new Microsoft.OpenApi.Models.OpenApiInfo()
                {
                    Title = "API Usuarios",
                    Version = "1",
                    Description = "API Usuarios",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                    {
                        Email = "admin@serviciopostal.gob.ec",
                        Name = "Servicios Postales del Ecuador SPE E.P.",
                        Url = new Uri("https://www.serviciopostal.gob.ec/")
                    },
                    License = new Microsoft.OpenApi.Models.OpenApiLicense()
                    {
                        Name = "MIT License",
                        Url = new Uri("https://en.wikipedia.org/wiki/MIT_License")
                    }
                });
                
                options.SwaggerDoc("ApiEnvios", new Microsoft.OpenApi.Models.OpenApiInfo()
                {
                    Title = "API Envios",
                    Version = "1",
                    Description = "Envios",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                    {
                        Email = "admin@serviciopostal.gob.ec",
                        Name = "Servicios Postales del Ecuador SPE E.P.",
                        Url = new Uri("https://www.serviciopostal.gob.ec/")
                    },
                    License = new Microsoft.OpenApi.Models.OpenApiLicense()
                    {
                        Name = "MIT License",
                        Url = new Uri("https://en.wikipedia.org/wiki/MIT_License")
                    }
                });
                
                
                var archivoXmlComentarios = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var rutaApiComentarios = Path.Combine(AppContext.BaseDirectory, archivoXmlComentarios);
                options.IncludeXmlComments(rutaApiComentarios);

                //Primero definir el esquema de seguridad
                options.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        Description = "Autenticación JWT (Bearer)",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer"
                    });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement{
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        }, new List<string>()
                    }
                  });


            });


            services.AddControllers();
            /*Damos soporte para CORS*/
            services.AddCors();
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
                app.UseExceptionHandler(builder =>
                {
                    builder.Run(async context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        var error = context.Features.Get<IExceptionHandlerFeature>();

                        if (error != null)
                        {
                            context.Response.AddApplicationError(error.Error.Message);
                            await context.Response.WriteAsync(error.Error.Message);
                        }
                    });
                });
            }

            app.UseHttpsRedirection();
            //Línea para documentación api
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
              
                options.SwaggerEndpoint("/swagger/ApiUsuarios/swagger.json", "API Usuarios");
                options.SwaggerEndpoint("/swagger/ApiEnvios/swagger.json", "API Envios");
                options.RoutePrefix = "";
            });

            app.UseRouting();

            /*Estos dos son para la autenticación y autorización*/
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            /*Damos soporte para CORS - Permitir conexiones de cualquier dominio*/
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        }
    
        
    }
}
