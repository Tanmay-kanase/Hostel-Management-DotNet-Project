using System.ComponentModel.DataAnnotations;

namespace HostelManagement.Models
{
    public class RoomViewModel
    {
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Room number is required")]
        public string RoomNumber { get; set; }

        [Required(ErrorMessage = "Room type is required")]
        public string RoomType { get; set; }

        [Required]
        [Range(0, 100000, ErrorMessage = "Rent must be a positive number")]
        public decimal RentAmount { get; set; }

        [Required]
        [Range(0, 50000, ErrorMessage = "Mess fee must be a positive number")]
        public decimal MessFee { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Capacity must be between 1 and 10")]
        public int TotalCapacity { get; set; }

        public int CurrentOccupancy { get; set; }
    }
}