﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microservices.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;

namespace Microservices
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
            services.AddDbContext<EventDbContext>(options =>
            {
                //options.UseInMemoryDatabase(databaseName: "EventDb");
                options.UseSqlServer(Configuration.GetConnectionString("EventSqlConnection"));
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Event API",
                    Version = "v1",
                    Contact = new Contact { Name = "Ravindra Gojare", Email = "ravi.gojare@hotmail.com" }
                });
            });

            services.AddCors(c =>
            {
                c.AddPolicy("MSPolicy", builder =>
                {
                    builder.WithOrigins("*.microsoft.com")
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
                c.AddPolicy("SynPolicy", builder =>
                {
                    builder.WithOrigins("*.synergetics-india.com")
                    .WithMethods("GET")
                    .WithHeaders("Authorization", "Content-Type", "Accept");
                });
                c.AddPolicy("Others", builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
                c.DefaultPolicyName = "Others";
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options=> {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration.GetValue<string>("Jwt:Issuer"),
                        ValidAudience = Configuration.GetValue<string>("Jwt:Audience"),
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("Jwt:Secret")))
                    };
                });

            services.AddMvc(c=> 
            {
                c.Filters.Add(typeof(CustomExceptionHandler));
                //c.OutputFormatters.Add();
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("Others");
            //app.UseCors(c =>
            //{
            //    c.WithOrigins("*.microsoft.com")
            //    .AllowAnyMethod()
            //    .AllowAnyHeader();

            //    c.WithOrigins("*.synergetics-india.com")
            //    .WithMethods("GET")
            //    .WithHeaders("Authorization", "Content-Type", "Accept");
            //});

            app.UseSwagger();

            if (env.IsDevelopment())
            {
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event API");
                });
            }

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
