using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Windows.Storage;

namespace App2
{
    public class DatabaseHelperclientes
    {
        private const string DatabaseName = "clientes.db";
        private string DatabasePath => Path.Combine(ApplicationData.Current.LocalFolder.Path, DatabaseName);
        private string ConnectionString => $"Data Source={DatabasePath};Version=3;";

        public DatabaseHelperclientes()
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
                    CREATE TABLE IF NOT EXISTS Clientes (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Email TEXT NOT NULL,
                        DateAdded TEXT NOT NULL,
                        Description TEXT,
                        ImagePath TEXT,
                        Endereco TEXT NOT NULL
                    );
                ";

                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void SaveClient(ClientModel client)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = "INSERT INTO Clientes (Name, Email, DateAdded, Description, ImagePath, Endereco) " +
                               "VALUES (@Name, @Email, @DateAdded, @Description, @ImagePath, @Endereco)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", client.Name);
                    command.Parameters.AddWithValue("@Email", client.Email ?? string.Empty);
                    command.Parameters.AddWithValue("@DateAdded", client.DateAdded?.ToString("dd-MM-yyyy") ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Description", client.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@ImagePath", client.ImagePath ?? string.Empty);
                    command.Parameters.AddWithValue("@Endereco", client.Endereco ?? string.Empty);

                    command.ExecuteNonQuery();
                }
            }
        }

        public bool UpdateClient(ClientModel client)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = @"
                    UPDATE Clientes 
                    SET Name = @Name, 
                        Email = @Email, 
                        DateAdded = @DateAdded, 
                        Description = @Description, 
                        ImagePath = @ImagePath, 
                        Endereco = @Endereco 
                    WHERE Id = @Id;
                ";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", client.Name);
                    command.Parameters.AddWithValue("@Email", client.Email ?? string.Empty);
                    command.Parameters.AddWithValue("@DateAdded", client.DateAdded?.ToString("dd-MM-yyyy") ?? string.Empty);
                    command.Parameters.AddWithValue("@Description", client.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@ImagePath", client.ImagePath ?? string.Empty);
                    command.Parameters.AddWithValue("@Endereco", client.Endereco ?? string.Empty);
                    command.Parameters.AddWithValue("@Id", client.Id);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteClient(int clientId)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = "DELETE FROM Clientes WHERE Id = @Id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", clientId);
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public List<ClientModel> GetAllComponents()
        {
            var clients = new List<ClientModel>();

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT Id, Name, Email, DateAdded, Description, ImagePath, Endereco FROM Clientes";

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

                            clients.Add(new ClientModel
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                DateAdded = parsedDate,
                                Description = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                ImagePath = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                Endereco = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                            });
                        }
                    }
                }
            }

            return clients;
        }
    }
}