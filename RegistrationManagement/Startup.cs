using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RegistrationManagement.IntegrationEvents.EventHandling;
using RegistrationManagement.IntegrationEvents.Events;
using TrainingManagementSystem.EventBus;
using TrainingManagementSystem.EventBus.Abstractions;
using TrainingManagementSystem.EventBusEasyNetQ;
using IServiceProvider = System.IServiceProvider;

namespace RegistrationManagement
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

            services.AddSingleton<IEasyNetQPersisterConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultEasyNetQPersisterConnection>>();

                var connectionString = "host=localhost:5672;username=guest;password=guest;platform=TrainingManagement;publisherConfirms=true";

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
            services.AddSingleton<IEventBusResilient, EventBusEasyNetQ>(sp =>
            {
                var easyNetQPersisterConnection = sp.GetRequiredService<IEasyNetQPersisterConnection>();
                var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                var logger = sp.GetRequiredService<ILogger<DefaultEasyNetQPersisterConnection>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                return new EventBusEasyNetQ(easyNetQPersisterConnection, logger, eventBusSubcriptionsManager, iLifetimeScope);
            });

            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

            // Event handlers instances
            services.AddTransient<TrainingSessionAddedIntegrationEventHandler>();
            services.AddTransient<TrainingSessionAddedIntegrationEventHandler>();
        }

        private void ConfigureEventBus(IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBusResilient>();

            eventBus.Subscribe<TrainingSessionAddedIntegrationEvent, TrainingSessionAddedIntegrationEventHandler>();
            eventBus.Subscribe<TrainingSessionChangedIntegrationEvent, TrainingSessionChangedIntegrationEventHandler>();
        }
    }
}
