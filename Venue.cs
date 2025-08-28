using System.ComponentModel.DataAnnotations;

namespace PTM2._0.Models
{
    public class Venue
    {
        [Key]
        [Display(Name = "场馆ID")]
        public int VenueID { get; set; }

        [Required]
        [Display(Name = "场馆名")]
        [StringLength(255, ErrorMessage = "场馆名长度不能超过255字符")]
        public string VenueName { get; set; }

        [Required]
        [Display(Name = "地址")]
        [StringLength(255)]
        public string VenueAddress { get; set; }

        [Required]
        [Range(10, 100000, ErrorMessage = "可容纳人数必须在10到100000之间")]
        [Display(Name = "可容纳人数")]
        public int Capacity { get; set; }

        public virtual ICollection<Performance> Performances { get; set; }
    }
}
