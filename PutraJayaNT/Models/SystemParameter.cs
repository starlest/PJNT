namespace ECRP.Models
{
    using System.ComponentModel.DataAnnotations;

    public class SystemParameter
    {
        public int ID { get; set; }

        [Required]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
