using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AfroBooks.Models
{
    public class Category
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        //[Range(0,1000, ErrorMessage = "Display Order must be between 0 and 300.")]
        public int DisplayOrder { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.Now;
    }
}
