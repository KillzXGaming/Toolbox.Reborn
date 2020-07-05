using System;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.Reflection;
using Toolbox.Core;

namespace Toolbox.Winforms
{
    public class LabelControl : INotifyPropertyChanged
    {
        object Value;
        PropertyInfo Property;

        public string Text
        {
            get { return (string)Property.GetValue(Value); }
            set
            {
                Property.SetValue(Value, value);
                NotifyPropertyChanged("Text");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public LabelControl(object value, PropertyInfo prop) {
            Property = prop;
            Value = value;
        }
    }
}
