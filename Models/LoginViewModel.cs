using System.ComponentModel.DataAnnotations;

namespace HostelManagement.Models
{
    public class LoginViewModel
    {
        // Admin Login Properties
        public string? AdminUsername { get; set; }
        public string? AdminPassword { get; set; }

        // Student Login Properties (For later use)
        public string? StudentName { get; set; }
        public DateTime? StudentDOB { get; set; }
    }
}