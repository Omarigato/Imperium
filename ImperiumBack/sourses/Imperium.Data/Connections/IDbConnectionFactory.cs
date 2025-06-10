using System.Data;
using System.Threading.Tasks;

namespace Imperium.Data.Connections
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
        Task<IDbConnection> CreateConnectionAsync();
    }
}