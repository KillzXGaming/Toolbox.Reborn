using System;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.Reflection;
using Toolbox.Core;

namespace Toolbox.Winforms
{
    public class ColorControl : INotifyPropertyChanged
    {
        object Value;
        PropertyInfo Property;

        public STColor8 Color
        {
            get { return (STColor8)Property.GetValue(Value); }
            set
            {
                Property.SetValue(Value, value);
                Console.WriteLine("ColorControl " + Property.Name);
                NotifyPropertyChanged("Color");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public ColorControl(object value, PropertyInfo prop) {
            Property = prop;
            Value = value;
        }
    }
}
