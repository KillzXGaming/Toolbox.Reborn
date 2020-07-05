using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using Toolbox.Core;
using Toolbox.Core.GUI;
using STLibrary.Forms;

namespace Toolbox.Winforms
{
    public class GUIGenerator
    {
        public static Control Generate(object value)
        {
            ControlHandle handler = new ControlHandle();
            handler.SearchTypes(value);
            handler.SearchFields(value);
            handler.SearchProperties(value);
            handler.AddFlowTable();
            return handler.GetControl();
        }

        class BasePanel : STPanel
        {
            public BasePanel()
            {
                SetStyle(
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer,
                     true);
            }
        }

        public class ControlHandle
        {
            private STPanel Control;
            private FlowLayoutPanel FlowPanel;
            private FlowLayoutPanel DropFlowPanel;
            private TableLayoutPanel Table;

            Dictionary<string, DropdownPanel> DropdownPanels = new Dictionary<string, DropdownPanel>();

            public ControlHandle()
            {
                Control = new BasePanel();
                Control.Width = 400;
                FlowPanel = new FlowLayoutPanel();
                FlowPanel.Dock = DockStyle.Fill;
                Control.ResumeLayout(false);
                Control.PerformLayout();
            }

            internal void AddFlowTable()
            {
                if (FlowPanel.Controls.Count > 0)
                    Control.Controls.Add(FlowPanel);
                Control.ResumeLayout(false);
                Control.PerformLayout();
            }

            public Control GetControl() => Control;

            public void SearchTypes(object value)
            {
                Console.WriteLine($"TYPE {value}");
                if (value is Controls.TabControl)
                {
                    var tabProperty = (Controls.TabControl)value;

                    STTabControl tabControl = new STTabControl();
                    tabControl.Dock = DockStyle.Fill;
                    Control.Controls.Add(tabControl);

                    foreach (var page in tabProperty.Pages)
                    {
                        var control = GUIGenerator.Generate(page);
                        if (control != null)
                        {
                            TabPage pg = new TabPage();
                            pg.Text = page.Text;
                            pg.DataBindings.Add(new Binding("Text", page, "Text", false, DataSourceUpdateMode.OnPropertyChanged));
                            control.Dock = DockStyle.Fill;
                            pg.Controls.Add(control);
                            tabControl.TabPages.Add(pg);
                        }
                    }

                    Console.WriteLine($"tab {tabControl.TabPages.Count}");
                }
            }

            public void SearchFields(object value)
            {
                foreach (var field in value.GetType().GetFields())
                {
                    SetupEditor(value, field);
                    var subFields = field.GetType().GetFields().Where(subField => subField.IsStatic == false).ToArray();
                    var subProperties = field.GetType().GetProperties().ToArray();

                    /*   foreach (var subfield in subFields)
                           SearchFields(subfield);
                       foreach (var subProp in subProperties)
                           SearchProperties(subProp);*/
                }
            }

            public void SearchProperties(object value)
            {
                var properties = value.GetType().GetProperties();
                for (int i = 0; i < properties.Length; i++)
                {
                    var subFields = properties[i].PropertyType.GetFields().Where(subField => subField.IsStatic == false).ToArray();
                    var subProperties = properties[i].PropertyType.GetProperties().ToArray();

                    var bindableAttribute = properties[i].GetCustomAttribute<BindGUI>();
                    if (bindableAttribute == null)
                        continue;

                    var rowProperties = GetRowProperties(properties, bindableAttribute, i);
                    SetupEditor(value, rowProperties);

                    //Increase the amount of properties already used by this row
                    i += (rowProperties.Length - 1);
                }
            }

            private PropertyInfo[] GetRowProperties(PropertyInfo[] properties, BindGUI att, int startIndex)
            {
                List<PropertyInfo> rowProperties = new List<PropertyInfo>();
                rowProperties.Add(properties[startIndex]);

                int currentColumnIndex = att.ColumnIndex;
                int nextPropertyIndex = startIndex + 1;
                if (currentColumnIndex != -1)
                {
                    while (nextPropertyIndex < properties.Length)
                    {
                        var nextColumnAttribute = properties[nextPropertyIndex].GetCustomAttribute<BindGUI>();
                        if (nextColumnAttribute != null && nextColumnAttribute.ColumnIndex > currentColumnIndex)
                        {
                            currentColumnIndex = nextColumnAttribute.ColumnIndex;
                            rowProperties.Add(properties[nextPropertyIndex]);
                        }
                        else
                            break;

                        nextPropertyIndex++;
                    }
                }
                return rowProperties.ToArray();
            }

            private void SetupEditor(object value, FieldInfo field)
            {
                Console.WriteLine($"field {field.Name}");
            }

