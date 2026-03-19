using System.ComponentModel.DataAnnotations;

namespace HostelManagement.Models
{
    // For listing and adding staff
    public class StaffViewModel
    {
        public int StaffId { get; set; }

        [Required(ErrorMessage = "Staff name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } // e.g., 'Cleaning', 'Security', 'Cook', 'Manager'

        [Required(ErrorMessage = "Monthly salary is required")]
        [Range(1, 200000, ErrorMessage = "Enter a valid salary amount")]
        public decimal MonthlySalary { get; set; }
        public DateTime JoiningDate { get; set; }
         public decimal TotalPaid { get; set; }
    public int MonthsWorked { get; set; }
    public decimal RemainingDue { get; set; }
    }

    // For the Salary Payment Modal
    public class SalaryPaymentViewModel
    {
        [Required(ErrorMessage = "Please select a staff member")]
        public int StaffId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(1, 200000, ErrorMessage = "Enter a valid amount")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment month is required")]
        public string PaymentMonth { get; set; }
        public DateTime JoiningDate { get; set; }
   
    }
}