using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMDBApp.Models
{
    public class Genre
    {
        // note u used the data-anotation here because it is not int
        // so EF don't know that the pk by defult 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; }
    }
}
