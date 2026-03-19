using Microsoft.AspNetCore.Mvc;
using HostelManagement.Models;
using HostelManagement.DAL;

namespace HostelManagement.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminDAL _adminDAL = new AdminDAL();

        // Security check method to keep code DRY (Don't Repeat Yourself)
        private bool IsAdminLoggedIn()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("AdminUser"));
        }

        // GET: /Admin/Index (The Dashboard)
        public IActionResult Index()
        {
            if (!IsAdminLoggedIn())
            {
                // Kick them back to the login page if they aren't authenticated
                return RedirectToAction("Welcome", "Account");
            }

            // Fetch the dynamic data from MySQL
            AdminDashboardViewModel dashboardData = _adminDAL.GetDashboardStats();

            // Pass the populated model to the Index.cshtml view
            return View(dashboardData);
        }

        // --------------------------------------------------------
        // Placeholder Actions for the Sidebar Navigation Links
        // We will build the views for these next!
        // --------------------------------------------------------

        // Update these actions inside your AdminController

        [HttpGet]
        public IActionResult NewAdmission()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Welcome", "Account");

            // Fetch rooms with empty beds and pass them to the View using ViewBag
            ViewBag.AvailableRooms = _adminDAL.GetAvailableRooms();
            ViewBag.Message = TempData["StatusMessage"];

            return View(new StudentViewModel());
        }

        [HttpPost]
        public IActionResult NewAdmission(StudentViewModel model)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Welcome", "Account");

            if (ModelState.IsValid)
            {
                string result = _adminDAL.AdmitNewStudent(model);
                TempData["StatusMessage"] = result;

                // If successful, clear the form by redirecting to the GET method
                if (result.Contains("Success"))
                {
                    return RedirectToAction("NewAdmission");
                }
            }
            else
            {
                ViewBag.Message = "Error: Please correct the invalid fields below.";
            }

            // If validation failed or database threw an error, reload the dropdown and show the form again
            ViewBag.AvailableRooms = _adminDAL.GetAvailableRooms();
            return View(model);
        }

        [HttpGet]
     
public IActionResult RentAndPayments()
{
    if (!IsAdminLoggedIn()) return RedirectToAction("Welcome", "Account");

    // Fetch the new combined ledger model
    FinancialLedgerViewModel ledger = _adminDAL.GetFinancialLedger();
    
    // Keep this so the modal dropdown still works
    ViewBag.ActiveStudents = _adminDAL.GetActiveStudentsList(); 
    ViewBag.Message = TempData["StatusMessage"];

    return View(ledger);
}

        [HttpPost]
        public IActionResult RecordPayment(RecordPaymentViewModel model)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Welcome", "Account");

            if (ModelState.IsValid)
            {
                string result = _adminDAL.RecordPayment(model);
                TempData["StatusMessage"] = result;
            }
            else
            {
                TempData["StatusMessage"] = "Error: Invalid payment details.";
            }

            return RedirectToAction("RentAndPayments");
        }

        [HttpGet]
        public IActionResult StaffAndSalaries()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Welcome", "Account");

            List<StaffViewModel> staffList = _adminDAL.GetAllStaff();
            ViewBag.Message = TempData["StatusMessage"];

            return View(staffList);
        }

        [HttpPost]
public IActionResult DeleteStaff(int staffId)
{
    if (!IsAdminLoggedIn()) return RedirectToAction("Welcome", "Account");

    TempData["StatusMessage"] = _adminDAL.DeleteStaff(staffId);
    return RedirectToAction("StaffAndSalaries");
}

        [HttpPost]
        public IActionResult AddStaff(StaffViewModel model)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Welcome", "Account");

            if (ModelState.IsValid)
            {
                TempData["StatusMessage"] = _adminDAL.AddNewStaff(model);
            }
            else
            {
                TempData["StatusMessage"] = "Error: Invalid staff details.";
            }

            return RedirectToAction("StaffAndSalaries");
        }

        [HttpPost]
        public IActionResult RecordSalary(SalaryPaymentViewModel model)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Welcome", "Account");

            if (ModelState.IsValid)
            {
                TempData["StatusMessage"] = _adminDAL.RecordSalaryPayment(model);
            }
            else
            {
                TempData["StatusMessage"] = "Error: Invalid salary payment details.";
            }

            return RedirectToAction("StaffAndSalaries");
        }

        // Update these actions inside your AdminController

        [HttpGet]
        public IActionResult RoomManagement()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Welcome", "Account");

            // Fetch rooms to display in the table
            List<RoomViewModel> rooms = _adminDAL.GetAllRooms();

            // Pass TempData messages to ViewBag so we can show success/error alerts
            ViewBag.Message = TempData["StatusMessage"];

            return View(rooms);
        }

        [HttpPost]
        public IActionResult AddRoom(RoomViewModel model)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Welcome", "Account");

            if (ModelState.IsValid)
            {
                string result = _adminDAL.AddNewRoom(model);

                // Store the result message temporarily to show it after redirecting
                TempData["StatusMessage"] = result;
            }
            else
            {
                TempData["StatusMessage"] = "Error: Invalid room data submitted.";
            }

            // Redirect back to the GET method to refresh the table
            return RedirectToAction("RoomManagement");
        }

[HttpPost]
public IActionResult UpdateRoom(RoomViewModel room)
{
    if (!IsAdminLoggedIn()) return RedirectToAction("Welcome", "Account");

    TempData["StatusMessage"] = _adminDAL.UpdateRoom(room);
    return RedirectToAction("RoomManagement");
}

[HttpPost]
public IActionResult DeleteRoom(int roomId)
{
    if (!IsAdminLoggedIn()) return RedirectToAction("Welcome", "Account");

    TempData["StatusMessage"] = _adminDAL.DeleteRoom(roomId);
    return RedirectToAction("RoomManagement");
}

        [HttpGet]
        public IActionResult ManageStudents()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Welcome", "Account");

            List<StudentListViewModel> students = _adminDAL.GetAllStudents();
            ViewBag.Message = TempData["StatusMessage"];

            return View(students);
        }

        [HttpPost]
        public IActionResult UpdateStudent(UpdateStudentViewModel model)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Welcome", "Account");
            if (ModelState.IsValid)
            {
                TempData["StatusMessage"] = _adminDAL.UpdateStudentContact(model);
            }
            return RedirectToAction("ManageStudents");
        }

        [HttpPost]
        public IActionResult MarkAsLeft(int studentId, DateTime actualLeavingDate)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Welcome", "Account");

            TempData["StatusMessage"] = _adminDAL.ProcessStudentLeaving(studentId, actualLeavingDate);
            return RedirectToAction("ManageStudents");
        }

        [HttpPost]
public IActionResult DeleteStudent(int studentId)
{
    if (!IsAdminLoggedIn()) return RedirectToAction("Welcome", "Account");

    bool isDeleted = _adminDAL.DeleteStudent(studentId);

    if (isDeleted)
    {
        TempData["StatusMessage"] = "Student and payment history deleted. Room bed is now free.";
    }
    else
    {
        TempData["StatusMessage"] = "Error: Could not delete the student.";
    }

    return RedirectToAction("ManageStudents");
}
    }
}