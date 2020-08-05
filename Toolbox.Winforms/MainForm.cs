using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using STLibrary.Forms;
using Toolbox.Core;
using Toolbox.Core.IO;
using System.Runtime.InteropServices;

namespace Toolbox.Winforms
{
    public partial class MainForm : Form
    {
        private static MainForm _instance;
        public static MainForm Instance { get { return _instance; } }

        public MainForm()
        {
            InitializeComponent();

            var fileFormats = FileManager.GetFileFormats();
            BackColor = FormThemes.BaseTheme.MDIParentBackColor;
            Runtime.OpenTKInitialized = true;
            _instance = this;
        }

        public static void SetProgressBar(string text, int amount, bool continuous = false) {
            ProgressWindow.Update(text, amount, continuous);
        }

        public static void HideProgressBar() {
            ProgressWindow.CloseProgressBar();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (Runtime.UseOpenGL) {
                STLibrary.Forms.Rendering.OpenTKHelper.CreateContext();
            }

            LoadMDITheme();
            LoadRecentList();
            OnMdiWindowClosed();
            ResetMenus();

            foreach (var menu in CompressionMenu.GetMenuItems()) {
                var item = new STToolStipMenuItem(menu.Name, null, menu.Click) { Enabled = menu.Enabled };
                compressionToolStripMenuItem.DropDownItems.Add(item);
                foreach (var child in menu.Children)
                    item.DropDownItems.Add(new STToolStipMenuItem(child.Name, null, child.Click) { Enabled = child.Enabled });
            }
        }

        private void pluginsToolStripMenuItem_Click(object sender, EventArgs e) {
            PluginWindow window = new PluginWindow();
            window.Show();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK) {
                foreach (var file in ofd.FileNames)
                    OpenFileBackground(file);
            }
        }

        private void OpenFileBackground(string fileName)
        {
            if (!File.Exists(fileName))
                return;

            SaveRecentFile(fileName);

            // settings = STFileLoader.TryDecompressFile(File.OpenRead(fileName), fileName);
            var file = STFileLoader.OpenFileFormat(fileName);

            //Check if the file has compression or not
            //Load compression format only if the file is not supported
            /*if (file == null && settings.CompressionFormat != null)
            {
                if (ActiveMdiChild is ObjectView && Runtime.ObjectEditor.OpenInActiveEditor)
                {
                    ((ObjectView)ActiveMdiChild).LoadFormat(settings.CompressionFormat, settings.Stream, fileName);
                }
                else
                {
                    ObjectView view = new ObjectView();
                    view.Text = CheckTabDupes(Path.GetFileName(fileName));
                    view.LoadFormat(settings.CompressionFormat, settings.Stream, fileName);
                    view.MdiParent = this;
                    view.Show();
                }
                return;
            }*/

            if (file == null)
            {
                MessageBox.Show("File format is not supported!");
                return;
            }

            ReloadFileMenus(file);
            if (ActiveMdiChild is ObjectView && Runtime.ObjectEditor.OpenInActiveEditor)
            {
                ((ObjectView)ActiveMdiChild).LoadFormat(file);
            }
            else
            {
                ObjectView view = new ObjectView();
                view.Text = CheckTabDupes(file.FileInfo.FileName);
                view.LoadFormat(file);
                view.MdiParent = this;
                view.Show();
            }
        }

        static IFileFormat OpenFileAsync(string fileName)
        {
            return STFileLoader.OpenFileFormat(fileName);

          /*  var file = await Task.FromResult(STFileLoader.OpenFileFormat(fileName));
            return file;*/
        }

        private void OpenFile(string fileName)
        {
            if (File.Exists(fileName))
                SaveRecentFile(fileName);

            IFileFormat file = STFileLoader.OpenFileFormat(fileName);
            if (file == null) return;

            ReloadFileMenus(file);

            if (ActiveMdiChild is ObjectView && Runtime.ObjectEditor.OpenInActiveEditor)
            {
                ((ObjectView)ActiveMdiChild).LoadFormat(file);
            }
            else
            {
                ObjectView view = new ObjectView();
                view.MdiParent = this;
                view.Text = CheckTabDupes(file.FileInfo.FileName);
                view.LoadFormat(file);
                view.Show();
            }
        }

        private void ReloadFileMenus(IFileFormat fileFormat) {
            saveAsToolStripMenuItem.Enabled = fileFormat.CanSave;
            saveToolStripMenuItem.Enabled = fileFormat.CanSave;                
        }

