using Azure.Identity;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;


namespace App2
{
    public sealed partial class SecondaryWindow : Window
    {

        public SecondaryWindow()
        {
            this.InitializeComponent();



            // Load ComponentsPage by default
            ContentFrame.Navigate(typeof(ComponentsPage));
        }

        private void NavigateToComponentsPage(object sender, RoutedEventArgs e)
        {
            if (ContentFrame != null)
            {
                ContentFrame.Navigate(typeof(ComponentsPage));
            }
            else
            {
                // Handle the case where ContentFrame is still null.  Log an error, etc.
                System.Diagnostics.Debug.WriteLine("ERROR: ContentFrame is NULL in NavigateToComponentsPage!");
            }
        }

        private void NavigateToClientsPage(object sender, RoutedEventArgs e)
        {
            // Similar logic for the Clients button
            if (ContentFrame != null)
            {
                ContentFrame.Navigate(typeof(ClientsPage)); // Assuming you have a ClientsPage
            }
            else
            {
                // Handle the case where ContentFrame is still null.  Log an error, etc.
                System.Diagnostics.Debug.WriteLine("ERROR: ContentFrame is NULL in NavigateToClientsPage!");
            }
        }

    }
}
