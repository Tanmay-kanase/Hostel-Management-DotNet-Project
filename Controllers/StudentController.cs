using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HostelManagement.Models;
using HostelManagement.DAL;

namespace HostelManagement.Controllers
{
    public class StudentController : Controller
    {
        private readonly StudentDAL _studentDAL = new StudentDAL();

        public IActionResult Index()
        {
            // Security Check: Ensure the student is actually logged in
            int? studentId = HttpContext.Session.GetInt32("StudentId");

            if (studentId == null)
            {
                // Kick back to login if no session exists
                return RedirectToAction("Welcome", "Account");
            }

            // Fetch the dashboard data using the secured session ID
            StudentDashboardViewModel dashboardData = _studentDAL.GetStudentDashboardData(studentId.Value);

            return View(dashboardData);
        }
    }
}