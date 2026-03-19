using System;
using System.Collections.Generic;

namespace HostelManagement.Models
{
    public class StudentDashboardViewModel
    {
        // Profile & Room Info
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string RoomNumber { get; set; }
        public string RoomType { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal MessFee { get; set; }
        public DateTime JoiningDate { get; set; }
        public DateTime? LeavingDate { get; set; }

        // Financials
        public decimal TotalPaid { get; set; }
        public string PaymentStatus { get; set; }

        // Ledger
        public List<PaymentRecord> PaymentHistory { get; set; } = new List<PaymentRecord>();
    }

    public class PaymentRecord
    {
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMonth { get; set; }
    }
}