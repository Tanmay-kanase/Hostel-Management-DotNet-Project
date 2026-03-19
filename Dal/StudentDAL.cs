using System;
using HostelManagement.Models;
using MySql.Data.MySqlClient;

namespace HostelManagement.DAL
{
    public class StudentDAL
    {
        private readonly DatabaseHelper _dbHelper = new DatabaseHelper();

        // Returns the Student ID if a match is found, otherwise returns 0
        public int ValidateStudentLogin(string name, DateTime dob)
        {
            int studentId = 0;
            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand("sp_StudentLogin", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("p_name", name);
                // Formatting date to match MySQL 'YYYY-MM-DD'
                cmd.Parameters.AddWithValue("p_dob", dob.ToString("yyyy-MM-dd"));

                try
                {
                    conn.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            studentId = Convert.ToInt32(reader["student_id"]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Student Login Error: " + ex.Message);
                }
            }
            return studentId;
        }
        // Add this inside your StudentDAL class

        public StudentDashboardViewModel GetStudentDashboardData(int studentId)
        {
            StudentDashboardViewModel model = new StudentDashboardViewModel();

            using (MySqlConnection conn = _dbHelper.GetConnection())
            {
                // Query 1: Get Profile and Room details
                string profileQuery = @"
            SELECT s.name, s.email, s.mobile, s.joining_date, s.leaving_date, s.total_paid, s.payment_status,
                   r.room_number, r.room_type, r.rent_amount, r.mess_fee
            FROM Students s
            LEFT JOIN Rooms r ON s.room_id = r.room_id
            WHERE s.student_id = @studentId";

                // Query 2: Get Payment History
                string historyQuery = @"
            SELECT payment_date, amount, payment_month 
            FROM Rent_Payments 
            WHERE student_id = @studentId 
            ORDER BY payment_date DESC";

                try
                {
                    conn.Open();

                    // Execute Query 1
                    using (MySqlCommand cmdProfile = new MySqlCommand(profileQuery, conn))
                    {
                        cmdProfile.Parameters.AddWithValue("@studentId", studentId);
                        using (MySqlDataReader reader = cmdProfile.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                model.Name = reader["name"].ToString();
                                model.Email = reader["email"].ToString();
                                model.Mobile = reader["mobile"].ToString();
                                model.JoiningDate = Convert.ToDateTime(reader["joining_date"]);
                                model.LeavingDate = reader["leaving_date"] != DBNull.Value ? Convert.ToDateTime(reader["leaving_date"]) : (DateTime?)null;
                                model.TotalPaid = Convert.ToDecimal(reader["total_paid"]);
                                model.PaymentStatus = reader["payment_status"].ToString();

                                model.RoomNumber = reader["room_number"]?.ToString() ?? "Unassigned";
                                model.RoomType = reader["room_type"]?.ToString() ?? "N/A";
                                model.MonthlyRent = reader["rent_amount"] != DBNull.Value ? Convert.ToDecimal(reader["rent_amount"]) : 0;
                                model.MessFee = reader["mess_fee"] != DBNull.Value ? Convert.ToDecimal(reader["mess_fee"]) : 0;
                            }
                        }
                    }

                    // Execute Query 2
                    using (MySqlCommand cmdHistory = new MySqlCommand(historyQuery, conn))
                    {
                        cmdHistory.Parameters.AddWithValue("@studentId", studentId);
                        using (MySqlDataReader reader = cmdHistory.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                model.PaymentHistory.Add(new PaymentRecord
                                {
                                    PaymentDate = Convert.ToDateTime(reader["payment_date"]),
                                    Amount = Convert.ToDecimal(reader["amount"]),
                                    PaymentMonth = reader["payment_month"].ToString()
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error fetching student dashboard: " + ex.Message);
                }
            }
            return model;
        }
    }
}