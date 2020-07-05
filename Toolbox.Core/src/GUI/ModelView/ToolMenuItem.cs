using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core.ModelView
{
    public class ToolMenuItem
    {
        public string Name { get; set; }

        public bool Enabled { get; set; } = true;

        public object Tag { get; set; }

        public EventHandler Click;

        public List<ToolMenuItem> Children = new List<ToolMenuItem>();

        public ToolMenuItem(string name) { Name = name; }

        public ToolMenuItem(string name, EventHandler eventHandler) {
            Name = name;
            Click += eventHandler;
        }
    }

    public class ToolMenuItemSeparator : ToolMenuItem
    {
        public ToolMenuItemSeparator() : base("")
        {
        }
    }

}
