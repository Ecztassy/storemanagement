using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace App2
{
    public sealed partial class ComponentsPage : Page
    {
        public ObservableCollection<ComponentModel> Components { get; set; }
        private readonly DatabaseHelper dbHelper;

        public ComponentsPage()
        {
            this.InitializeComponent();
            Components = new ObservableCollection<ComponentModel>();
            dbHelper = new DatabaseHelper();

            WelcomeTextBlock.Text = "Components";

            LoadComponents();
        }

        private void LoadComponents()
        {
            var components = dbHelper.GetAllComponents();
            Components.Clear();
            foreach (var component in components)
            {
                Components.Add(component);
            }
            FilterComponents();
        }

        private void FilterComponents()
        {
            if (Components == null || !Components.Any())
            {
                NoResultsText.Visibility = Visibility.Visible;
                return;
            }

            var searchText = SearchBox.Text.ToLower();

            var filtered = Components.Where(c =>
                string.IsNullOrWhiteSpace(searchText) ||
                (c.Name != null && c.Name.ToLower().Contains(searchText)))
                .ToList();

            ComponentsRepeater.ItemsSource = filtered;
            NoResultsText.Visibility = filtered.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ComponentButton_Click(object sender, RoutedEventArgs e)
        {
            OpenComponentEditor(new ComponentModel());
        }

        private void EditComponent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.CommandParameter is ComponentModel componentToEdit)
            {
                OpenComponentEditor(componentToEdit);
            }
        }

        private async void DeleteComponent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem menuItem && menuItem.CommandParameter is ComponentModel componentToDelete)
            {
                bool deleteConfirmed = await ConfirmDeletion(componentToDelete);
                if (!deleteConfirmed) return;

                bool isDeleted = dbHelper.DeleteComponent(componentToDelete.Id);
                if (isDeleted)
                {
                    Components.Remove(componentToDelete);
                    ComponentsRepeater.ItemsSource = null;
                    ComponentsRepeater.ItemsSource = Components;
                    FilterComponents();
                }
                else
                {
                    await ShowErrorDialog("Error deleting", "Failed to delete the selected component");
                }
            }
        }

        private async Task<bool> ConfirmDeletion(ComponentModel component)
        {
            var dialog = new ContentDialog
            {
                Title = "Confirm Deletion",
                Content = $"Do you want to delete the component '{component.Name}'?",
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

        private void OpenComponentEditor(ComponentModel component)
        {
            var componentWindow = new Window();
            var componentControl = new ComponentControl(component);

            componentControl.ComponentSaved += updatedComponent =>
            {
                if (updatedComponent.Id == 0)
                {
                    dbHelper.SaveComponent(updatedComponent);
                }
                else
                {
                    dbHelper.UpdateComponent(updatedComponent);
                }

                componentWindow.Close();
                RefreshComponents();
            };

            componentWindow.Closed += (_, _) => RefreshComponents();

            componentWindow.Content = componentControl;
            componentWindow.Activate();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterComponents();
        }

        private void RefreshComponents()
        {
            Components.Clear();
            LoadComponents();
            FilterComponents();
        }
    }
}
