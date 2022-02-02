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
        private bool StartOnBoot, BackupOnStart;
        private LibraryMgr libManager;

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
            onedrivesavePath = Path.Combine(onedrivePath, "Saved Games");

            // update environment var
            Environment.SetEnvironmentVariable("OneDriveSavedGames", onedrivesavePath);

            // initialize Task Manager
            Utils.SetStartup(StartOnBoot, Application.ExecutablePath, "OneDriveSavedGames");

            if (!Directory.Exists(onedrivesavePath))
                Directory.CreateDirectory(onedrivesavePath);

            if (BackupOnStart)
            {
                DateTime localDate = DateTime.Now;
                string filename = $"SavedGames-{localDate.ToString("dd-MM-yyyy")}.zip";
                if (!File.Exists($"{onedrivePath}\\{filename}"))
                    ZipFile.CreateFromDirectory(onedrivesavePath, $"{onedrivePath}\\{filename}");
            }

            // initialize library manager
            libManager = new LibraryMgr(onedrivesavePath);
            libManager.Updated += UpdateList;

            /*
             * Dictionary<string, GameSettings> testsettings = new();                
             * 
             * FileInfo info = new FileInfo("%localappdata%\\kena\\saved\\savegames\\");
             * DirectoryInfo info2 = new DirectoryInfo("%localappdata%\\kena\\saved\\savegames\\");                
             * 
             * GameSettings testsetting = new GameSettings("%localappdata%\\kena\\saved\\savegames\\", "%userprofile%\\dropbox\\dropboxme\\kena bridge of spirits\\saves\\", SymLinkHelper.SymbolicLinkType.Directory);
             * testsettings.Add(info2.Name, testsetting);
             * 
             * testsetting = new GameSettings("%localappdata%\\kena\\saved\\savegames\\autosave.sav", "%userprofile%\\dropbox\\dropboxme\\kena bridge of spirits\\saves\\autosave.sav", SymLinkHelper.SymbolicLinkType.File);
             * testsettings.Add("autosave.sav", testsetting);                
             * 
             * Game test = new Game()                
             * {                    
             * Name = "test",                    
             * Settings = testsettings                
             * };                
             * test.Serialize();            
             */
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            libManager.Process();
        }

        #region UI

        public void UpdateList(Game game)
        {
            this.BeginInvoke((MethodInvoker)delegate ()
            {
                lB_Games.Items.Add(game);
            });
        }

        #endregion

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.Visible = true;
                ShowInTaskbar = false;
            }
            else if (WindowState == FormWindowState.Normal)
            {
                notifyIcon1.Visible = false;
                ShowInTaskbar = true;
            }
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

                IEnumerable<KeyValuePair<string, GameSettings>> directories = game.Settings.Where(a => a.Value.type == SymbolicLinkType.AllDirectories);
                int idx_directory = 0;

                foreach (KeyValuePair<string, GameSettings> pair in directories)
                {
                    GameSettings setting = pair.Value;

                    treeView1.Nodes.Add(setting.fileName, setting.path, (int)setting.type, (int)setting.type);
                    treeView1.Nodes[idx_directory].Tag = setting.type;

                    IEnumerable<KeyValuePair<string, GameSettings>> files = game.Settings.Where(a => a.Value.type == SymbolicLinkType.File && a.Value.parent == setting.fileName);
                    int idx_files = 0;

                    foreach (KeyValuePair<string, GameSettings> sub_pair in files)
                    {
                        GameSettings sub_setting = sub_pair.Value;

                        treeView1.Nodes[idx_directory].Nodes.Add(sub_setting.fileName, sub_setting.path, (int)sub_setting.type, (int)sub_setting.type);
                        treeView1.Nodes[idx_directory].Nodes[idx_files].Tag = sub_setting.type;
                        idx_files++;
                    }

                    idx_directory++;
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
            Close();
        }
    }
}
