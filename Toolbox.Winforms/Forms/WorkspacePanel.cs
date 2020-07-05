using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Toolbox.Core;
using Toolbox.Core.Animations;
using STLibrary.Forms;

namespace Toolbox.Winforms
{
    public partial class WorkspacePanel : UserControl
    {
        public Viewport Viewport;

        public WorkspacePanel()
        {
            InitializeComponent();

            Viewport = new Viewport() { Dock = DockStyle.Fill };
            stPanel2.Controls.Add(Viewport);
        }

        public void LoadFileFormat(IModelSceneFormat fileFormat) {
            Viewport.LoadModelFormat(fileFormat);
        }

        public void LoadFileFormat(IModelFormat fileFormat) {
            Viewport.LoadModelFormat(fileFormat);
        }

        public void LoadFileFormat(STAnimation fileFormat) {
            Viewport.LoadAnimationFormat(fileFormat);
        }

        public void UpdateViewport() {
            Viewport.UpdateViewport();
        }

        private Control ActiveEditor
        {
            get
            {
                if (contentPanel.Controls.Count == 0) return null;
                return contentPanel.Controls[0];
            }
        }

        public T GetActiveEditor<T>() where T : Control, new()
        {
            T instance = new T();

            if (ActiveEditor?.GetType() == instance.GetType())
                return ActiveEditor as T;
            else
            {
                DisposeEdtiors();
                contentPanel.Controls.Clear();
                instance.Dock = DockStyle.Fill;
                contentPanel.Controls.Add(instance);
            }

            return instance;
        }

        private void DisposeEdtiors()
        {
            if (ActiveEditor == null) return;
            if (ActiveEditor is STUserControl)
                ((STUserControl)ActiveEditor).OnControlClosing();
        }
    }
}
