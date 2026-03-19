using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace HostelManagement.Models
{
    public class RentTransaction
    {
        public int PaymentId { get; set; }
        public string StudentName { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMonth { get; set; }
    }

    public class SalaryTransaction
    {
        public int SalaryPaymentId { get; set; }
        public string StaffName { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMonth { get; set; }
    }

    public class FinancialLedgerViewModel
    {
        public List<RentTransaction> RentPayments { get; set; } = new List<RentTransaction>();
        public List<SalaryTransaction> SalaryPayments { get; set; } = new List<SalaryTransaction>();
    }

    // For displaying the table
    public class RentSummaryViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string RoomNumber { get; set; }
        public decimal MonthlyFee { get; set; } // Rent + Mess
        public decimal TotalPaid { get; set; }
        public string PaymentStatus { get; set; }
        public decimal TotalDue { get; set; }
    }

    // For the Add Payment Modal
    public class RecordPaymentViewModel
    {
        [Required(ErrorMessage = "Please select a student")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(1, 100000, ErrorMessage = "Enter a valid amount")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment month is required")]
        public string PaymentMonth { get; set; }
    }

}