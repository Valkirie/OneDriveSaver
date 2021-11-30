﻿
namespace DropboxMe
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gB_Profiles = new System.Windows.Forms.GroupBox();
            this.b_CreateProfile = new System.Windows.Forms.Button();
            this.lB_Profiles = new System.Windows.Forms.ListBox();
            this.gB_ProfileDetails = new System.Windows.Forms.GroupBox();
            this.b_DeleteProfile = new System.Windows.Forms.Button();
            this.tB_ProfilePath = new System.Windows.Forms.TextBox();
            this.lb_ProfilePath = new System.Windows.Forms.Label();
            this.tB_ProfileName = new System.Windows.Forms.TextBox();
            this.lb_ProfileName = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.gB_DeviceDetails = new System.Windows.Forms.GroupBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.contextMenuStrip1.SuspendLayout();
            this.gB_Profiles.SuspendLayout();
            this.gB_ProfileDetails.SuspendLayout();
            this.gB_DeviceDetails.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "DropboxMe";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(104, 26);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // gB_Profiles
            // 
            this.gB_Profiles.Controls.Add(this.b_CreateProfile);
            this.gB_Profiles.Controls.Add(this.lB_Profiles);
            this.gB_Profiles.Location = new System.Drawing.Point(12, 12);
            this.gB_Profiles.Name = "gB_Profiles";
            this.gB_Profiles.Size = new System.Drawing.Size(240, 552);
            this.gB_Profiles.TabIndex = 1;
            this.gB_Profiles.TabStop = false;
            this.gB_Profiles.Text = "Profiles";
            // 
            // b_CreateProfile
            // 
            this.b_CreateProfile.Location = new System.Drawing.Point(6, 522);
            this.b_CreateProfile.Name = "b_CreateProfile";
            this.b_CreateProfile.Size = new System.Drawing.Size(228, 23);
            this.b_CreateProfile.TabIndex = 1;
            this.b_CreateProfile.Text = "Create new profile";
            this.b_CreateProfile.UseVisualStyleBackColor = true;
            this.b_CreateProfile.Click += new System.EventHandler(this.b_CreateProfile_Click);
            // 
            // lB_Profiles
            // 
            this.lB_Profiles.FormattingEnabled = true;
            this.lB_Profiles.ItemHeight = 15;
            this.lB_Profiles.Location = new System.Drawing.Point(6, 32);
            this.lB_Profiles.Name = "lB_Profiles";
            this.lB_Profiles.Size = new System.Drawing.Size(228, 484);
            this.lB_Profiles.TabIndex = 0;
            this.lB_Profiles.SelectedIndexChanged += new System.EventHandler(this.lB_Profiles_SelectedIndexChanged);
            // 
            // gB_ProfileDetails
            // 
            this.gB_ProfileDetails.Controls.Add(this.button1);
            this.gB_ProfileDetails.Controls.Add(this.b_DeleteProfile);
            this.gB_ProfileDetails.Controls.Add(this.tB_ProfilePath);
            this.gB_ProfileDetails.Controls.Add(this.lb_ProfilePath);
            this.gB_ProfileDetails.Controls.Add(this.tB_ProfileName);
            this.gB_ProfileDetails.Controls.Add(this.lb_ProfileName);
            this.gB_ProfileDetails.Location = new System.Drawing.Point(258, 12);
            this.gB_ProfileDetails.Name = "gB_ProfileDetails";
            this.gB_ProfileDetails.Size = new System.Drawing.Size(987, 121);
            this.gB_ProfileDetails.TabIndex = 2;
            this.gB_ProfileDetails.TabStop = false;
            this.gB_ProfileDetails.Text = "Profile Details";
            // 
            // b_DeleteProfile
            // 
            this.b_DeleteProfile.Location = new System.Drawing.Point(156, 87);
            this.b_DeleteProfile.Name = "b_DeleteProfile";
            this.b_DeleteProfile.Size = new System.Drawing.Size(104, 23);
            this.b_DeleteProfile.TabIndex = 6;
            this.b_DeleteProfile.Text = "Delete profile";
            this.b_DeleteProfile.UseVisualStyleBackColor = true;
            // 
            // tB_ProfilePath
            // 
            this.tB_ProfilePath.Location = new System.Drawing.Point(156, 58);
            this.tB_ProfilePath.Name = "tB_ProfilePath";
            this.tB_ProfilePath.ReadOnly = true;
            this.tB_ProfilePath.Size = new System.Drawing.Size(344, 23);
            this.tB_ProfilePath.TabIndex = 5;
            // 
            // lb_ProfilePath
            // 
            this.lb_ProfilePath.AutoSize = true;
            this.lb_ProfilePath.Location = new System.Drawing.Point(6, 61);
            this.lb_ProfilePath.Name = "lb_ProfilePath";
            this.lb_ProfilePath.Size = new System.Drawing.Size(34, 15);
            this.lb_ProfilePath.TabIndex = 4;
            this.lb_ProfilePath.Text = "Path:";
            // 
            // tB_ProfileName
            // 
            this.tB_ProfileName.Location = new System.Drawing.Point(156, 29);
            this.tB_ProfileName.Name = "tB_ProfileName";
            this.tB_ProfileName.ReadOnly = true;
            this.tB_ProfileName.Size = new System.Drawing.Size(222, 23);
            this.tB_ProfileName.TabIndex = 3;
            // 
            // lb_ProfileName
            // 
            this.lb_ProfileName.AutoSize = true;
            this.lb_ProfileName.Location = new System.Drawing.Point(6, 32);
            this.lb_ProfileName.Name = "lb_ProfileName";
            this.lb_ProfileName.Size = new System.Drawing.Size(42, 15);
            this.lb_ProfileName.TabIndex = 2;
            this.lb_ProfileName.Text = "Name:";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // gB_DeviceDetails
            // 
            this.gB_DeviceDetails.Controls.Add(this.treeView1);
            this.gB_DeviceDetails.Location = new System.Drawing.Point(258, 139);
            this.gB_DeviceDetails.Name = "gB_DeviceDetails";
            this.gB_DeviceDetails.Size = new System.Drawing.Size(987, 425);
            this.gB_DeviceDetails.TabIndex = 4;
            this.gB_DeviceDetails.TabStop = false;
            this.gB_DeviceDetails.Text = "Profile Settings";
            // 
            // treeView1
            // 
            this.treeView1.Cursor = System.Windows.Forms.Cursors.Default;
            this.treeView1.ImageIndex = 0;
            this.treeView1.ImageList = this.imageList1;
            this.treeView1.Location = new System.Drawing.Point(6, 22);
            this.treeView1.Name = "treeView1";
            this.treeView1.SelectedImageIndex = 0;
            this.treeView1.Size = new System.Drawing.Size(975, 396);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "FolderBottomPanel_16x.png");
            this.imageList1.Images.SetKeyName(1, "FileSystemDriverFile_16x.png");
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(266, 87);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(104, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "Delete profile";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1257, 572);
            this.Controls.Add(this.gB_DeviceDetails);
            this.Controls.Add(this.gB_ProfileDetails);
            this.Controls.Add(this.gB_Profiles);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DropboxMe";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.contextMenuStrip1.ResumeLayout(false);
            this.gB_Profiles.ResumeLayout(false);
            this.gB_ProfileDetails.ResumeLayout(false);
            this.gB_ProfileDetails.PerformLayout();
            this.gB_DeviceDetails.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.GroupBox gB_Profiles;
        private System.Windows.Forms.Button b_CreateProfile;
        private System.Windows.Forms.ListBox lB_Profiles;
        private System.Windows.Forms.GroupBox gB_ProfileDetails;
        private System.Windows.Forms.Button b_DeleteProfile;
        private System.Windows.Forms.TextBox tB_ProfilePath;
        private System.Windows.Forms.Label lb_ProfilePath;
        private System.Windows.Forms.TextBox tB_ProfileName;
        private System.Windows.Forms.Label lb_ProfileName;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.GroupBox gB_DeviceDetails;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Button button1;
    }
}

