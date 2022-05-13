using BugTracker.Models;

namespace BugTracker.Data.DAL
{
    public class TicketHistoryRepository : IRepository<TicketHistory>
    {
        private readonly ApplicationDbContext Db;

        public TicketHistoryRepository(ApplicationDbContext db)
        {
            Db = db;
        }

        public void Add(TicketHistory ticketH)
        {
            Db.TicketHistory.Add(ticketH);
        }

        public void Delete(TicketHistory ticketH)
        {
            Db.TicketHistory.Remove(ticketH);
        }

        public void Update(TicketHistory ticketH)
        {
            Db.TicketHistory.Update(ticketH);
        }

        public TicketHistory Get(int id)
        {
            return Db.TicketHistory.First(th => th.Id == id);
        }

        public TicketHistory Get(Func<TicketHistory, bool> firstFunction)
        {
            return Db.TicketHistory.First(firstFunction);
        }

        public ICollection<TicketHistory> GetAll()
        {
            return Db.TicketHistory.ToList();
        }

        public ICollection<TicketHistory> GetList(Func<TicketHistory, bool> whereFunction)
        {
            return Db.TicketHistory.Where(whereFunction).ToList();
        }

        public void Save()
        {
            Db.SaveChanges();
        }
    }
}
