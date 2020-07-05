using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using STLibrary.Forms;
using Toolbox.Core;

namespace Toolbox.Winforms
{
    //The panel when a file in an archive is clicked on
    //Configures the editor view, ie Hex, Editor, Text Edtior, etc
    public partial class ArchiveFilePanel : UserControl
    {
        ObjectView ParentObjectView;

        ArchiveFileInfo ArchiveFileInfo;
        IArchiveFile ArchiveFile;

        private bool _IsLoaded = false;

        public ArchiveFilePanel()
        {
            InitializeComponent();

            ReloadEditors();
            _IsLoaded = true;
            saveBtn.Visible = false;
        }

        public void LoadFile(ObjectView parent, ArchiveFileInfo archiveFileInfo, IArchiveFile archiveFile)
        {
            ParentObjectView = parent;
            ArchiveFileInfo = archiveFileInfo;
            ArchiveFile = archiveFile;
            UpdateEditor();
        }

        public void LoadFile(ObjectView parent, ArchiveFileInfo archiveFileInfo)
        {
            ParentObjectView = parent;
            ArchiveFileInfo = archiveFileInfo;
            UpdateEditor();
        }

        private void ReloadEditors()
        {
            stComboBox1.Items.Clear();
            stComboBox1.Items.Add("Properties");
            stComboBox1.Items.Add("Hex Editor");
            stComboBox1.Items.Add("File Editor");
            stComboBox1.Items.Add("Text Editor");

            if (Runtime.ObjectEditor.EditorSelectedIndex < stComboBox1.Items.Count)
                stComboBox1.SelectedIndex = Runtime.ObjectEditor.EditorSelectedIndex;
            else
                stComboBox1.SelectedIndex = 0;
        }

        public void SetEditor(int Index) { stComboBox1.SelectedIndex = Index; }
        public int GetEditor() { return stComboBox1.SelectedIndex; }

        public void UpdateEditor()
        {
            if (GetEditor() == 0)
                UpdatePropertiesView();
            else if (GetEditor() == 1)
                UpdateHexView();
            else if (GetEditor() == 2)
                UpdateFileEditor();
            else if (GetEditor() == 3)
                UpdateTextView();

            if (GetEditor() == 2 || GetEditor() == 3)
                saveBtn.Visible = true;
            else
                saveBtn.Visible = false;
        }

        private void UpdateFileEditor()
        {
            var File = ArchiveFileInfo.FileFormat;
            if (File == null) //If the file is not open yet, try temporarily for a preview
                File = ArchiveFileInfo.OpenFile();

            Console.WriteLine($"file active? {File != null}");

            //If the file is still null, just add a basic control and return
            if (File == null)
            {
                GetActiveEditor<STUserControl>();
                return;
            }

            if (ArchiveFile != null)
                File.FileInfo.ParentArchive = ArchiveFile;

            ArchiveFileInfo.FileFormat = File;
            SetEditorForm(File);
        }

        private bool CheckActiveType(Type type)
        {
            return stPanel1.Controls.Count > 0 && stPanel1.Controls[0].GetType() != type;
        }

        public void SetEditorForm(IFileFormat fileFormat)
        {
            if (fileFormat == null)
            {
                GetActiveEditor<STUserControl>();
                return;
            }
            //Todo find a way to handle file editors
            if (fileFormat is IModelFormat)
            {
                if (ParentObjectView.WorkspacePanel == null)
                    ParentObjectView.WorkspacePanel = new WorkspacePanel() { Dock = DockStyle.Fill };

                WorkspacePanel workspace = GetActiveEditor<WorkspacePanel>(ParentObjectView.WorkspacePanel);
                workspace.LoadFileFormat((IModelFormat)fileFormat);
                workspace.UpdateViewport();
            }
            //Todo find a way to handle file editors
            if (fileFormat is STGenericTexture)
            {
                ImageEditorBase editor = GetActiveEditor<ImageEditorBase>();
                editor.LoadProperties(((STGenericTexture)fileFormat).DisplayProperties);
                editor.LoadImage((STGenericTexture)fileFormat);
            }
        }