            private void SetupEditor(object value, PropertyInfo[] properties)
            {
                string category = "";
                int height = 23;
                int columnIndex = 0;

                List<Tuple<Control, int, int>> controls = new List<Tuple<Control, int, int>>();

                //Go through each property and fill an entire row
                foreach (var property in properties)
                {
                    //Get our custom attributes
                    //Bind attribute for labels. Categories for dropdowns
                    var bindableAttribute = property.GetCustomAttribute<BindGUI>();
                    var categoryAttribute = property.GetCustomAttribute<BindCategory>();
                    if (categoryAttribute != null)
                        category = categoryAttribute.Label;

                    //Check if a category is set
                    if (category != string.Empty && !DropdownPanels.ContainsKey(category))
                    {
                        //Make a dropdown and fill it in a flow layout
                        var dropPanel = new DropdownPanel();
                        dropPanel.Height = 22;
                        dropPanel.PanelName = category;
                        DropdownPanels.Add(category, dropPanel);

                        DropFlowPanel = new FlowLayoutPanel();
                        DropFlowPanel.Dock = DockStyle.Fill;
                        dropPanel.AddControl(DropFlowPanel);
                        FlowPanel.Controls.Add(dropPanel);
                    }

                    //Create bindable properties for GUI
                    if (bindableAttribute != null)
                    {
                        if (bindableAttribute.Label != string.Empty)
                        {
                            controls.Add(Tuple.Create(CreateLabel(bindableAttribute.Label + ":"), columnIndex, 0));
                            columnIndex++;
                        }

                        var control = CreateBindedControl(value, property);
                        if (control != null)
                            controls.Add(Tuple.Create(control, columnIndex, 0));
                    }
                    //Classes can also have custom control classes for more customization
                    //These should be used if the control needs to be edited dynamically
                    var customcontrol = CreateCustomControl(value, property);
                    if (customcontrol != null)
                        controls.Add(Tuple.Create(customcontrol, columnIndex, 0));

                    columnIndex++;
                }

                STPanel panel = new STPanel();
                panel.Height = height;
                panel.Width = Control.Width;
                panel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                Table = CreateTableLayout(columnIndex + 1);
                Table.Dock = DockStyle.Fill;
                panel.Controls.Add(Table);

                foreach (var ctrl in controls)
                    Table.Controls.Add(ctrl.Item1, ctrl.Item2, ctrl.Item3);

                if (DropdownPanels.ContainsKey(category))
                {
                    DropdownPanels[category].Height += panel.Height + 8;
                    DropFlowPanel.Controls.Add(panel);
                }
                else
                    FlowPanel.Controls.Add(panel);
            }

            private Control CreateBindedControl(object value, PropertyInfo property)
            {
                Console.WriteLine($"property {property.Name} {property.PropertyType}");

                if (property.PropertyType.IsEnum)
                    return CreateComboBox(value, property);
                else if (property.PropertyType == typeof(STColor))
                    return CreateColorBox(value, property);
                else if (property.PropertyType == typeof(STColor8))
                    return CreateColorBox(value, property);
                else if (property.PropertyType == typeof(STColor16))
                    return CreateColorBox(value, property);
                else if (property.PropertyType == typeof(bool))
                    return CreateCheckBox(value, property);
                else if (property.PropertyType == typeof(string))
                    return CreateLabel(value, property);
                else if (property.PropertyType == typeof(float) ||
                    property.PropertyType == typeof(decimal) ||
                    property.PropertyType == typeof(uint) ||
                    property.PropertyType == typeof(int))
                {
                    return CreateNumberBox(value, property);
                }
                else
                    return null;
            }

            private Control CreateCustomControl(object value, PropertyInfo property)
            {
                if (property.PropertyType == typeof(Controls.ComboBox))
                    return CreateComboBox((Controls.ComboBox)property.GetValue(value));
                else if (property.PropertyType == typeof(Controls.NumericUpDown))
                    return CreateNumberBox((Controls.NumericUpDown)property.GetValue(value));
                else if (property.PropertyType == typeof(Controls.Checkbox))
                    return CreateCheckBox((Controls.Checkbox)property.GetValue(value));
                else
                    return null;
            }

            private Control CreateComboBox(Controls.ComboBox comboBox)
            {
                STComboBox control = new STComboBox();
                control.Anchor = AnchorStyles.Right | AnchorStyles.Left;
                control.Width = 300;
                control.Dock = DockStyle.Fill;
                control.SelectedIndexChanged += (sender, args) => {
                    comboBox.SetValue(control.SelectedItem);
                };

                foreach (var enumValue in comboBox.GetValues())
                    control.Items.Add(enumValue);

                control.SelectedItem = comboBox.SelectedValue;
                return control;
            }

            private Control CreateComboBox(object value, PropertyInfo property)
            {
                STComboBox comboBox = new STComboBox();
                comboBox.Anchor = AnchorStyles.Right | AnchorStyles.Left;
                comboBox.Width = 300;
                comboBox.Dock = DockStyle.Fill;
                comboBox.SelectedIndexChanged += (sender, args) => {
                    value = Enum.Parse(property.PropertyType, comboBox.SelectedItem.ToString());
                };

                comboBox.DataSource = Enum.GetValues(property.PropertyType);
                comboBox.DataBindings.Add(new Binding("SelectedItem", value, property.Name, false, DataSourceUpdateMode.OnPropertyChanged));
                return comboBox;
            }

