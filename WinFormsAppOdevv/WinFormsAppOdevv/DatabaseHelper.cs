using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace WinFormsAppOdevv
{
    public static class DatabaseHelper
    {
        private static string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=UserDb;Trusted_Connection=True;";

        public static void InitializeDatabase()
        {
            using (var connection = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;"))
            {
                connection.Open();
                string checkDbQuery = "SELECT database_id FROM sys.databases WHERE Name = 'UserDb'";
                using (var command = new SqlCommand(checkDbQuery, connection))
                {
                    var result = command.ExecuteScalar();
                    if (result == null)
                    {
                        string createDbQuery = "CREATE DATABASE UserDb";
                        using (var createDbCommand = new SqlCommand(createDbQuery, connection))
                        {
                            createDbCommand.ExecuteNonQuery();
                        }
                    }
                }
            }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string checkTableQuery = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users'";
                using (var command = new SqlCommand(checkTableQuery, connection))
                {
                    var result = command.ExecuteScalar();
                    if (result == null)
                    {
                        string createTableQuery = @"
                        CREATE TABLE Users (
                            Id INT PRIMARY KEY IDENTITY,
                            Username NVARCHAR(50) NOT NULL UNIQUE,
                            PasswordHash NVARCHAR(256) NOT NULL,
                            PasswordSalt NVARCHAR(256) NOT NULL
                        );";
                        using (var createTableCommand = new SqlCommand(createTableQuery, connection))
                        {
                            createTableCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public static bool RegisterUser(string username, string password)
        {
            if (UserExists(username))
            {
                return false; 
            }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                var salt = GenerateSalt();
                var hashedPassword = HashPassword(password, salt);

                string query = "INSERT INTO Users (Username, PasswordHash, PasswordSalt) VALUES (@Username, @PasswordHash, @PasswordSalt)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                    command.Parameters.AddWithValue("@PasswordSalt", salt);

                    command.ExecuteNonQuery();
                }
            }
            return true;
        }

        public static bool ValidateUser(string username, string password)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT PasswordHash, PasswordSalt FROM Users WHERE Username = @Username";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var storedHash = reader["PasswordHash"].ToString();
                            var storedSalt = reader["PasswordSalt"].ToString();
                            return storedHash.Equals(HashPassword(password, storedSalt));
                        }
                    }
                }
            }
            return false;
        }

        private static bool UserExists(string username)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        private static string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        private static string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = Encoding.UTF8.GetBytes(password + salt);
                var hash = sha256.ComputeHash(saltedPassword);
                return Convert.ToBase64String(hash);
            }
        }
    }
} 