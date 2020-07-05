using System;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;

namespace Toolbox.Core.GUI
{
    public enum Align
    {
        Left,
        Right,
        Center,
    }

    public enum FlowLayout
    {
        Vertical,
        Horizontal,
    }

    public class BindGUI : Attribute
    {
        /// <summary>
        /// The label displayed next to the control.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The index for the column the control is displayed on
        /// If none is set then controls will stack by default order.
        /// </summary>
        public int ColumnIndex { get; set; } = -1;

        /// <summary>
        /// The index for the row the control is displayed on.
        /// If none is set then controls will stack by default order.
        /// </summary>
        public int RowIndex { get; set; } = -1;

        public BindGUI(string text = "") {
            Label = text;
        }

        public BindGUI(string text, int column, int row) {
            Label = text;
            ColumnIndex = column;
            RowIndex = row;
        }
    }

    public class BindCategory : Attribute
    {
        /// <summary>
        /// The label displayed next to the control.
        /// </summary>
        public string Label { get; set; }

        public BindCategory(string text = "")
        {
            Label = text;
        }
    }

    public class ControlAttributes
    {
        public class MinMax : Attribute
        {
            /// <summary>
            /// The minimum vale used for the control.
            /// </summary>
            public decimal Min { get; set; }

            /// <summary>
            /// The maximum vale used for the control.
            /// </summary>
            public decimal Max { get; set; }

            public MinMax(decimal min, decimal max)
            {
                Min = min;
                Max = max;
            }

            public MinMax(int min, int max)
            {
                Min = min;
                Max = max;
            }
        }
    }

    public class Controls
    {
        public class TabControl : CommonControl
        {
            /// <summary>
            /// The list of pages the tab control contains.
            /// </summary>
            public List<TabPage> Pages { get; set; }

            public TabControl() {
                Pages = new List<TabPage>();
            }
        }

        public class TabPage : CommonControl
        {

        }

        public class Dropdown : CommonControl
        {

        }

        public class ColorBox : CommonControl
        {
            public Color Color { get; set; }

            public void Bind(ref Color Color)
            {

            }

            public void Bind(ref STColor Color)
            {

            }

            public void Bind(ref STColor8 Color)
            {

            }

            public void Bind(ref STColor16 Color)
            {

            }

            public void Bind(ref float R, ref float G, ref float B)
            {

            }

            public void Bind(ref byte R, ref byte G, ref byte B)
            {

            }
        }

        public class ColorAlphaBox : CommonControl
        {

        }

        public class TextBox : CommonControl
        {
            public bool Height { get; set; }
        }

        public class ComboBox : CommonControl
        {
            private List<object> Values = new List<object>();
            private Object BindObject;

            public object SelectedValue { get; set; }

            public void SetValue(object value) {
                SelectedValue = value;
            }

            public IEnumerable<object> GetValues() {
                return Values;
            }

            public void Bind(Type type, object obj, object value)
            {
                BindObject = obj;
                foreach (var enumValue in type.GetEnumValues()) {
                    Values.Add(enumValue);
                }
                SelectedValue = value;
            }

            public void Bind<T>(object obj, object value, IEnumerable<T> values)
            {
                BindObject = obj;
                foreach (var enumValue in values)
                    Values.Add(enumValue);
                SelectedValue = value;
            }
        }

        [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
        public class NumericUpDown : CommonControl
        {
            private decimal _value;
            public decimal Value
            {
                get { return _value; }
                set
                {
                    _value = value;
                    OnPropertyChanged("Value");
                }
            }

            public decimal Maximum { get; set; } = 100000000;
            public decimal Minimum { get; set; } = -100000000;

            public NumericUpDown(decimal value) {
                Value = value;
            }

            public NumericUpDown(decimal value, decimal max, decimal min) {
                Value = value;
                Maximum = max;
                Minimum = min;
            }

            public void SetValue(decimal value) {
                Value = value;
            }
        }

        [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
        public class Label : CommonControl
        {

        }

        [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
        public class Checkbox : CommonControl
        {
            public bool Checked { get; set; }
        }

        [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
        public class Panel : CommonControl
        {

        }

        [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
        public class CommonControl : Attribute, INotifyPropertyChanged
        {
            public virtual string Text { get; set; }

            private bool enabled = true;
            public bool Enabled
            {
                get { return enabled; }
                set
                {
                    if (enabled == value) return;
                    enabled = value;
                    OnPropertyChanged("Enabled");
                }
            }

            private bool visible = true;
            public bool Visible
            {
                get { return visible; }
                set
                {
                    if (visible == value) return;
                    visible = value;
                    OnPropertyChanged("Visible");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }

            public virtual void UpdateGUI()
            {

            }
        }
    }
}
