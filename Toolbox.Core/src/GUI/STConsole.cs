using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Toolbox.Core
{
    public class STConsole : INotifyPropertyChanged
    {
        private static STConsole _instance;
        public static STConsole Instance { get { return _instance == null ? _instance = new STConsole() : _instance; } }

        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged("Value");
            }
        }

        public static void Write(string value) {
            Instance.Value += $"{value}";
        }

        public static void WriteLine(string value) {
            Instance.Value += $"{value}\n";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
