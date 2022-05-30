using BugTracker.Models;

namespace BugTracker.Data.DAL
{
    public class TicketLogItemRepository : IRepository<TicketLogItem>
    {
        private readonly ApplicationDbContext Db;

        public TicketLogItemRepository(ApplicationDbContext db)
        {
            Db = db;
        }

        public void Add(TicketLogItem ticketLog)
        {
            Db.TicketLogItem.Add(ticketLog);
        }

        public void Delete(TicketLogItem ticketLog)
        {
            Db.TicketLogItem.Remove(ticketLog);
        }

        public void Update(TicketLogItem ticketLog)
        {
            Db.TicketLogItem.Update(ticketLog);
        }

        public TicketLogItem Get(int id)
        {
            return Db.TicketLogItem.First(ticketLog => ticketLog.Id == id);
        }

        public TicketLogItem Get(Func<TicketLogItem, bool> firstFunction)
        {
            return Db.TicketLogItem.First(firstFunction);
        }

        public ICollection<TicketLogItem> GetAll()
        {
            return Db.TicketLogItem.ToList();
        }

        public ICollection<TicketLogItem> GetList(Func<TicketLogItem, bool> whereFunction)
        {
            return Db.TicketLogItem.Where(whereFunction).ToList();
        }

        public void Save()
        {
            Db.SaveChanges();
        }
    }
}
