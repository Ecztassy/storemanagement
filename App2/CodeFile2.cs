using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using Windows.Storage;


namespace App2
{
    public class DatabaseHelper
    {
        private const string DatabaseName = "hardware.db";
        private string DatabasePath => Path.Combine(ApplicationData.Current.LocalFolder.Path, DatabaseName);
        private string ConnectionString => $"Data Source={DatabasePath};Version=3;";

        public DatabaseHelper()
        {
            CreateDatabase();
            EnsureTableExists();
        }

        private void CreateDatabase()
        {
            if (!File.Exists(DatabasePath))
            {
                SQLiteConnection.CreateFile(DatabasePath);
            }
        }

        private void EnsureTableExists()
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Components (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Version TEXT,
                        DateAdded TEXT,
                        Description TEXT,
                        ImagePath TEXT,
                        Status TEXT
                    );
                ";

                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void SaveComponent(ComponentModel component)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = "INSERT INTO Components (Name, Version, DateAdded, Description, ImagePath, Status) " +
                               "VALUES (@Name, @Version, @DateAdded, @Description, @ImagePath, @Status)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", component.Name);
                    command.Parameters.AddWithValue("@Version", component.Version ?? string.Empty);
                    command.Parameters.AddWithValue("@DateAdded", component.DateAdded?.ToString("dd-MM-yyyy") ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Description", component.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@ImagePath", component.ImagePath ?? string.Empty);
                    command.Parameters.AddWithValue("@Status", component.Status?.ToLower() ?? string.Empty);

                    command.ExecuteNonQuery();
                }
            }
        }


        public bool UpdateComponent(ComponentModel component)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = @"
                    UPDATE Components 
                    SET Name = @Name, 
                        Version = @Version, 
                        DateAdded = @DateAdded, 
                        Description = @Description, 
                        ImagePath = @ImagePath, 
                        Status = @Status 
                    WHERE Id = @Id;
                ";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", component.Name);
                    command.Parameters.AddWithValue("@Version", component.Version ?? string.Empty);
                    command.Parameters.AddWithValue("@DateAdded", component.DateAdded?.ToString("dd-MM-yyyy") ?? string.Empty);
                    command.Parameters.AddWithValue("@Description", component.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@ImagePath", component.ImagePath ?? string.Empty);
                    command.Parameters.AddWithValue("@Status", component.Status?.ToLower() ?? string.Empty);
                    command.Parameters.AddWithValue("@Id", component.Id);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }



        public bool DeleteComponent(int componentId)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = "DELETE FROM Components WHERE Id = @Id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", componentId);
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }



        public List<ComponentModel> GetAllComponents()
        {
            var components = new List<ComponentModel>();

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT Id, Name, Version, DateAdded, Description, ImagePath, Status FROM Components";

                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime? parsedDate = null;
                            if (!reader.IsDBNull(3))
                            {
                                var dateString = reader.GetString(3);
                                if (DateTime.TryParseExact(dateString, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out var tempDate))
                                {
                                    parsedDate = tempDate;
                                }
                            }

                            components.Add(new ComponentModel
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Version = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                DateAdded = parsedDate,
                                Description = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                ImagePath = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                Status = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                            });
                        }
                    }
                }
            }

            return components;
        }
    }
}
