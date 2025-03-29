using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace App2
{
    public class ComponentModel : INotifyPropertyChanged
    {
        private bool _isAtivoChecked;
        private bool _isInativoChecked;
        private bool _isReparoChecked;
        private string _status;

        public int Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public DateTime? DateAdded { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }

        // Checkboxes representing the status of the component
        public bool IsAtivoChecked
        {
            get => _isAtivoChecked;
            set
            {
                if (_isAtivoChecked != value)
                {
                    _isAtivoChecked = value;
                    OnPropertyChanged();
                    UpdateStatus();  // Recalculate Status when checkbox changes
                }
            }
        }

        public bool IsInativoChecked
        {
            get => _isInativoChecked;
            set
            {
                if (_isInativoChecked != value)
                {
                    _isInativoChecked = value;
                    OnPropertyChanged();
                    UpdateStatus();  // Recalculate Status when checkbox changes
                }
            }
        }

        public bool IsReparoChecked
        {
            get => _isReparoChecked;
            set
            {
                if (_isReparoChecked != value)
                {
                    _isReparoChecked = value;
                    OnPropertyChanged();
                    UpdateStatus();  // Recalculate Status when checkbox changes
                }
            }
        }

        // Computed property for formatted date
        public string FormattedDateAdded => DateAdded.HasValue ? DateAdded.Value.ToString("dd-MM-yyyy") : "No date available";

        // Property for Status with getter and setter
        public string Status
        {
            get => _status;  // Return the status string
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        // Method to compute and update Status based on the checkboxes
        private void UpdateStatus()
        {
            if (IsInativoChecked)
                Status = "Sem stock";
            else if (IsReparoChecked)
                Status = "Reparo";
            else if (IsAtivoChecked)
                Status = "Em stock";
            else
                Status = "Unknown";
        }



        // INotifyPropertyChanged implementation for data binding
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
