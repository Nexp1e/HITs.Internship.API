using System.ComponentModel.DataAnnotations;

namespace HITs.Internship.API.Dto.Local.Internship
{
    public class EditCharacteristicModel
    {
        [Required]
        public string NewCharacteristic { get; set; }
    }
}
