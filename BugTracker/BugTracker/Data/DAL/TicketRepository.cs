using BugTracker.Models;

namespace BugTracker.Data.DAL
{
    public class TicketRepository : IRepository<Ticket>
    {
        private readonly ApplicationDbContext Db;

        public TicketRepository(ApplicationDbContext db)
        {
            Db = db;
        }

        public void Add(Ticket ticket)
        {
            Db.Ticket.Add(ticket);
        }

        public void Delete(Ticket ticket)
        {
            Db.Ticket.Remove(ticket);
        }

        public void Update(Ticket ticket)
        {
            Db.Ticket.Update(ticket);
        }

        public Ticket Get(int id)
        {
            return Db.Ticket.First(t => t.Id == id);
        }

        public Ticket Get(Func<Ticket, bool> firstFunction)
        {
            return Db.Ticket.First(firstFunction);
        }

        public ICollection<Ticket> GetAll()
        {
            return Db.Ticket.ToList();
        }

        public ICollection<Ticket> GetList(Func<Ticket, bool> whereFunction)
        {
            return Db.Ticket.Where(whereFunction).ToList();
        }

        public void Save()
        {
            Db.SaveChanges();
        }
    }
}
