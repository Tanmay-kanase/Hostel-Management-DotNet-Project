using System;
using System.Collections.Generic;

namespace HostelManagement.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int AvailableBeds { get; set; }
        public int PendingRents { get; set; }
        public decimal MonthlyRevenue { get; set; }
        
        // List to hold the table rows
        public List<RecentAdmission> RecentAdmissions { get; set; } = new List<RecentAdmission>();
    }

    public class RecentAdmission
    {
        public int StudentId { get; set; }
        public string Name { get; set; }
        public string RoomNumber { get; set; }
        public DateTime JoiningDate { get; set; }
        public string PaymentStatus { get; set; }
    }
}