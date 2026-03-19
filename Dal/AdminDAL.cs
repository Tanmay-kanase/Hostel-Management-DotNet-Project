using System;
using MySql.Data.MySqlClient;
using HostelManagement.Models;
namespace HostelManagement.DAL
{
    public class AdminDAL
    {
        private readonly DatabaseHelper _dbHelper = new DatabaseHelper();

        public bool ValidateAdminLogin(string username, string password)
        {
            bool isValid = false;

            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                // In a production app, always compare hashed passwords!
                string query = "SELECT COUNT(1) FROM Admins WHERE username = @username AND password_hash = @password";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password); // Assuming plain text for this initial testing phase

                try
                {
                    conn.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count == 1)
                    {
                        isValid = true;
                    }
                }
                catch (Exception ex)
                {
                    // Log exception here
                    Console.WriteLine("Database Error: " + ex.Message);
                }
            }
            return isValid;
        }

        public AdminDashboardViewModel GetDashboardStats()
        {
            AdminDashboardViewModel model = new AdminDashboardViewModel();

            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                // We batch all 5 queries together separated by semicolons
                string query = @"
                    -- 1. Total Active Students
                    SELECT COUNT(student_id) FROM Students WHERE leaving_date IS NULL;
                    
                    -- 2. Available Beds (Total Capacity - Current Occupancy)
                    SELECT COALESCE(SUM(total_capacity) - SUM(current_occupancy), 0) FROM Rooms;
                    
                    -- 3. Pending/Partial Rents for Active Students
                    SELECT COUNT(student_id) FROM Students WHERE payment_status IN ('Pending', 'Partial') AND leaving_date IS NULL;
                    
                    -- 4. Current Month Revenue
                    SELECT COALESCE(SUM(amount), 0) FROM Rent_Payments WHERE MONTH(payment_date) = MONTH(CURDATE()) AND YEAR(payment_date) = YEAR(CURDATE());
                    
                    -- 5. Recent Admissions (Latest 5)
                    SELECT s.student_id, s.name, r.room_number, s.joining_date, s.payment_status 
                    FROM Students s 
                    LEFT JOIN Rooms r ON s.room_id = r.room_id 
                    ORDER BY s.student_id DESC 
                    LIMIT 5;
                ";

                MySqlCommand cmd = new MySqlCommand(query, conn);

                try
                {
                    conn.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Result 1: Total Students
                        if (reader.Read()) model.TotalStudents = Convert.ToInt32(reader[0]);

                        // Move to Result 2: Available Beds
                        if (reader.NextResult() && reader.Read()) model.AvailableBeds = Convert.ToInt32(reader[0]);

                        // Move to Result 3: Pending Rents
                        if (reader.NextResult() && reader.Read()) model.PendingRents = Convert.ToInt32(reader[0]);

                        // Move to Result 4: Monthly Revenue
                        if (reader.NextResult() && reader.Read()) model.MonthlyRevenue = Convert.ToDecimal(reader[0]);

                        // Move to Result 5: Recent Admissions List
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                model.RecentAdmissions.Add(new RecentAdmission
                                {
                                    StudentId = Convert.ToInt32(reader["student_id"]),
                                    Name = reader["name"].ToString(),
                                    RoomNumber = reader["room_number"]?.ToString() ?? "Unassigned",
                                    JoiningDate = Convert.ToDateTime(reader["joining_date"]),
                                    PaymentStatus = reader["payment_status"].ToString()
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Dashboard Data Error: " + ex.Message);
                    // In a real app, you'd use a logger here (like Serilog or NLog)
                }
            }

            return model;
        }

        // Add this inside your AdminDAL class

        public string UpdateRoom(RoomViewModel room)
        {
            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                string query = @"
            UPDATE Rooms 
            SET room_number = @roomNumber, room_type = @roomType, 
                rent_amount = @rent, mess_fee = @mess, total_capacity = @capacity 
            WHERE room_id = @id";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@roomNumber", room.RoomNumber);
                cmd.Parameters.AddWithValue("@roomType", room.RoomType);
                cmd.Parameters.AddWithValue("@rent", room.RentAmount);
                cmd.Parameters.AddWithValue("@mess", room.MessFee);
                cmd.Parameters.AddWithValue("@capacity", room.TotalCapacity);
                cmd.Parameters.AddWithValue("@id", room.RoomId);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return "Success: Room updated successfully.";
                }
                catch (Exception ex)
                {
                    return "Error: Could not update room. Check if room number already exists.";
                }
            }
        }

        public string DeleteRoom(int roomId)
        {
            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                // Safety lock: MySQL will only delete if occupancy is exactly 0
                string query = "DELETE FROM Rooms WHERE room_id = @id AND current_occupancy = 0";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", roomId);

                try
                {
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                        return "Success: Room deleted successfully.";
                    else
                        return "Error: Could not delete room. Ensure it is empty.";
                }
                catch (Exception ex)
                {
                    return "Error: " + ex.Message;
                }
            }
        }
        public List<RoomViewModel> GetAllRooms()
        {
            List<RoomViewModel> rooms = new List<RoomViewModel>();
            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                string query = "SELECT * FROM Rooms ORDER BY room_number ASC";
                MySqlCommand cmd = new MySqlCommand(query, conn);

                try
                {
                    conn.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rooms.Add(new RoomViewModel
                            {
                                RoomId = Convert.ToInt32(reader["room_id"]),
                                RoomNumber = reader["room_number"].ToString(),
                                RoomType = reader["room_type"].ToString(),
                                RentAmount = Convert.ToDecimal(reader["rent_amount"]),
                                MessFee = Convert.ToDecimal(reader["mess_fee"]),
                                TotalCapacity = Convert.ToInt32(reader["total_capacity"]),
                                CurrentOccupancy = Convert.ToInt32(reader["current_occupancy"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error fetching rooms: " + ex.Message);
                }
            }
            return rooms;
        }

        public string AddNewRoom(RoomViewModel room)
        {
            string statusMessage = "";
            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand("sp_AddRoom", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("p_room_number", room.RoomNumber);
                cmd.Parameters.AddWithValue("p_room_type", room.RoomType);
                cmd.Parameters.AddWithValue("p_rent_amount", room.RentAmount);
                cmd.Parameters.AddWithValue("p_mess_fee", room.MessFee);
                cmd.Parameters.AddWithValue("p_total_capacity", room.TotalCapacity);

                try
                {
                    conn.Open();
                    // Reading the StatusMessage returned by our Stored Procedure
                    statusMessage = cmd.ExecuteScalar()?.ToString();
                }
                catch (Exception ex)
                {
                    statusMessage = "Application Error: " + ex.Message;
                }
            }
            return statusMessage;
        }
        // Add these inside your AdminDAL class

        public List<RoomViewModel> GetAvailableRooms()
        {
            List<RoomViewModel> availableRooms = new List<RoomViewModel>();
            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                // Only select rooms where current occupancy is less than total capacity
                string query = "SELECT room_id, room_number, room_type, rent_amount FROM Rooms WHERE current_occupancy < total_capacity ORDER BY room_number ASC";
                MySqlCommand cmd = new MySqlCommand(query, conn);

                try
                {
                    conn.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            availableRooms.Add(new RoomViewModel
                            {
                                RoomId = Convert.ToInt32(reader["room_id"]),
                                RoomNumber = reader["room_number"].ToString(),
                                RoomType = reader["room_type"].ToString(),
                                RentAmount = Convert.ToDecimal(reader["rent_amount"])
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error fetching available rooms: " + ex.Message);
                }
            }
            return availableRooms;
        }

        public string AdmitNewStudent(StudentViewModel student)
        {
            string statusMessage = "";
            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand("sp_AdmitStudent", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Calculate the exact leaving date based on duration
                DateTime calculatedLeavingDate = student.JoiningDate.AddMonths(student.StayDurationMonths);

                // Define the month string for the receipt (e.g., "March 2026")
                string paymentMonth = student.JoiningDate.ToString("MMMM yyyy");

                cmd.Parameters.AddWithValue("p_name", student.Name);
                cmd.Parameters.AddWithValue("p_dob", student.DOB);
                cmd.Parameters.AddWithValue("p_email", student.Email);
                cmd.Parameters.AddWithValue("p_mobile", student.Mobile);
                cmd.Parameters.AddWithValue("p_room_id", student.RoomId);
                cmd.Parameters.AddWithValue("p_joining_date", student.JoiningDate);
                cmd.Parameters.AddWithValue("p_stay_months", student.StayDurationMonths);
                cmd.Parameters.AddWithValue("p_initial_payment", student.InitialPayment);
                cmd.Parameters.AddWithValue("p_payment_month", paymentMonth);

                try
                {
                    conn.Open();
                    statusMessage = cmd.ExecuteScalar()?.ToString();
                }
                catch (Exception ex)
                {
                    statusMessage = "Application Error: " + ex.Message;
                }
            }
            return statusMessage;
        }

        // Add these to your AdminDAL class

        public List<RentSummaryViewModel> GetRentSummary()
        {
            List<RentSummaryViewModel> summary = new List<RentSummaryViewModel>();
            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                string query = @"
            -- Part 1: Fetch all active students (Includes actual Student ID)
            SELECT s.student_id AS StudentId, s.name AS StudentName, IFNULL(r.room_number, 'N/A') AS RoomNumber, 
                   (r.rent_amount + r.mess_fee) AS MonthlyFee, 
                   s.total_paid AS TotalPaid, s.payment_status AS PaymentStatus
            FROM Students s
            LEFT JOIN Rooms r ON s.room_id = r.room_id
            
            UNION ALL
            
            -- Part 2: Fetch revenue from deleted students (Passes '0' as a dummy ID)
            SELECT 0 AS StudentId, rp.student_name AS StudentName, 'Left Hostel' AS RoomNumber, 
                   0 AS MonthlyFee, 
                   SUM(rp.amount) AS TotalPaid, 'Archived' AS PaymentStatus
            FROM Rent_Payments rp
            WHERE rp.student_id IS NULL AND rp.student_name IS NOT NULL
            GROUP BY rp.student_name
            
            ORDER BY PaymentStatus, StudentName;";

                MySqlCommand cmd = new MySqlCommand(query, conn);

                try
                {
                    conn.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            summary.Add(new RentSummaryViewModel
                            {
                                // Now mapping to the EXACT aliases from the SQL query above
                                StudentId = Convert.ToInt32(reader["StudentId"]),
                                StudentName = reader["StudentName"].ToString(),
                                RoomNumber = reader["RoomNumber"].ToString(),
                                MonthlyFee = Convert.ToDecimal(reader["MonthlyFee"]),
                                TotalPaid = Convert.ToDecimal(reader["TotalPaid"]),
                                PaymentStatus = reader["PaymentStatus"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error fetching rent summary: " + ex.Message);
                }
            }
            return summary;
        }

        // To populate the Dropdown in the Add Payment Modal
        public Dictionary<int, string> GetActiveStudentsList()
        {
            Dictionary<int, string> students = new Dictionary<int, string>();
            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                string query = "SELECT student_id, name, room_id FROM Students WHERE leaving_date >= CURDATE() OR leaving_date IS NULL";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                try
                {
                    conn.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add(Convert.ToInt32(reader["student_id"]), reader["name"].ToString());
                        }
                    }
                }
                catch (Exception ex) { /* Handle Error */ }
            }
            return students;
        }

        public string RecordPayment(RecordPaymentViewModel payment)
        {
            string statusMessage = "";
            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand("sp_ProcessRentPayment", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("p_student_id", payment.StudentId);
                cmd.Parameters.AddWithValue("p_amount", payment.Amount);

                // Convert HTML 'YYYY-MM' input into a readable string like 'March 2026'
                DateTime parsedDate;
                string formattedMonth = payment.PaymentMonth;
                if (DateTime.TryParse(payment.PaymentMonth + "-01", out parsedDate))
                {
                    formattedMonth = parsedDate.ToString("MMMM yyyy");
                }
                cmd.Parameters.AddWithValue("p_payment_month", formattedMonth);

                try
                {
                    conn.Open();
                    statusMessage = cmd.ExecuteScalar()?.ToString();
                }
                catch (Exception ex)
                {
                    statusMessage = "Application Error: " + ex.Message;
                }
            }
            return statusMessage;
        }

        // Add these to your AdminDAL class

        public List<StaffViewModel> GetAllStaff()
        {
            List<StaffViewModel> staffList = new List<StaffViewModel>();
            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                string query = "SELECT * FROM Staff ORDER BY role, name";
                MySqlCommand cmd = new MySqlCommand(query, conn);

                try
                {
                    conn.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime joiningDate = Convert.ToDateTime(reader["joining_date"]);
                            decimal monthlySalary = Convert.ToDecimal(reader["monthly_salary"]);
                            decimal totalPaid = Convert.ToDecimal(reader["total_paid"]);

                            // Calculate total months worked (inclusive of the starting month)
                            int monthsWorked = ((DateTime.Today.Year - joiningDate.Year) * 12) + DateTime.Today.Month - joiningDate.Month + 1;
                            if (monthsWorked < 1) monthsWorked = 1; // Minimum 1 month

                            decimal totalDueOverall = monthsWorked * monthlySalary;
                            decimal remainingDue = totalDueOverall - totalPaid;

                            staffList.Add(new StaffViewModel
                            {
                                StaffId = Convert.ToInt32(reader["staff_id"]),
                                Name = reader["name"].ToString(),
                                Role = reader["role"].ToString(),
                                MonthlySalary = monthlySalary,
                                JoiningDate = joiningDate,
                                TotalPaid = totalPaid,
                                MonthsWorked = monthsWorked,
                                RemainingDue = remainingDue
                            });
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine("Error fetching staff: " + ex.Message); }
            }
            return staffList;
        }

        public string DeleteStaff(int staffId)
        {
            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                string query = "DELETE FROM Staff WHERE staff_id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", staffId);

                try
                {
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                        return "Success: Staff member removed from the directory.";
                    else
                        return "Error: Could not find the specified staff member.";
                }
                catch (Exception ex)
                {
                    return "Error: Could not remove staff member. " + ex.Message;
                }
            }
        }

        public string AddNewStaff(StaffViewModel staff)
        {
            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                string query = "INSERT INTO Staff (name, role, joining_date, monthly_salary) VALUES (@name, @role, @joining, @salary)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", staff.Name);
                cmd.Parameters.AddWithValue("@role", staff.Role);
                cmd.Parameters.AddWithValue("@joining", staff.JoiningDate);
                cmd.Parameters.AddWithValue("@salary", staff.MonthlySalary);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return "Success: Staff member added successfully.";
                }
                catch (Exception ex)
                {
                    return "Error: Could not add staff. " + ex.Message;
                }
            }
        }

        public string RecordSalaryPayment(SalaryPaymentViewModel payment)
        {
            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                try
                {
                    conn.Open();

                    // 1. Fetch exact monthly salary AND the staff name
                    MySqlCommand getStaffCmd = new MySqlCommand("SELECT name, monthly_salary FROM Staff WHERE staff_id = @id", conn);
                    getStaffCmd.Parameters.AddWithValue("@id", payment.StaffId);

                    string staffName = "";
                    decimal exactSalary = 0;

                    using (MySqlDataReader reader = getStaffCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            staffName = reader["name"].ToString();
                            exactSalary = Convert.ToDecimal(reader["monthly_salary"]);
                        }
                    }

                    if (string.IsNullOrEmpty(staffName)) return "Error: Staff member not found.";

                    // 2. Format the month
                    DateTime parsedDate;
                    string formattedMonth = payment.PaymentMonth;
                    if (DateTime.TryParse(payment.PaymentMonth + "-01", out parsedDate))
                    {
                        formattedMonth = parsedDate.ToString("MMMM yyyy");
                    }

                    // 3. Prevent duplicate payments for the same month
                    MySqlCommand checkCmd = new MySqlCommand("SELECT COUNT(1) FROM Salary_Payments WHERE staff_id = @id AND payment_month = @month", conn);
                    checkCmd.Parameters.AddWithValue("@id", payment.StaffId);
                    checkCmd.Parameters.AddWithValue("@month", formattedMonth);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0) return $"Error: Salary for {formattedMonth} has already been paid to {staffName}.";

                    // 4. Insert the payment WITH the staff_name stamped permanently
                    string query = "INSERT INTO Salary_Payments (staff_id, staff_name, payment_date, amount, payment_month) VALUES (@id, @name, CURDATE(), @amount, @month)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", payment.StaffId);
                    cmd.Parameters.AddWithValue("@name", staffName);
                    cmd.Parameters.AddWithValue("@amount", exactSalary); // Force exact amount
                    cmd.Parameters.AddWithValue("@month", formattedMonth);

                    cmd.ExecuteNonQuery();
                    return $"Success: Exact salary of ₹{exactSalary} for {formattedMonth} recorded successfully for {staffName}.";
                }
                catch (Exception ex)
                {
                    return "Error: Could not record salary. " + ex.Message;
                }
            }
        }


        public FinancialLedgerViewModel GetFinancialLedger()
        {
            FinancialLedgerViewModel ledger = new FinancialLedgerViewModel();
            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                try
                {
                    conn.Open();

                    // 1. Fetch Rent Payments (Income)
                    string rentQuery = @"
                SELECT payment_id, IFNULL(student_name, 'Unknown (Archived)') AS student_name, 
                       payment_date, amount, payment_month 
                FROM Rent_Payments 
                ORDER BY payment_date DESC, payment_id DESC";

                    using (MySqlCommand cmd = new MySqlCommand(rentQuery, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ledger.RentPayments.Add(new RentTransaction
                            {
                                PaymentId = Convert.ToInt32(reader["payment_id"]),
                                StudentName = reader["student_name"].ToString(),
                                PaymentDate = Convert.ToDateTime(reader["payment_date"]),
                                Amount = Convert.ToDecimal(reader["amount"]),
                                PaymentMonth = reader["payment_month"].ToString()
                            });
                        }
                    }

                    // 2. Fetch Salary Payments (Expenses)
                    string salaryQuery = @"
                SELECT salary_payment_id, IFNULL(staff_name, 'Unknown (Archived)') AS staff_name, 
                       payment_date, amount, payment_month 
                FROM Salary_Payments 
                ORDER BY payment_date DESC, salary_payment_id DESC";

                    using (MySqlCommand cmd = new MySqlCommand(salaryQuery, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ledger.SalaryPayments.Add(new SalaryTransaction
                            {
                                SalaryPaymentId = Convert.ToInt32(reader["salary_payment_id"]),
                                StaffName = reader["staff_name"].ToString(),
                                PaymentDate = Convert.ToDateTime(reader["payment_date"]),
                                Amount = Convert.ToDecimal(reader["amount"]),
                                PaymentMonth = reader["payment_month"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error fetching ledger: " + ex.Message);
                }
            }
            return ledger;
        }


        // Add these to your AdminDAL class

        public List<StudentListViewModel> GetAllStudents()
        {
            List<StudentListViewModel> students = new List<StudentListViewModel>();
            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                string query = @"
            SELECT s.student_id, s.name, s.email, s.mobile, r.room_number, s.joining_date, s.leaving_date , s.total_due, s.total_paid
            FROM Students s
            LEFT JOIN Rooms r ON s.room_id = r.room_id
            ORDER BY s.leaving_date ASC, s.name ASC"; // Active students first

                MySqlCommand cmd = new MySqlCommand(query, conn);

                try
                {
                    conn.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var leavingDate = reader["leaving_date"] != DBNull.Value ? Convert.ToDateTime(reader["leaving_date"]) : (DateTime?)null;

                            students.Add(new StudentListViewModel
                            {
                                StudentId = Convert.ToInt32(reader["student_id"]),
                                Name = reader["name"].ToString(),
                                Email = reader["email"].ToString(),
                                Mobile = reader["mobile"].ToString(),
                                RoomNumber = reader["room_number"]?.ToString() ?? "N/A",
                                JoiningDate = Convert.ToDateTime(reader["joining_date"]),
                                LeavingDate = leavingDate,
                                TotalDue = reader["total_due"] != DBNull.Value ? Convert.ToDecimal(reader["total_due"]) : 0m,
                                TotalPaid = reader["total_paid"] != DBNull.Value ? Convert.ToDecimal(reader["total_paid"]) : 0m,
                                // If leaving date is null or in the future, they are Active
                                Status = (leavingDate == null || leavingDate > DateTime.Today) ? "Active" : "Left"
                            });
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); }
            }
            return students;
        }

        public string UpdateStudentContact(UpdateStudentViewModel model)
        {
            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                string query = "UPDATE Students SET email = @email, mobile = @mobile WHERE student_id = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@email", model.Email);
                cmd.Parameters.AddWithValue("@mobile", model.Mobile);
                cmd.Parameters.AddWithValue("@id", model.StudentId);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return "Success: Student details updated.";
                }
                catch (Exception ex) { return "Error: " + ex.Message; }
            }
        }

        public string ProcessStudentLeaving(int studentId, DateTime actualLeavingDate)
        {
            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                // Calling the Stored Procedure we created earlier. Trigger B will handle the room capacity automatically!
                MySqlCommand cmd = new MySqlCommand("sp_ProcessStudentLeaving", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_student_id", studentId);
                cmd.Parameters.AddWithValue("p_leaving_date", actualLeavingDate);

                try
                {
                    conn.Open();
                    return cmd.ExecuteScalar()?.ToString();
                }
                catch (Exception ex) { return "Error: " + ex.Message; }
            }
        }

        public bool DeleteStudent(int studentId)
        {
            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                // We only need to delete the student. 
                // MySQL Cascade will delete payments, and our new Trigger will free the bed.
                string query = "DELETE FROM Students WHERE student_id = @studentId";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@studentId", studentId);

                try
                {
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    return false;
                }
            }
        }
    }
}