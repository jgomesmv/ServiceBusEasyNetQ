using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrainingManagement.Model
{
    public static class DataAccess
    {
        static DataAccess()
        {
            TraningSessions = new List<TrainingSession>();
        }

        public class TrainingSession
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int MaxReservationsNumber { get; set; }
        }

        private static IList<TrainingSession> TraningSessions { get ; set;}

        public static IList<TrainingSession> GetAllTrainingSessions()
        {
            return TraningSessions;
        }

        public static TrainingSession GetTraningSession(int id)
        {
            return TraningSessions.SingleOrDefault(ts => ts.Id == id);
        }

        public static int AddTrainingSession(TrainingSession traningSession)
        {
            TraningSessions.Add(traningSession);

            return traningSession.Id;
        }

        public static TrainingSession UpdateTrainingSession(TrainingSession session)
        {
            TrainingSession storedTrainingSession = TraningSessions.FirstOrDefault(s => s.Id == session.Id);
            if (storedTrainingSession != null)
            {
                storedTrainingSession.Name = session.Name;
                storedTrainingSession.MaxReservationsNumber = session.MaxReservationsNumber;
            }

            return storedTrainingSession;
        }

        public static void DeleteTrainingSession(int id)
        {
            TraningSessions.ToList().RemoveAll(ts => ts.Id == id);
        }
    }
}
