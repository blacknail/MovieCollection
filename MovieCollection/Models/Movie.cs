using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MovieCollection.Models
{
    public class Movie
    {
        public int MovieID { get; set; }

        [Required]
        [StringLength(400, MinimumLength = 3)]
        public string Title { get; set; }

        [StringLength(4000, MinimumLength = 3)]
        public string Description { get; set; }

        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ReleaseDate { get; set; }

        [StringLength(200, MinimumLength = 3)]
        public string Director { get; set; }

        public string Poster { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}