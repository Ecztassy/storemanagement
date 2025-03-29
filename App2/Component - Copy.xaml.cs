using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using WindowsAPICodePack.Dialogs;

namespace App2
{
    public sealed partial class ClientControl : UserControl
    {
        public event Action<ClientModel> ClientSaved;

        private readonly DatabaseHelperclientes dbHelper;
        private ClientModel currentClient;

        public ClientControl(ClientModel client = null)
        {
            this.InitializeComponent();
            dbHelper = new DatabaseHelperclientes();
            currentClient = client ?? new ClientModel();
            EmailTextBox.TextChanged += EmailTextBox_TextChanged;

            if (client != null)
            {
                NameTextBox.Text = client.Name;
                EmailTextBox.Text = client.Email;
                DatePicker.Date = client.DateAdded.HasValue
                    ? new DateTimeOffset(client.DateAdded.Value)
                    : (DateTimeOffset?)null;
                DescriptionTextBox.Text = client.Description;
                EnderecoTextBox.Text = client.Endereco;
                SetImagePath(client.ImagePath);
            }
        }

        private void SetImagePath(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                ClientImage.Source = new BitmapImage(new Uri(imagePath));
            }
            else
            {
                ClientImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/PlaceholderImage.png"));
            }
        }
        private bool IsValidEmail(string email)
        {
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }


        private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (Regex.IsMatch(email, emailPattern))
            {
                EmailTextBox.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green);
            }
            else
            {
                EmailTextBox.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
            }
        }



        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValidEmail(EmailTextBox.Text))
            {
                ContentDialog invalidEmailDialog = new ContentDialog
                {
                    Title = "Erro de Validação",
                    Content = "O email digitado não é válido. Por favor, insira um email correto.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot // Required for WinUI 3 dialogs
                };
                _ = invalidEmailDialog.ShowAsync();
                return;
            }

            string name = NameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string dateAddedStr = DatePicker.Date?.ToString("dd-MM-yyyy") ?? string.Empty;
            string description = DescriptionTextBox.Text.Trim();
            string endereco = EnderecoTextBox.Text.Trim();
            string imagePath = (ClientImage.Source as BitmapImage)?.UriSource?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(endereco))
            {
                await ShowErrorDialog("Validation Error", "Name, Email, and Endereco cannot be empty.");
                return;
            }

            currentClient.Name = name;
            currentClient.Email = email;
            currentClient.DateAdded = string.IsNullOrWhiteSpace(dateAddedStr)
                ? (DateTime?)null
                : DateTime.ParseExact(dateAddedStr, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
            currentClient.Description = description;
            currentClient.Endereco = endereco;
            currentClient.ImagePath = imagePath;

            ClientSaved?.Invoke(currentClient);
        }

        private async Task ShowErrorDialog(string title, string message)
        {
            var errorDialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "Ok",
                XamlRoot = this.XamlRoot
            };
            await errorDialog.ShowAsync();
        }

        private async void ClientImage_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await OpenFilePicker();
        }

        private async void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            await OpenFilePicker();
        }

        private async Task OpenFilePicker()
        {
            using (var dialog = new CommonOpenFileDialog
            {
                Multiselect = false,
                IsFolderPicker = false,
                Title = "Select an Image",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            })
            {
                dialog.Filters.Add(new CommonFileDialogFilter("Image Files", "*.jpg;*.jpeg;*.png"));
                dialog.Filters.Add(new CommonFileDialogFilter("All Files", "*.*"));

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var filePath = dialog.FileName;
                    await LoadImageAsync(filePath);
                }
            }
        }

        private async Task LoadImageAsync(string filePath)
        {
            try
            {
                var bitmapImage = new BitmapImage(new Uri(filePath));
                ClientImage.Source = bitmapImage;
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Error", $"Failed to load image: {ex.Message}");
            }
        }
    }
}
