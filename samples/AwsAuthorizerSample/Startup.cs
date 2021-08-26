using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Moschen.AwsLambdaAuthenticationHandler;

namespace AwsAuthorizerSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Making all endpoints secure
            services.AddControllers(options => options.Filters.Add(new AuthorizeFilter()));

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "AwsAuthorizerSample", Version = "v1" });
            });

            services.AddAuthentication(AwsAuthorizerDefaults.AuthenticationScheme)
                .AddAwsAuthorizer();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("v1/swagger.json", "AwsAuthorizerSample v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
