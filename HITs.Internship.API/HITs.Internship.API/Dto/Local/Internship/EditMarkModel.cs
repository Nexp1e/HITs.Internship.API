using System.ComponentModel.DataAnnotations;

namespace HITs.Internship.API.Dto.Local.Internship
{
    public class EditMarkModel
    {
        [Required]
        [Range(minimum: 2.0, maximum: 5.3)]
        public double NewMark { get; set; }
    }
}
