using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using WindowsAPICodePack.Dialogs;

namespace App2
{
    public sealed partial class ComponentControl : UserControl
    {
        public event Action<ComponentModel> ComponentSaved;

        private readonly DatabaseHelper dbHelper;
        private ComponentModel currentComponent;

        public ComponentControl(ComponentModel component = null)
        {
            this.InitializeComponent();
            dbHelper = new DatabaseHelper();
            currentComponent = component ?? new ComponentModel();

            // Populate UI with existing data if editing
            if (component != null)
            {
                NameTextBox.Text = component.Name;
                VersionTextBox.Text = component.Version;
                DatePicker.Date = component.DateAdded.HasValue
                    ? new DateTimeOffset(component.DateAdded.Value)
                    : (DateTimeOffset?)null;
                DescriptionTextBox.Text = component.Description;
                SetImagePath(component.ImagePath);
                SetStatusCheckBoxes(component.Status);
            }
        }

        private void SetImagePath(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                ComponentImage.Source = new BitmapImage(new Uri(imagePath));
            }
            else
            {
                ComponentImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/PlaceholderImage.png"));
            }
        }

        private void SetStatusCheckBoxes(string status)
        {
            if (!string.IsNullOrWhiteSpace(status))
            {
                AtivoCheckBox.IsChecked = status.Contains("Em Stock");
                InativoCheckBox.IsChecked = status.Contains("Sem Stock");
                ReparoCheckBox.IsChecked = status.Contains("Reparo");
            }
        }
        private void OnStatusCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox changedCheckBox)
            {
                // Ensure mutual exclusivity
                if (changedCheckBox == AtivoCheckBox && AtivoCheckBox.IsChecked == true)
                {
                    InativoCheckBox.IsChecked = false;
                    ReparoCheckBox.IsChecked = false;
                }
                else if (changedCheckBox == InativoCheckBox && InativoCheckBox.IsChecked == true)
                {
                    AtivoCheckBox.IsChecked = false;
                    ReparoCheckBox.IsChecked = false;
                }
                else if (changedCheckBox == ReparoCheckBox && ReparoCheckBox.IsChecked == true)
                {
                    AtivoCheckBox.IsChecked = false;
                    InativoCheckBox.IsChecked = false;
                }
            }
        }


        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text.Trim();
            string version = VersionTextBox.Text.Trim();
            string dateAddedStr = DatePicker.Date?.ToString("dd-MM-yyyy") ?? string.Empty;
            string description = DescriptionTextBox.Text.Trim();
            string imagePath = (ComponentImage.Source as BitmapImage)?.UriSource?.ToString() ?? string.Empty;

            // Determine status based on checkboxes
            string status = "Unknown";
            if (AtivoCheckBox.IsChecked == true) status = "Em stock";
            else if (InativoCheckBox.IsChecked == true) status = "Sem stock";
            else if (ReparoCheckBox.IsChecked == true) status = "Reparo";

            currentComponent.Status = status;

            // Basic validation
            if (string.IsNullOrEmpty(name))
            {
                await ShowErrorDialog("Validation Error", "Name cannot be empty.");
                return;
            }

            currentComponent.Name = name;
            currentComponent.Version = version;
            currentComponent.DateAdded = string.IsNullOrWhiteSpace(dateAddedStr)
                ? (DateTime?)null
                : DateTime.ParseExact(dateAddedStr, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
            currentComponent.Description = description;
            currentComponent.ImagePath = imagePath;
            currentComponent.Status = status;


            ComponentSaved?.Invoke(currentComponent);
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

        private async void ComponentImage_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await OpenFilePicker();
        }
        public void SetStatus(string status)
        {
            if (status == "Em stock")
            {
                AtivoCheckBox.IsChecked = true;
                InativoCheckBox.IsChecked = false;
                ReparoCheckBox.IsChecked = false;
            }
            else if (status == "Sem stock")
            {
                AtivoCheckBox.IsChecked = false;
                InativoCheckBox.IsChecked = true;
                ReparoCheckBox.IsChecked = false;
            }
            else if (status == "Reparo")
            {
                AtivoCheckBox.IsChecked = false;
                InativoCheckBox.IsChecked = false;
                ReparoCheckBox.IsChecked = true;
            }
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
                ComponentImage.Source = bitmapImage;
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Error", $"Failed to load image: {ex.Message}");
            }
        }
    }
}
