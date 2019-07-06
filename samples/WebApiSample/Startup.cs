using Imprise.MediatR.Extensions.Caching;
using MediatR;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace WebApiSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            AddMediator(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseMvc();
        }

        private static IServiceCollection AddMediator(IServiceCollection services)
        {
            // MediatR ServiceFactory
            services.AddScoped<ServiceFactory>(p => p.GetService);
            
            // Configure MediatR Pipeline with cache invalidation and cached request behaviors
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CacheBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestPostProcessorBehavior<,>));

            // Use Scrutor to scan and register all classes as their implemented interfaces.
            // This simplifies hooking up any ICache<Request, Response> implementations for the pipeline
            services.Scan(scan => scan
                .FromAssembliesOf(typeof(IMediator), typeof(Startup))
                .AddClasses()
                .AsImplementedInterfaces());

            return services;
        }
    }
}