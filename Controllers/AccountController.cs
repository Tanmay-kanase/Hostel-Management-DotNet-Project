using Microsoft.AspNetCore.Mvc;
using HostelManagement.Models;
using HostelManagement.DAL;
using Microsoft.AspNetCore.Http; // Required for Sessions

namespace HostelManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly AdminDAL _adminDAL = new AdminDAL();
        private readonly StudentDAL _studentDAL = new StudentDAL();
        // GET: Loads the Welcome Page
        [HttpGet]
        public IActionResult Welcome()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public IActionResult StudentLogin(LoginViewModel model)
        {
            if (!string.IsNullOrEmpty(model.StudentName) && model.StudentDOB.HasValue)
            {
                int studentId = _studentDAL.ValidateStudentLogin(model.StudentName, model.StudentDOB.Value);

                if (studentId > 0)
                {
                    // Create Session specific to the Student using SetInt32
                    HttpContext.Session.SetInt32("StudentId", studentId);
                    HttpContext.Session.SetString("StudentName", model.StudentName);

                    // Redirect to the upcoming Student Dashboard
                    return RedirectToAction("Index", "Student");
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid Name or Date of Birth. Please check your admission details.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Please enter both Name and Date of Birth.";
            }

            // Keep the student tab active when reloading the page with an error
            ViewBag.ActiveTab = "student";
            return View("Welcome", model);
        }

        // POST: Handles Admin Login attempt
        [HttpPost]
        public IActionResult AdminLogin(LoginViewModel model)
        {
            if (!string.IsNullOrEmpty(model.AdminUsername) && !string.IsNullOrEmpty(model.AdminPassword))
            {
                bool isValid = _adminDAL.ValidateAdminLogin(model.AdminUsername, model.AdminPassword);

                if (isValid)
                {
                    // Create Session for Security
                    HttpContext.Session.SetString("AdminUser", model.AdminUsername);

                    // Redirect to the Admin Dashboard (assuming you have an AdminController with an Index action)
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid Admin Username or Password.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Please enter both username and password.";
            }

            // Return to the Welcome page with the error message
            return View("Welcome", model);
        }

        // Logout Action
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Welcome");
        }
    }
}