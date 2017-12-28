using Autofac;
using Autofac.Extensions.DependencyInjection;
using EasyNetQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TrainingManagement.IntegrationEvents;
using TrainingManagementSystem.EventBus;
using TrainingManagementSystem.EventBusEasyNetQ;
using IEventBus = TrainingManagementSystem.EventBus.Abstractions.IEventBus;
using IServiceProvider = System.IServiceProvider;

namespace TrainingManagement
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddTransient<ITrainingSessionIntegrationEventService, TrainingSessionIntegrationEventService>();

            services.AddSingleton<IEasyNetQPersisterConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultEasyNetQPersisterConnection>>();

                var connectionString = "host=localhost:5672;username=guest;password=guest;platform=TrainingManagement";

                return new DefaultEasyNetQPersisterConnection(connectionString, logger);
            });

            RegisterEventBus(services);

            var container = new ContainerBuilder();
            container.Populate(services);

            return new AutofacServiceProvider(container.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            ConfigureEventBus(app);
        }

        private void RegisterEventBus(IServiceCollection services)
        {
            services.AddSingleton<IEventBus, EventBusEasyNetQ>(sp =>
            {
                var easyNetQPersisterConnection = sp.GetRequiredService<IEasyNetQPersisterConnection>();
                var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                var logger = sp.GetRequiredService<ILogger<DefaultEasyNetQPersisterConnection>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                return new EventBusEasyNetQ(easyNetQPersisterConnection, logger, eventBusSubcriptionsManager, iLifetimeScope);
            });


            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

            // Event handlers instances
            //services.AddTransient<TrainingSessionChangedIntegrationEventHandler>();
        }

        private void ConfigureEventBus(IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

            //eventBus.Subscribe<TrainingSessionChangedIntegrationEvent, TrainingSessionChangedIntegrationEventHandler>();
        }
    }
}
