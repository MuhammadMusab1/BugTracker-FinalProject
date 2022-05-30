using BugTracker.Models;

namespace BugTracker.Data.DAL
{
    public class TicketNotificationRepository : IRepository<TicketNotification>
    {
        private readonly ApplicationDbContext Db;

        public TicketNotificationRepository(ApplicationDbContext db)
        {
            Db = db;
        }

        public void Add(TicketNotification ticketN)
        {
            Db.TicketNotification.Add(ticketN);
        }

        public void Delete(TicketNotification ticketN)
        {
            Db.TicketNotification.Remove(ticketN);
        }

        public void Update(TicketNotification ticketN)
        {
            Db.TicketNotification.Update(ticketN);
        }

        public TicketNotification Get(int id)
        {
            return Db.TicketNotification.First(tn => tn.Id == id);
        }

        public TicketNotification Get(Func<TicketNotification, bool> firstFunction)
        {
            return Db.TicketNotification.First(firstFunction);
        }

        public ICollection<TicketNotification> GetAll()
        {
            return Db.TicketNotification.ToList();
        }

        public ICollection<TicketNotification> GetList(Func<TicketNotification, bool> whereFunction)
        {
            return Db.TicketNotification.Where(whereFunction).ToList();
        }

        public void Save()
        {
            Db.SaveChanges();
        }
    }
}
