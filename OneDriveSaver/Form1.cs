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

                Dictionary<GameSettings, TreeNode> settings = new Dictionary<GameSettings, TreeNode>();

                foreach(var pair in game.Settings)
                {
                    GameSettings setting = pair.Value;
                    string key = pair.Key;
                    TreeNode Node = new TreeNode(setting.path, (int)setting.type, (int)setting.type);
                    Node.Tag = setting.type;

                    settings.Add(setting, Node);
                }

                foreach(var pair in settings)
                {
                    GameSettings setting = pair.Key;
                    TreeNode node = pair.Value;

                    if (setting.parent == null)
                        continue;

                    KeyValuePair<GameSettings, TreeNode> parent = settings.Where(a => a.Key.fileName == setting.parent).FirstOrDefault();
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
            Close();
        }
    }
}
