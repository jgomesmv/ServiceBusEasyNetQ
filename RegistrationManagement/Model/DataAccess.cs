using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistrationManagement.Model
{
    public static class DataAccess
    {
        static DataAccess()
        {
            Sessions = new List<Session>();
        }

        public class Session
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int MaxReservationsNumber { get; set; }
        }

        private static IList<Session> Sessions { get; set; }

        public static async Task<IList<Session>> GetAllSessions()
        {
            return Sessions;
        }

        public static async Task<Session> GetSession(int id)
        {
            return Sessions.SingleOrDefault(ts => ts.Id == id);
        }

        public static async Task<int> AddSession(Session session)
        {
            Sessions.Add(session);

            return session.Id;
        }

        public static async Task<Session> UpdateSession(Session session)
        {
            Session storedSession = Sessions.FirstOrDefault(s => s.Id == session.Id);
            if (storedSession != null)
            {
                storedSession.Name = session.Name;
                storedSession.MaxReservationsNumber = session.MaxReservationsNumber;
            }

            return storedSession;
        }

        public static async Task DeleteSession(int id)
        {
            Sessions.ToList().RemoveAll(ts => ts.Id == id);
        }
    }
}
