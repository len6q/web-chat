using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notes.Application;
using Notes.Application.Common.Mappings;
using Notes.Application.Interfaces;
using Notes.Persistence;
using Notes.WebApi.Middleware;
using System.Reflection;

namespace Notes.WebApi
{
    public class Startup
    {
        private const string ALLOWALL = "AllowAll";

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) =>
            Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(config =>
            {
                config.AddProfile(new
                    AssemblyMappingProfile(Assembly.GetExecutingAssembly()));
                config.AddProfile(new
                    AssemblyMappingProfile(typeof(INotesDbContext).Assembly));
            });

            services.AddAplication();
            services.AddPersistence(Configuration);
            services.AddControllers();

            services.AddCors(options =>
            {
                options.AddPolicy(ALLOWALL, policy =>
                {
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                    policy.AllowAnyOrigin();
                });
            });

            services.AddAuthentication(config =>
            {
                config.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;
                config.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = "https://localhost:44350/";
                    options.Audience = "NotesWebAPI";
                    options.RequireHttpsMetadata = false;
                });
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCustomExceptionHandler();
            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseCors(ALLOWALL);
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
