namespace ECERP.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Server
    {
        public int ID { get; set; }

        [Required, MaxLength(100), Index(IsUnique = true)]
        public string ServerName { get; set; }

        [Required, MaxLength(100), Index(IsUnique = true)]
        public string DatabaseName { get; set; }
    }
}
