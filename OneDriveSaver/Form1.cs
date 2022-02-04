using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using static OneDriveSaver.SymLinkHelper;

namespace OneDriveSaver
{
    public partial class Form1 : Form
    {
        private string onedrivePath, onedrivesavePath;
        private bool StartOnBoot, BackupOnStart, StartMinimized, CloseMinimises, ToastEnable, appClosing;

        private LibraryMgr m_LibraryManager;
        private ToastManager m_ToastManager;

        private FormWindowState prevWindowState;

        public Form1()
        {
            InitializeComponent();

            // check if onedrive is installed
            onedrivePath = Environment.GetEnvironmentVariable("OneDriveConsumer");

            if (!Directory.Exists(onedrivePath))
            {
                MessageBox.Show("OneDrive could not be found!");
                Application.Exit();
            }

            // settings
            StartOnBoot = Properties.Settings.Default.StartOnBoot;
            BackupOnStart = Properties.Settings.Default.BackupOnStart;
            StartMinimized = Properties.Settings.Default.StartMinimized;
            CloseMinimises = Properties.Settings.Default.CloseMinimises;
            ToastEnable = Properties.Settings.Default.ToastEnable;

            onedrivesavePath = Path.Combine(onedrivePath, "Saved Games");

            // update environment var
            Environment.SetEnvironmentVariable("OneDriveSavedGames", onedrivesavePath);

            // initialize Task Manager
            Utils.SetStartup(StartOnBoot, Application.ExecutablePath, "OneDriveSavedGames");

            // initialize toast manager
            m_ToastManager = new ToastManager("ControllerService");
            m_ToastManager.Enabled = ToastEnable;

            if (!Directory.Exists(onedrivesavePath))
                Directory.CreateDirectory(onedrivesavePath);

            if (BackupOnStart)
            {
                m_ToastManager.SendToast("Information", "Please wait while we create a backup of all your precious game saves.");
                DateTime localDate = DateTime.Now;
                string filename = $"SavedGames-{localDate.ToString("dd-MM-yyyy")}.zip";
                if (!File.Exists($"{onedrivePath}\\{filename}"))
                    ZipFile.CreateFromDirectory(onedrivesavePath, $"{onedrivePath}\\{filename}");
            }

            // initialize library manager
            m_LibraryManager = new LibraryMgr(onedrivesavePath);
            m_LibraryManager.Updated += UpdateSucceeded;
            m_LibraryManager.Failed += UpdateFailed;
            m_LibraryManager.Completed += UpdateCompleted;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // update Position and Size
            Size = new Size((int)Math.Max(this.MinimumSize.Width, Properties.Settings.Default.MainWindowWidth), (int)Math.Max(this.MinimumSize.Height, Properties.Settings.Default.MainWindowHeight));
            Location = new Point((int)Math.Max(0, Properties.Settings.Default.MainWindowX), (int)Math.Max(0, Properties.Settings.Default.MainWindowY));
            WindowState = StartMinimized ? FormWindowState.Minimized : (FormWindowState)Properties.Settings.Default.WindowState;

            m_LibraryManager.Process();
        }

        #region UI

        private void UpdateCompleted()
        {
            m_ToastManager.SendToast("Information", $"Library scan completed.");
        }

        public void UpdateSucceeded(Game game)
        {
            this.BeginInvoke((MethodInvoker)delegate ()
            {
                lB_Games.Items.Add(game);
            });
        }

        private void UpdateFailed(string fileName)
        {
            m_ToastManager.SendToast("Error", $"{fileName} could not be deserialized.");
        }

        #endregion

        private void Form1_Resize(object sender, EventArgs e)
        {
            switch (WindowState)
            {
                case FormWindowState.Minimized:
                    notifyIcon1.Visible = true;
                    ShowInTaskbar = false;
                    m_ToastManager.SendToast("Information", "The application is running in the background");
                    break;
                case FormWindowState.Normal:
                case FormWindowState.Maximized:
                    notifyIcon1.Visible = false;
                    ShowInTaskbar = true;
                    prevWindowState = WindowState;
                    break;
            }
        }

        private void Form1_Close(object sender, FormClosingEventArgs e)
        {
            // position and size settings
            switch (WindowState)
            {
                case FormWindowState.Normal:
                    Properties.Settings.Default.MainWindowX = (uint)Location.X;
                    Properties.Settings.Default.MainWindowY = (uint)Location.Y;

                    Properties.Settings.Default.MainWindowWidth = (uint)Size.Width;
                    Properties.Settings.Default.MainWindowHeight = (uint)Size.Height;
                    break;
            }
            Properties.Settings.Default.WindowState = (int)WindowState;

            if (CloseMinimises && e.CloseReason == CloseReason.UserClosing && !appClosing)
            {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
            }

            Properties.Settings.Default.Save();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            WindowState = FormWindowState.Normal;
        }

        private void b_CreateProfile_Click(object sender, EventArgs e)
        {
            // IMPLEMENT ME !
            return;
        }

        private void lB_Games_SelectedIndexChanged(object sender, EventArgs e)
        {
            Game game = (Game)lB_Games.SelectedItem;
            if (game == null)
                return;

            this.BeginInvoke((MethodInvoker)delegate ()
            {
                tB_ProfileName.Text = game.Name;
                tB_ProfilePath.Text = game.m_Path;

                treeView1.Nodes.Clear();

                Dictionary<GameSettings, TreeNode> settings = new Dictionary<GameSettings, TreeNode>();

                foreach(var pair in game.Settings)
                {
                    GameSettings setting = pair.Value;
                    string key = pair.Key;
                    TreeNode Node = new TreeNode(setting.path, (int)setting.type, (int)setting.type);
                    Node.Tag = setting.type;

                    settings.Add(setting, Node);
                }

                foreach(var pair in settings.OrderBy(a => a.Key.fileName.Length))
                {
                    GameSettings setting = pair.Key;
                    TreeNode node = pair.Value;

                    if (setting.parent == null)
                        continue;

                    KeyValuePair<GameSettings, TreeNode> parent = settings.Where(a => a.Key.fileName == setting.parent).FirstOrDefault();
                    if (parent.Value != null) // should not happen, prevent crash
                        parent.Value.Nodes.Add(node);
                }

                foreach (var pair in settings.Where(a => a.Key.parent == null))
                {
                    GameSettings setting = pair.Key;
                    TreeNode node = pair.Value;

                    try
                    {
                        treeView1.Nodes.Add(node);
                    }
                    catch(Exception ex)
                    {

                    }
                }
            });
        }

        // Display the appropriate context menu.
        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            // Make sure this is the right button.
            if (e.Button != MouseButtons.Right) return;

            // Select this node.
            TreeNode node_here = treeView1.GetNodeAt(e.X, e.Y);
            treeView1.SelectedNode = node_here;

            // See if we got a node.
            if (node_here == null) return;

            // See what kind of object this is and
            // display the appropriate popup menu.
            if (node_here.Tag is SymbolicLinkType.AllDirectories || node_here.Tag is SymbolicLinkType.TopDirectoryOnly)
                contextMenuStrip2.Show(treeView1, new Point(e.X, e.Y));
            else if (node_here.Tag is SymbolicLinkType.File)
                contextMenuStrip3.Show(treeView1, new Point(e.X, e.Y));
        }

        private void b_DeleteProfile_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