            private Control CreateLabel(object value, PropertyInfo property)
            {

                var label = new STLabel()
                {
                    AutoSize = true,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Left,
                };
                var labelControl = new LabelControl(value, property);
                label.DataBindings.Add(new Binding("Text", labelControl, "Text", false, DataSourceUpdateMode.OnPropertyChanged));
                return label;
            }

            private Control CreateComboBox(Controls.NumericUpDown numericUD)
            {
                STNumbericUpDown control = new STNumbericUpDown();
                control.Anchor = AnchorStyles.Right | AnchorStyles.Left;
                control.Width = 300;
                control.Dock = DockStyle.Fill;
                control.Maximum = numericUD.Maximum;
                control.Minimum = numericUD.Minimum;
                control.Bind(numericUD, "Value");
                control.ValueChanged += (sender, args) => {
                    numericUD.SetValue(control.Value);
                };

                return control;
            }

            private Control CreateColorBox(object value, PropertyInfo property)
            {
                ColorControl colorCtrl = new ColorControl(value, property);

                PictureBox pb = new PictureBox();
                pb.DataBindings.Add("BackColor", colorCtrl.Color, "Color", false, DataSourceUpdateMode.OnPropertyChanged);

                pb.Width = 22;
                pb.Height = 22;
                pb.Click += (sender, args) => {
                    STColorDialog dlg = new STColorDialog(pb.BackColor);
                    dlg.ColorChanged += (s, a) => {
                        pb.BackColor = dlg.NewColor;
                        colorCtrl.Color = new STColor8(dlg.NewColor);
                    };
                    dlg.Show();
                };

                return pb;
            }

            private Control CreateNumberBox(object value, PropertyInfo property)
            {
                STNumbericUpDown control = new STNumbericUpDown();
                var minmaxAttribute = property.GetCustomAttribute<ControlAttributes.MinMax>();
                if (minmaxAttribute != null)
                {
                    control.Maximum = minmaxAttribute.Max;
                    control.Minimum = minmaxAttribute.Min;
                }
                Console.WriteLine($"BIND {value} PROPERTY {property.Name}");
                control.DataBindings.Add("Value", value, property.Name, false, DataSourceUpdateMode.OnPropertyChanged);
                return control;
            }

            private Control CreateNumberBox(Controls.NumericUpDown numericUD)
            {
                STNumbericUpDown control = new STNumbericUpDown();
                control.DataBindings.Add("Maximum", numericUD, "Maximum", false, DataSourceUpdateMode.OnPropertyChanged);
                control.DataBindings.Add("Minimum", numericUD, "Minimum", false, DataSourceUpdateMode.OnPropertyChanged);
                control.DataBindings.Add("Value", numericUD, "Value", false, DataSourceUpdateMode.OnPropertyChanged);
                control.DataBindings.Add("Enabled", numericUD, "Enabled", false, DataSourceUpdateMode.OnPropertyChanged);
                control.DataBindings.Add("Visible", numericUD, "Visible", false, DataSourceUpdateMode.OnPropertyChanged);
                return control;
            }

            private Control CreateCheckBox(Controls.Checkbox checkBox)
            {
                STCheckBox control = new STCheckBox();
                control.AutoSize = true;
                control.Size = new System.Drawing.Size(65, 17);
                control.UseVisualStyleBackColor = true;
                control.Text = checkBox.Text;
                control.Enabled = checkBox.Enabled;
                control.Visible = checkBox.Visible;
                control.Bind(checkBox, "Checked");
                return control;
            }

            private Control CreateCheckBox(object value, PropertyInfo property)
            {
                STCheckBox control = new STCheckBox();
                control.AutoSize = true;
                control.Size = new System.Drawing.Size(65, 17);
                control.UseVisualStyleBackColor = true;
                control.Text = "";
                control.Bind(value, property.Name);
                return control;
            }

            private Control CreateLabel(string text)
            {
                return new STLabel()
                {
                    AutoSize = true,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                    Text = text,
                    Dock = DockStyle.Left,
                };
            }

            public static TableLayoutPanel CreateTableLayout(int columnCount)
            {
                var tableLayout = new TableLayoutPanel()
                {
                    RowCount = 1,
                    ColumnCount = columnCount,
                    CellBorderStyle = TableLayoutPanelCellBorderStyle.None
                };

                tableLayout.SuspendLayout();
                SetColumnStyles(tableLayout, columnCount);
                tableLayout.ResumeLayout();

                return tableLayout;
            }

            private static void SetColumnStyles(TableLayoutPanel tableLayout, int columnCount)
            {
                tableLayout.ColumnStyles.Clear();
                var size = 60;
                for (int i = 0; i < columnCount; i++)
                {
                    if (i == 0)
                        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
                    else if (i == columnCount - 1)
                        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
                    else
                        tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, size));
                }
            }

        }
    }
}
