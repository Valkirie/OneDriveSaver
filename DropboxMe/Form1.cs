using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using static DropboxMe.SymLinkHelper;

namespace DropboxMe
{
    public partial class Form1 : Form
    {
        private string dropboxPath, dropboxMePath;
        private bool StartOnBoot, BackupOnStart;
        LibraryMgr manager;

        public Form1()
        {
            InitializeComponent();

            // check if Dropbox is installed
            string infoPath = @"Dropbox\info.json";
            string jsonPath = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), infoPath);
            if (!File.Exists(jsonPath))
                jsonPath = Path.Combine(Environment.GetEnvironmentVariable("AppData"), infoPath);

            if (!File.Exists(jsonPath))
            {
                MessageBox.Show("Dropbox could not be found!");
                Application.Exit();
            }

            // settings
            StartOnBoot = Properties.Settings.Default.StartOnBoot;
            BackupOnStart = Properties.Settings.Default.BackupOnStart;
            dropboxPath = File.ReadAllText(jsonPath).Split('\"')[5].Replace(@"\\", @"\");
            dropboxMePath = Path.Combine(dropboxPath, "DropboxMe");

            // update environment var
            Environment.SetEnvironmentVariable("dropboxme", dropboxMePath);

            // initialize Task Manager
            Utils.SetStartup(StartOnBoot, Application.ExecutablePath, "DropboxMe");

            if (!Directory.Exists(dropboxMePath))
                Directory.CreateDirectory(dropboxMePath);

            if (BackupOnStart)
            {
                DateTime localDate = DateTime.Now;
                string filename = $"DropboxMe-backup-{localDate.ToString("dd-MM-yyyy")}.zip";
                if (!File.Exists($"{dropboxPath}\\{filename}"))
                    ZipFile.CreateFromDirectory(dropboxMePath, $"{dropboxPath}\\{filename}");
            }

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
            // initialize library manager
            manager = new LibraryMgr(this, dropboxMePath);
        }

        #region UI

        public void UpdateList(string game)
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
            string name = Interaction.InputBox("Please type the name of your game", "Create a new game profile", null, 0, 0);

            if (name == null)
                return;

            if (manager.games.ContainsKey(name))
                return;

            string filename = Path.Combine(dropboxMePath, name, "Settings.json");

            Game game = new Game(name, filename);
            manager.games[name] = game;
            game.Serialize();
        }

        private void lB_Games_SelectedIndexChanged(object sender, EventArgs e)
        {
            Game game = manager.games[(string)lB_Games.SelectedItem];

            if (game == null)
                return;

            this.BeginInvoke((MethodInvoker)delegate ()
            {
                tB_ProfileName.Text = game.Name;
                tB_ProfilePath.Text = game.Path;

                treeView1.Nodes.Clear();

                var directories = game.Settings.Values.Where(a => a.type == SymbolicLinkType.Directory);
                int idx_directory = 0;

                foreach (GameSettings setting in directories)
                {
                    treeView1.Nodes.Add(setting.key, setting.path, 0, 0);
                    treeView1.Nodes[idx_directory].Tag = setting.type;

                    var files = game.Settings.Values.Where(a => a.type == SymbolicLinkType.File && a.parent == setting.key);
                    int idx_files = 0;

                    foreach (GameSettings sub_setting in files)
                    {
                        treeView1.Nodes[idx_directory].Nodes.Add(sub_setting.key, sub_setting.path, 1, 1);
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
            if (node_here.Tag is SymbolicLinkType.Directory)
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
