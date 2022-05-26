using BugTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Data.DAL
{
    public class TicketCommentRepository : IRepository<TicketComment>
    {
        private readonly ApplicationDbContext Db;

        public TicketCommentRepository(ApplicationDbContext db)
        {
            Db = db;
        }

        public void Add(TicketComment ticketC)
        {
            Db.TicketComment.Add(ticketC);
        }

        public void Delete(TicketComment ticketC)
        {
            Db.TicketComment.Remove(ticketC);
        }

        public void Update(TicketComment ticketC)
        {
            Db.TicketComment.Update(ticketC);
        }

        public TicketComment Get(int id)
        {
            return Db.TicketComment.First(tc => tc.Id == id);
        }

        public TicketComment Get(Func<TicketComment, bool> firstFunction)
        {
            return Db.TicketComment.First(firstFunction);
        }

        public ICollection<TicketComment> GetAll()
        {
            return Db.TicketComment.ToList();
        }

        public ICollection<TicketComment> GetList(Func<TicketComment, bool> whereFunction)
        {
            return Db.TicketComment.Where(whereFunction).ToList();
        }

        public void Save()
        {
            Db.SaveChanges();
        }
    }
}
