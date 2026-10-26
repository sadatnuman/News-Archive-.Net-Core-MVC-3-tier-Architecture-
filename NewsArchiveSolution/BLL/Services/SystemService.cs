using BLL.Interfaces;
using DAL;
using DAL.Interfaces;

namespace BLL.Services
{
    public class SystemService : ISystemService
    {
        private readonly IDataAccessFactory factory;

        public SystemService(IDataAccessFactory factory)
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
