using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrainingManagement.IntegrationEvents;
using TrainingManagement.IntegrationEvents.Events;
using TrainingManagement.Model;
using TrainingManagement.ViewModel;
using TrainingManagementSystem.EventBus.Abstractions;

namespace TrainingManagement.Controllers
{
    [Route("TrainingManagement/Training")]
    public class TrainingController : ControllerBase
    {
        private readonly ITrainingSessionIntegrationEventService _trainingSessionIntegrationEventService;

        public TrainingController(ITrainingSessionIntegrationEventService trainingSessionIntegrationEventService)
        {
            _trainingSessionIntegrationEventService = trainingSessionIntegrationEventService;      
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Training Session 1", "Training Session 2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "Training Session 1";
        }

        // POST api/values
        [HttpPost]
        public string Post([FromBody]TrainingSession trainingSession)
        {
            DataAccess.AddTrainingSession(new DataAccess.TrainingSession { Id = trainingSession.Id, Name = trainingSession.Name, MaxReservationsNumber = trainingSession.MaxReservationsNumber });

            var trainingSessionAddedEvent = new TrainingSessionAddedIntegrationEvent(trainingSession.Id, trainingSession.Name, trainingSession.MaxReservationsNumber);
            _trainingSessionIntegrationEventService.PublishThroughEventBusAsync(trainingSessionAddedEvent);

            return "Training Session Added!";
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public string Put(int id, [FromBody]TrainingSession trainingSession)
        {
            DataAccess.UpdateTrainingSession(new DataAccess.TrainingSession { Id = trainingSession.Id, Name = trainingSession.Name, MaxReservationsNumber = trainingSession.MaxReservationsNumber });

            var trainingSessionChangedEvent = new TrainingSessionChangedIntegrationEvent(id, trainingSession.Name, trainingSession.MaxReservationsNumber);
            _trainingSessionIntegrationEventService.PublishThroughEventBusAsync(trainingSessionChangedEvent);

            return "Training Session Updated!";
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
