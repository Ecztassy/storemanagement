using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace App2
{
    public sealed partial class ClientsPage : Page
    {
        public ObservableCollection<ClientModel> Clients { get; set; }
        private readonly DatabaseHelperclientes dbHelper;

        public ClientsPage()
        {
            this.InitializeComponent();
            Clients = new ObservableCollection<ClientModel>();
            dbHelper = new DatabaseHelperclientes();

            WelcomeTextBlock.Text = "Clients";

            LoadClients();
        }

        

        private void LoadClients()
        {
            var clients = dbHelper.GetAllComponents();
            Clients.Clear();
            foreach (var client in clients)
            {
                Clients.Add(client);
            }
            FilterClients();
        }

        private void FilterClients()
        {
            if (Clients == null || !Clients.Any())
            {
                NoResultsText.Visibility = Visibility.Visible;
                return;
            }

            var searchText = SearchBox.Text.ToLower();

            var filtered = Clients.Where(c =>
                string.IsNullOrWhiteSpace(searchText) ||
                (c.Name != null && c.Name.ToLower().Contains(searchText)) ||
                (c.Email != null && c.Email.ToLower().Contains(searchText)))
                .ToList();

            ClientsRepeater.ItemsSource = filtered;
            NoResultsText.Visibility = filtered.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ClientButton_Click(object sender, RoutedEventArgs e)
        {
            OpenClientEditor(new ClientModel());
        }

        private void EditClient_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.CommandParameter is ClientModel clientToEdit)
            {
                OpenClientEditor(clientToEdit);
            }
        }

        private async void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.CommandParameter is ClientModel clientToDelete)
            {
                bool deleteConfirmed = await ConfirmDeletion(clientToDelete);
                if (!deleteConfirmed) return;

                bool isDeleted = dbHelper.DeleteClient(clientToDelete.Id);
                if (isDeleted)
                {
                    Clients.Remove(clientToDelete);
                    ClientsRepeater.ItemsSource = null;
                    ClientsRepeater.ItemsSource = Clients;
                    FilterClients();
                }
                else
                {
                    await ShowErrorDialog("Error deleting", "Failed to delete the selected client");
                }
            }
        }

        private async Task<bool> ConfirmDeletion(ClientModel client)
        {
            var dialog = new ContentDialog
            {
                Title = "Confirm Deletion",
                Content = $"Do you want to delete the client '{client.Name}'?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
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

        private void OpenClientEditor(ClientModel client)
        {
            var clientWindow = new Window();
            var clientControl = new ClientControl(client);

            clientControl.ClientSaved += updatedClient =>
            {
                if (updatedClient.Id == 0)
                {
                    dbHelper.SaveClient(updatedClient);
                }
                else
                {
                    dbHelper.UpdateClient(updatedClient);
                }

                clientWindow.Close();
                RefreshClients();
            };

            clientWindow.Closed += (_, _) => RefreshClients();

            clientWindow.Content = clientControl;
            clientWindow.Activate();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterClients();
        }

        private void RefreshClients()
        {
            Clients.Clear();
            LoadClients();
            FilterClients();
        }
    }
}
