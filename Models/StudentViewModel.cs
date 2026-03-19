using System;
using System.ComponentModel.DataAnnotations;

namespace HostelManagement.Models
{
    public class StudentViewModel
    {
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Student Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        public DateTime DOB { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mobile Number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Enter a valid 10-digit mobile number")]
        public string Mobile { get; set; }

        [Required(ErrorMessage = "Please select a room")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Joining Date is required")]
        [DataType(DataType.Date)]
        public DateTime JoiningDate { get; set; } = DateTime.Today; // Defaults to today's date

        [Required(ErrorMessage = "Please specify stay duration")]
        [Range(1, 60, ErrorMessage = "Duration must be between 1 and 60 months")]
        public int StayDurationMonths { get; set; }

        [Required(ErrorMessage = "Initial payment is required")]
        [Range(0, 100000, ErrorMessage = "Payment must be a positive amount")]
        public decimal InitialPayment { get; set; }
    }
}