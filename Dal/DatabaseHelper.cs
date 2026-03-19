using MySql.Data.MySqlClient;

namespace HostelManagement.DAL
{
    public class DatabaseHelper
    {
        private readonly string connectionString;

        public DatabaseHelper()
        {
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

            connectionString = $"Server=mysql-2f2bf5ea-tsktechnological-4f6a.j.aivencloud.com;" +
                               $"Port=13920;" +
                               $"Database=HostelManagement;" +
                               $"User=avnadmin;" +
                               $"Password={password};" +
                               $"SslMode=Required;";
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}