        private void SaveActiveFiles(bool useDialog = false)
        {
            var files = GetActiveFiles();
            foreach (var file in files)
                SaveFileFormat(file);

            MessageBox.Show("Files saved!");
        }

        private void SaveFileFormat(IFileFormat fileFormat, bool useDialog = false)
        {
            string filePath = fileFormat.FileInfo.FilePath;
            if (useDialog)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.DefaultExt = Path.GetExtension(fileFormat.FileInfo.FileName);
                sfd.FileName = fileFormat.FileInfo.FileName;
                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                filePath = sfd.FileName;
            }

            STFileSaver.SaveFileFormat(fileFormat, filePath);
            if (ActiveMdiChild is ObjectView && fileFormat.FileInfo.KeepOpen)
                ((ObjectView)ActiveMdiChild).ReloadFile(fileFormat);
        }

        private IFileFormat[] GetActiveFiles()
        {
            if (ActiveMdiChild is IFileEditor)
                return ((IFileEditor)ActiveMdiChild).GetFileFormats();

            return new IFileFormat[0];
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (!Runtime.EnableDragDrop) return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
            {
                String[] strGetFormats = e.Data.GetFormats();
                e.Effect = DragDropEffects.None;
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (!Runtime.EnableDragDrop) return;

            Cursor.Current = Cursors.WaitCursor;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string filename in files)
            {
                OpenFileBackground(filename);
            }

            Cursor.Current = Cursors.Default;
        }

        #region MDI Windows

        private const int WM_SETREDRAW = 11;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);

        int TabDupeIndex = 0;
        private string CheckTabDupes(string Name)
        {
            foreach (TabPage tab in tabForms.TabPages)
            {
                if (tab.Text == Name)
                {
                    Name = Name + TabDupeIndex++;
                    return CheckTabDupes(Name);
                }
            }
            return Name;
        }

        private void tabForms_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) {
                this.tabControlContextMenuStrip.Show(this.tabForms, e.Location);
            }
        }

        bool IsChanged = false;
        private void tabForms_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (TabPage tpCheck in tabForms.TabPages)
                tpCheck.BackColor = FormThemes.BaseTheme.TabPageInactive;

          // SetFormatSettings(GetActiveIFileFormat());

            IsChanged = true;

            if (tabForms.SelectedTab != null)
            {
                tabForms.SelectedTab.BackColor = FormThemes.BaseTheme.TabPageActive;
            }

            if ((tabForms.SelectedTab != null) &&
                (tabForms.SelectedTab.Tag != null))
            {
                SendMessage(this.Handle, WM_SETREDRAW, false, 0);
                (tabForms.SelectedTab.Tag as Form).Select();
                SendMessage(this.Handle, WM_SETREDRAW, true, 0);
                this.Refresh();
            }
        }

        private void MainForm_MdiChildActivate(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild == null)
            {
                tabForms.Visible = false;
                ResetMenus();
            }
            // If no any child form, hide tabControl 
            else
            {
                if (IsChanged)
                {
                    ResetAnimPanel();
                    IsChanged = false;
                }

                // If child form is new and no has tabPage, 
                // create new tabPage 
                if (this.ActiveMdiChild.Tag == null)
                {
                    if (Runtime.GUI.MaximizeMdiWindow && ActiveMdiChild.WindowState != FormWindowState.Maximized)
                    {
                        ((STForm)this.ActiveMdiChild).Maximize();
                    }

                    int tpIndex = 0;
                    foreach (TabPage tpCheck in tabForms.TabPages)
                    {
                        if (tpCheck.Text == this.ActiveMdiChild.Text)
                        {
                            tabForms.SelectedIndex = tpIndex;
                            return;
                        }
                        tpIndex++;
                    }

                    // Add a tabPage to tabControl with child 
                    // form caption 
                    TabPage tp = new TabPage(this.ActiveMdiChild.Text);
                    tp.BackColor = FormThemes.BaseTheme.TabPageInactive;
                    tp.ForeColor = FormThemes.BaseTheme.FormContextMenuForeColor;
                    tp.Tag = this.ActiveMdiChild;
                    tp.Parent = tabForms;

                    tabForms.SelectedTab = tp;
                    tabForms.SelectedTab.BackColor = FormThemes.BaseTheme.TabPageActive;

                    this.ActiveMdiChild.Tag = tp;
                    this.ActiveMdiChild.FormClosed +=
                        new FormClosedEventHandler(ActiveMdiChild_FormClosed);
                    this.ActiveMdiChild.SizeChanged +=
                        new EventHandler(ActiveMdiChild_StateChanged);
                }
                else
                {
                    //Select a tab if it has a tag
                    int tpIndex = 0;
                    foreach (TabPage tpCheck in tabForms.TabPages)
                    {
                        if (tpCheck.Tag == this.ActiveMdiChild)
                        {
                            tabForms.SelectedIndex = tpIndex;

                            if (ActiveMdiChild is STForm)
                            {
                                if (ActiveMdiChild.WindowState == FormWindowState.Maximized)
                                {
                                    ((STForm)ActiveMdiChild).MDIMaximized();
                                }
                            }

                            return;
                        }
                        tpIndex++;
                    }
                }

                if (!tabForms.Visible) tabForms.Visible = true;

            }
        }

        private void ResetAnimPanel()
        {
        }

        private void ResetMenus()
        {
            saveAsToolStripMenuItem.Enabled = false;
            saveToolStripMenuItem.Enabled = false;
        }

        private void ActiveMdiChild_StateChanged(object sender, EventArgs e)
        {
            if (ActiveMdiChild == null)
                return;

            if (ActiveMdiChild.WindowState == FormWindowState.Maximized)
                OnMdiWindowActived();
            else
                OnMdiWindowClosed();
        }

        private void BtnMdiClose_MouseEnter(object sender, System.EventArgs e)
        {
            BtnMdiClose.Image = STLibraryForms.Properties.Resources.close_selected;
        }

        private void BtnMdiClose_MouseLeave(object sender, System.EventArgs e)
        {
            BtnMdiClose.Image = STLibraryForms.Properties.Resources.close;
        }

        private void BtnMdiMinMax_MouseEnter(object sender, EventArgs e)
        {
            BtnMdiMinMax.Image = STLibraryForms.Properties.Resources.maximize_selected;
        }

        private void BtnMdiMinMax_MouseLeave(object sender, EventArgs e)
        {
            BtnMdiMinMax.Image = STLibraryForms.Properties.Resources.maximize;
        }

        private void BtnMdiMinimize_MouseEnter(object sender, EventArgs e)
        {
            BtnMdiMinimize.Image = STLibraryForms.Properties.Resources.minimize_selected;
        }

        private void BtnMdiMinimize_MouseLeave(object sender, EventArgs e)
        {
            BtnMdiMinimize.Image = STLibraryForms.Properties.Resources.minimize;
        }


        private void ActiveMdiChild_FormClosed(object sender,
                            FormClosedEventArgs e)
        {
            ((sender as Form).Tag as TabPage).Dispose();
        }

        private void OnMdiWindowClosed()
        {
            BtnMdiClose.Visible = false;
            BtnMdiMinMax.Visible = false;
            BtnMdiMinimize.Visible = false;
        }

        private void OnMdiWindowActived()
        {
            BtnMdiClose.Visible = true;
            BtnMdiMinMax.Visible = true;
            BtnMdiMinimize.Visible = true;
        }

        private void LoadMDITheme()
        {
            MDIController.MDIClientSupport.SetBevel(this, false);
            MdiClient ctlMDI;

            // Loop through all of the form's controls looking
            // for the control of type MdiClient.
            foreach (Control ctl in this.Controls)
            {
                try
                {
                    // Attempt to cast the control to type MdiClient.
                    ctlMDI = (MdiClient)ctl;

                    // Set the BackColor of the MdiClient control.
                    ctlMDI.BackColor = FormThemes.BaseTheme.MDIParentBackColor;
                }
                catch (InvalidCastException exc)
                {
                    // Catch and ignore the error if casting failed.
                }
            }
        }

        #endregion

        private void BtnMdiMinimize_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild != null)
                ActiveMdiChild.WindowState = FormWindowState.Minimized;
        }

        private void BtnMdiMinMax_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);

            foreach (STForm frm in this.MdiChildren) frm.MDIWindowed();
        }

        private void BtnMdiClose_Click(object sender, EventArgs e)
        {
            foreach (var child in this.MdiChildren)
            {
                if (child == tabForms.SelectedTab.Tag)
                {
                    OnMdiWindowClosed();
                    child.Close();
                    /*  GC.Collect();
                      RenderTools.DisposeTextures();*/
                    return;
                }
            }
        }

        FormWindowState LastWindowState = FormWindowState.Minimized;
        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState != LastWindowState)
                LastWindowState = WindowState;

            if (WindowState == FormWindowState.Maximized)
            {
                Runtime.GUI.MaximizeMdiWindow = true;
            }
            if (WindowState == FormWindowState.Normal)
            {
                Runtime.GUI.MaximizeMdiWindow = false;
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S) // Ctrl + S Save
            {
                e.SuppressKeyPress = true;
                SaveActiveFiles(false);
            }
            if (e.Control && e.Alt && e.KeyCode == Keys.S) // Ctrl + Alt + S Save As
            {
                e.SuppressKeyPress = true;
                SaveActiveFiles(true);
            }
            else if (e.Control && e.KeyCode == Keys.W) // Ctrl + W Exit
            {
                e.SuppressKeyPress = true;
                var notify = MessageBox.Show("Are you sure you want to exit the application?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (notify == DialogResult.OK)
                    Application.Exit();
            }
        }

        private void cascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
            foreach (STForm frm in this.MdiChildren) frm.MDIWindowed();
        }

        private void minimizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild != null)
                ActiveMdiChild.WindowState = FormWindowState.Minimized;
        }

        private void maximizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild != null)
                ActiveMdiChild.WindowState = FormWindowState.Maximized;
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild != null)
                ActiveMdiChild.Close();

            OnMdiWindowClosed();

            //Force garbage collection.
            GC.Collect();

            // Wait for all finalizers to complete before continuing.
            GC.WaitForPendingFinalizers();
        }

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form frm in this.MdiChildren) frm.Close();

            OnMdiWindowClosed();

            RenderTools.DisposeTextures();

            //Force garbage collection.
            GC.Collect();

            // Wait for all finalizers to complete before continuing.
            GC.WaitForPendingFinalizers();
        }


        #region Recent Files

        List<string> RecentFiles = new List<string>();

        const int MRUnumber = 6;
        private void SaveRecentFile(string path)
        {
            Console.WriteLine($"SaveRecentFile {path}");

            recentToolStripMenuItem.DropDownItems.Clear();
            LoadRecentList(); //load list from file
            if (!(RecentFiles.Contains(path))) //prevent duplication on recent list
                RecentFiles.Insert(0, path); //insert given path into list

            //keep list number not exceeded the given value
            while (RecentFiles.Count > MRUnumber)
            {
                RecentFiles.RemoveAt(MRUnumber);
            }
            foreach (string item in RecentFiles)
            {
                //create new menu for each item in list
                STToolStripItem fileRecent = new STToolStripItem();
                fileRecent.Click += RecentFile_click;
                fileRecent.Text = item;
                fileRecent.Size = new System.Drawing.Size(170, 40);
                fileRecent.AutoSize = true;
                fileRecent.Image = null;

                //add the menu to "recent" menu
                recentToolStripMenuItem.DropDownItems.Add(fileRecent);
            }
            //writing menu list to file
            //create file called "Recent.txt" located on app folder
            StreamWriter stringToWrite =
            new StreamWriter(Runtime.ExecutableDir + "\\Recent.txt");
            foreach (string item in RecentFiles)
            {
                stringToWrite.WriteLine(item); //write list to stream
            }
            stringToWrite.Flush(); //write stream to file
            stringToWrite.Close(); //close the stream and reclaim memory
        }
        private void LoadRecentList()
        {//try to load file. If file isn't found, do nothing
            RecentFiles.Clear();

            if (File.Exists(Runtime.ExecutableDir + "\\Recent.txt"))
            {
                StreamReader listToRead = new StreamReader(Runtime.ExecutableDir + "\\Recent.txt"); //read file stream
                string line;
                while ((line = listToRead.ReadLine()) != null) //read each line until end of file
                {
                    if (File.Exists(line))
                        RecentFiles.Add(line); //insert to list
                }
                listToRead.Close(); //close the stream
            }
            foreach (string item in RecentFiles)
            {
                STToolStripItem fileRecent = new STToolStripItem();
                fileRecent.Click += RecentFile_click;
                fileRecent.Text = item;
                fileRecent.Size = new System.Drawing.Size(170, 40);
                fileRecent.AutoSize = true;
                fileRecent.Image = null;
                recentToolStripMenuItem.DropDownItems.Add(fileRecent); //add the menu to "recent" menu
            }
        }

        private void RecentFile_click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            OpenFile(sender.ToString());
            Cursor.Current = Cursors.Default;
        }

        #endregion

        private void consoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConsoleWindow console = new ConsoleWindow();
            console.Show(this);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }
    }
}
