using System;
using System.Data.SQLite;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace App2
{
    public sealed partial class MainWindow : Window
    {
        private readonly string DatabasePath;


        public MainWindow()
        {
            this.InitializeComponent();

            // Define the database path in the local application data folder
            string appFolder = Path.Combine(AppContext.BaseDirectory, "Data");
            Directory.CreateDirectory(appFolder); // Ensure the folder exists
            DatabasePath = Path.Combine(appFolder, "users.db"); 


            // Ensure the directory exists
            Directory.CreateDirectory(appFolder);
            DatabasePath = Path.Combine(appFolder, "users.db");

            PrintDatabasePath(); // Debug: Print the database path
            InitializeDatabase();
        }

        private void PrintDatabasePath()
        {
            statusMessage.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Blue);
        }

        private void InitializeDatabase()
        {
            string connectionString = $"Data Source={DatabasePath};Version=3;";
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string createTableQuery = @"CREATE TABLE IF NOT EXISTS Users (
                                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            Username TEXT NOT NULL UNIQUE,
                                            Password TEXT NOT NULL)";
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            // Register button click handler
            string username = loginUsername.Text;
            string password = loginPassword.Password;

            if (RegisterUser(username, password))
            {
                statusMessage.Text = "Utilizador registrado com sucesso!";
                statusMessage.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green);
            }
            else
            {
                statusMessage.Text = "Erro! Utilizador já existente!";
                statusMessage.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
            }
        }

        private void myButton_Click2(object sender, RoutedEventArgs e)
        {
            // Login button click handler
            string username = loginUsername.Text;
            string password = loginPassword.Password;

            if (LoginUser(username, password))
            {
                statusMessage.Text = "Login successful!";
                statusMessage.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green);

                // Open Secondary Window with the username
                SecondaryWindow secondaryWindow = new SecondaryWindow();
                secondaryWindow.Activate();

                // Optionally, close the current window
                this.Close();
            }
            else
            {
                statusMessage.Text = "Invalid username or password.";
                statusMessage.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
            }
        }

        private bool RegisterUser(string username, string password)
        {
            try
            {
                string connectionString = $"Data Source={DatabasePath};Version=3;";
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string insertQuery = "INSERT INTO Users (Username, Password) VALUES (@Username, @Password)";
                    using (var command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password);
                        command.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (SQLiteException)
            {
                return false;
            }
        }

        private bool LoginUser(string username, string password)
        {
            string connectionString = $"Data Source={DatabasePath};Version=3;";
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string selectQuery = "SELECT COUNT(1) FROM Users WHERE Username = @Username AND Password = @Password";
                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count == 1;
                }
            }
        }
    }
}
