namespace BugTracker.Data.DAL
{
    public interface IRepository<T> where T : class
    {
        //create
        void Add(T entity);

        //read
        T Get(int id);
        T Get(Func<T, bool> firstFunction);
        ICollection<T> GetAll();
        ICollection<T> GetList(Func<T, bool> whereFunction);

        //update
        void Update(T entity);

        //delete
        void Delete(T entity);

        void Save();
    }
}
