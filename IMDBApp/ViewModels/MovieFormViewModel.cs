using IMDBApp.Models;
using System.ComponentModel.DataAnnotations;

namespace IMDBApp.ViewModels
{
    public class MovieFormViewModel
    {
        public int Id { get; set; }

        [StringLength(250)]
        public string Title { get; set; }

        public int Year { get; set; }

        [Range(1, 10)]
        public double Rate { get; set; }

        [StringLength(2500)]
        public string StoryLine { get; set; }

        [Display(Name = "Select poster...")]
        public byte[]? Poster { get; set; }

        [Display(Name = "Genre")]
        public byte GenreId { get; set; }

        public IEnumerable<Genre>? Genres { get; set; }
    }
}
