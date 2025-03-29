using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace App2
{
    public class ClientModel : INotifyPropertyChanged 
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime? DateAdded { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public string Endereco { get; set; }

        public string FormattedDateAdded => DateAdded.HasValue ? DateAdded.Value.ToString("dd-MM-yyyy") : "No date available";
        public bool IsValidEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
                return false;

            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(Email, emailPattern);
        }





        // INotifyPropertyChanged implementation for data binding
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
