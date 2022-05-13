using BugTracker.Models;

namespace BugTracker.Data.DAL
{
    public class TicketAttachmentRepository : IRepository<TicketAttachment>
    {
        private readonly ApplicationDbContext Db;

        public TicketAttachmentRepository(ApplicationDbContext db)
        {
            Db = db;
        }

        public void Add(TicketAttachment ticketA)
        {
            Db.TicketAttachment.Add(ticketA);
        }

        public void Delete(TicketAttachment ticketA)
        {
            Db.TicketAttachment.Remove(ticketA);
        }

        public void Update(TicketAttachment ticketA)
        {
            Db.TicketAttachment.Update(ticketA);
        }

        public TicketAttachment Get(int id)
        {
            return Db.TicketAttachment.First(ta => ta.Id == id);
        }

        public TicketAttachment Get(Func<TicketAttachment, bool> firstFunction)
        {
            return Db.TicketAttachment.First(firstFunction);
        }

        public ICollection<TicketAttachment> GetAll()
        {
            return Db.TicketAttachment.ToList();
        }

        public ICollection<TicketAttachment> GetList(Func<TicketAttachment, bool> whereFunction)
        {
            return Db.TicketAttachment.Where(whereFunction).ToList();
        }

        public void Save()
        {
            Db.SaveChanges();
        }
    }
}
