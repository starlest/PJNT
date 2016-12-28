namespace ECERP.Utilities
{
    using System.Configuration;
    using System.Data.Entity;

    public class ERPInitialContext : DbContext
    {
        public ERPInitialContext()
        {
        }

        public ERPInitialContext(string ipAddress)
            : base(GetConnectionString(ipAddress))
        {
        }

        public static string GetConnectionString(string ipAddress)
        {
            var connString =
                ConfigurationManager.ConnectionStrings["ERPInitialContext"].ConnectionString;
            return string.Format(connString, ipAddress);
        }
    }
}
