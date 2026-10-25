using DAL;

namespace BLL.Services
{
    public class SystemService
    {
        private readonly DataAccessFactory factory;

        public SystemService(DataAccessFactory factory)
        {
            this.factory = factory;
        }

        public bool IsDatabaseReachable()
        {
            try
            {
                return factory.CanConnect();
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
