using System.Data;
using System.Data.SqlClient;

namespace DefenceAcademy
{
    public class DapperContext
    {
        private readonly IConfiguration configuration;
        private readonly string connectionSting;
        public DapperContext(IConfiguration configuration)
        {
            this.configuration = configuration;
            connectionSting = configuration.GetConnectionString("DefaultConnection");
        }

       public async Task<IDbConnection> createConnection()
        {
            var open = new SqlConnection(connectionSting);
            await open.OpenAsync();
            return open;
        }
    }
}