        private void UpdateTextView()
        {
            TextEditor editor = GetActiveEditor<TextEditor>();
            editor.Text = Text;

            var File = ArchiveFileInfo.FileFormat;
            if (File == null)
                File = ArchiveFileInfo.OpenFile();

            if (File != null && IsConvertableText(File.GetType()))
            {
                editor.FillEditor(((IConvertableTextFormat)File).ConvertToString());
                if (((IConvertableTextFormat)File).TextFileType == TextFileType.Yaml)
                    editor.LoadSyntaxYAML();

                if (((IConvertableTextFormat)File).TextFileType == TextFileType.Xml)
                    editor.LoadSyntaxXML();

                if (((IConvertableTextFormat)File).TextFileType == TextFileType.Json)
                    editor.LoadSyntaxJson();
            }
            else if (ArchiveFileInfo.FileData != null)
                editor.FillEditor(ArchiveFileInfo.FileData);

            ArchiveFileInfo.FileFormat = File;
        }

        private void NotifyFormatSwitched()
        {

        }

        private void SaveTextFormat()
        {

        }

        private bool IsConvertableText(Type type)
        {
            return typeof(IConvertableTextFormat).IsAssignableFrom(type);
        }

        private void UpdatePropertiesView()
        {
            STPropertyGrid editor = GetActiveEditor<STPropertyGrid>();
            editor.Text = Text;
          //  editor.LoadProperty(ArchiveFileInfo.DisplayProperties);
        }

        private void UpdateHexView()
        {
            HexEditor editor = GetActiveEditor<HexEditor>();
            editor.Text = Text;

            if (ArchiveFileInfo.FileData != null)
                editor.LoadData(ArchiveFileInfo.DecompressData(ArchiveFileInfo.FileData));
            editor.Refresh();
        }

        private Control ActiveEditor
        {
            get
            {
                if (stPanel1.Controls.Count == 0) return null;
                return stPanel1.Controls[0];
            }
        }

        private T GetActiveEditor<T>(Control control = null) where T : Control, new()
        {
            T instance = new T();

            if (ActiveEditor?.GetType() == instance.GetType())
                return ActiveEditor as T;
            else
            {
                DisposeEdtiors();
                stPanel1.Controls.Clear();
                instance.Dock = DockStyle.Fill;

                if (control != null)
                    stPanel1.Controls.Add(control);
                else
                    stPanel1.Controls.Add(instance);
            }

            return instance;
        }

        private void DisposeEdtiors()
        {
            if (ActiveEditor == null) return;
            if (ActiveEditor is STUserControl)
                ((STUserControl)ActiveEditor).OnControlClosing();
        }

        private void stComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_IsLoaded && stComboBox1.SelectedIndex != -1) {
                Runtime.ObjectEditor.EditorSelectedIndex = stComboBox1.SelectedIndex;
                UpdateEditor();
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            bool IsTextEditor = GetEditor() == 3;

            var File = ArchiveFileInfo.FileFormat;
            if (IsTextEditor && File != null && IsConvertableText(File.GetType()))
            {
                if (((IConvertableTextFormat)File).CanConvertBack)
                {
                    TextEditor editor = GetActiveEditor<TextEditor>();
                    ((IConvertableTextFormat)File).ConvertFromString(editor.GetText());

                    ArchiveFileInfo.SaveFileFormat();
                    MessageBox.Show($"Saved {File.FileInfo.FileName} to archive!");
                }
                else
                {
                    MessageBox.Show($"File format does not support converting back from type: {((IConvertableTextFormat)File).TextFileType}!");
                }
            }
            else if (File != null && File.CanSave)
            {
                ArchiveFileInfo.SaveFileFormat();
                MessageBox.Show($"Saved {File.FileInfo.FileName} to archive!");
            }
            else
                MessageBox.Show($"File format does not support saving!");
        }
    }
}
