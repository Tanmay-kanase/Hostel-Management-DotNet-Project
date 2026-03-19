using System;
using System.ComponentModel.DataAnnotations;

namespace HostelManagement.Models
{
    // For listing students in the table
    public class StudentListViewModel
    {
        public int StudentId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string RoomNumber { get; set; }
        public DateTime JoiningDate { get; set; }
        public DateTime? LeavingDate { get; set; } // Nullable because they might still be active
        public string Status { get; set; } // Active or Left
        public decimal TotalDue { get; set; }
    public decimal TotalPaid { get; set; }
    }

    // For updating basic contact details
    public class UpdateStudentViewModel
    {
        public int StudentId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Enter a valid 10-digit mobile number")]
        public string Mobile { get; set; }
    }
}