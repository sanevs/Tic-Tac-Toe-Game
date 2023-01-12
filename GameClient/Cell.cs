using Prism.Commands;
using System;
using System.ComponentModel;

namespace GameClient
{
    public class Cell : INotifyPropertyChanged
    {
        public Cell()
        {
            Text = ' ';
            OnOpen = new DelegateCommand(() => Open());
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public DelegateCommand OnOpen { get; set; }

        private bool isOpen;
        public bool IsOpen 
        { 
            get => isOpen; 
            set
            {
                if (isOpen == value)
                    return;
                isOpen = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsOpen)));
            }
        }
        private char text;
        public char Text
        {
            get => text;
            set
            {
                if (text == value)
                    return;
                text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }
        private void Open()
        {
            if (IsOpen)
                return;
            IsOpen = true;
        }
    }
}
