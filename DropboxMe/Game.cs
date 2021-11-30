using SymbolicLinkSupport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;
using static DropboxMe.SymLinkHelper;

namespace DropboxMe
{
    [Serializable]
    public class GameSettings
    {
        [JsonIgnore()] public Game game { get; set; }

        [JsonIgnore()] public FileSystemWatcher pathWatcher { get; set; }
        [JsonIgnore()] public FileSystemWatcher symlinkWatcher { get; set; }
        [JsonIgnore()] public Timer timer { get; set; }

        [JsonIgnore()] public string loc_path { get; set; }
        [JsonIgnore()] public string loc_symlink { get; set; }
        [JsonIgnore()] public bool SymbolicLink { get; set; } // dirty

        public string path { get; set; }
        public string symlink { get; set; }
        public SymbolicLinkType type { get; set; }

        public GameSettings()
        {
        }

        public GameSettings(string _path, string _symlink, SymbolicLinkType _type)
        {
            path = LocalEnvironment.ContractEnvironmentVariables(_path);
            symlink = LocalEnvironment.ContractEnvironmentVariables(_symlink);
            loc_path = Environment.ExpandEnvironmentVariables(_path);
            loc_symlink = Environment.ExpandEnvironmentVariables(_symlink);
        }

        public void Initialize(Game _game)
        {
            // set game instance
            game = _game;

            // set pathes
            loc_path = Environment.ExpandEnvironmentVariables(path);
            loc_symlink = Environment.ExpandEnvironmentVariables(symlink);

            // set timer
            timer = new Timer(3000);
            timer.Enabled = false;
            timer.AutoReset = false;
            timer.Elapsed += Timer_Elapsed;

            if (type == SymbolicLinkType.Directory)
            {
                if (!Directory.Exists(loc_path))
                    Directory.CreateDirectory(loc_path);

                pathWatcher = new FileSystemWatcher()
                {
                    Path = loc_path,
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true
                };
                pathWatcher.Created += PathCreated;
                pathWatcher.Deleted += PathDeleted;

                string[] fileEntries = Directory.GetFiles(loc_path, "*.*", SearchOption.AllDirectories);
                foreach (string fileName in fileEntries)
                    ProcessPath(fileName);

                symlinkWatcher = new FileSystemWatcher()
                {
                    Path = loc_symlink,
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true
                };
                symlinkWatcher.Created += SymlinkCreated;
                symlinkWatcher.Deleted += SymlinkDeleted;

                fileEntries = Directory.GetFiles(loc_symlink, "*.*", SearchOption.AllDirectories);
                foreach (string fileName in fileEntries)
                    ProcessSymlink(fileName);
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            game.Serialize();
            timer.Stop();
        }

        private void PathDeleted(object sender, FileSystemEventArgs e)
        {
            if (!game.isInitialized)
                return;

            RemovePath(e.FullPath);
        }

        private void PathCreated(object sender, FileSystemEventArgs e)
        {
            if (!game.isInitialized)
                return;

            ProcessPath(e.FullPath);
        }

        private void SymlinkDeleted(object sender, FileSystemEventArgs e)
        {
            if (!game.isInitialized)
                return;

            RemoveSymlink(e.FullPath);
        }

        private void SymlinkCreated(object sender, FileSystemEventArgs e)
        {
            if (!game.isInitialized)
                return;

            ProcessSymlink(e.FullPath);
        }

        private void RemoveSymlink(string FullPath)
        {
            string relative = Path.GetRelativePath(loc_symlink, FullPath);
            string target = Path.Combine(loc_path, relative);

            // FileAttributes attr = File.GetAttributes(FullPath);
            FileSystemInfo info = /* attr == FileAttributes.Directory ? new DirectoryInfo(FullPath) :*/ new FileInfo(FullPath);

            string key = info.Name.ToLower();
            if (game.Settings.ContainsKey(key))
            {
                GameSettings setting = game.Settings[key];
                if (setting.SymbolicLink)
                {
                    setting.SymbolicLink = false;
                    return;
                }

                if (File.Exists(target) || Directory.Exists(target))
                    File.Delete(target);

                game.Settings.Remove(key);

                timer.Stop();
                timer.Start();
            }
        }

        private void ProcessSymlink(string FullPath)
        {
            string relative = Path.GetRelativePath(loc_symlink, FullPath);
            string target = Path.Combine(loc_path, relative);
            string target_folder = Path.GetDirectoryName(target);

            if (!Directory.Exists(target_folder))
                Directory.CreateDirectory(target_folder);

            // FileAttributes attr = File.GetAttributes(FullPath);
            FileSystemInfo info = /* attr == FileAttributes.Directory ? new DirectoryInfo(FullPath) :*/ new FileInfo(FullPath);
            try
            {
                // sometimes file creation process is an obstacle to the below
                if (((FileInfo)info).IsSymbolicLink())
                    return;
            }
            catch (Exception)
            { }

            if (game.Ignore.Contains(info.Name))
                return;

            GameSettings setting = new GameSettings(target, FullPath, info.Attributes == FileAttributes.Directory ? SymbolicLinkType.Directory : SymbolicLinkType.File);
            setting.Initialize(game);
            setting.SetJunction();

            string key = info.Name.ToLower();
            if (!game.Settings.ContainsKey(key))
            {
                game.Settings.Add(key, setting);

                timer.Stop();
                timer.Start();
            }
        }

        private void RemovePath(string FullPath)
        {
            string relative = Path.GetRelativePath(loc_path, FullPath);
            string target = Path.Combine(loc_symlink, relative);

            // FileAttributes attr = File.GetAttributes(FullPath);
            FileSystemInfo info = /* attr == FileAttributes.Directory ? new DirectoryInfo(FullPath) :*/ new FileInfo(FullPath);

            string key = info.Name.ToLower();
            if (game.Settings.ContainsKey(key))
            {
                GameSettings setting = game.Settings[key];
                if (setting.SymbolicLink)
                {
                    setting.SymbolicLink = false;
                    return;
                }

                if (File.Exists(target) || Directory.Exists(target))
                    File.Delete(target);

                game.Settings.Remove(key);

                timer.Stop();
                timer.Start();
            }
        }

        private void ProcessPath(string FullPath)
        {
            string relative = Path.GetRelativePath(loc_path, FullPath);
            string target = Path.Combine(loc_symlink, relative);
            string target_folder = Path.GetDirectoryName(target);

            if (!Directory.Exists(target_folder))
                Directory.CreateDirectory(target_folder);

            FileAttributes attr = File.GetAttributes(FullPath);
            FileSystemInfo info = attr == FileAttributes.Directory ? new DirectoryInfo(FullPath) : new FileInfo(FullPath);
            try
            {
                // sometimes file creation process is an obstacle to the below
                if (((FileInfo)info).IsSymbolicLink())
                    return;
            }
            catch (Exception)
            { }

            if (game.Ignore.Contains(info.Name))
                return;

            GameSettings setting = new GameSettings(FullPath, target, info.Attributes == FileAttributes.Directory ? SymbolicLinkType.Directory : SymbolicLinkType.File);
            setting.SetJunction();

            string key = info.Name.ToLower();
            if (!game.Settings.ContainsKey(key))
            {
                game.Settings.Add(key, setting);

                timer.Stop();
                timer.Start();
            }
        }

        public bool HasJunction()
        {
            if (type == SymbolicLinkType.File)
                if (!File.Exists(loc_path))
                    return false;

            if (type == SymbolicLinkType.Directory)
                if (!Directory.Exists(loc_path))
                    return false;

            DirectoryInfo dir = new DirectoryInfo(loc_path);
            return dir.IsSymbolicLink();
        }

        public void SetJunction()
        {
            // do we have an existing junction already
            bool junction = HasJunction();
            if (junction)
                return; // looks like its all set already

            bool exist_orig = File.Exists(loc_path);
            bool exist_dest = File.Exists(loc_symlink);
            FileInfo info_orig = new FileInfo(loc_path);
            FileInfo info_dest = new FileInfo(loc_symlink);
            string key = info_orig.Name.ToLower();

            // both file exists
            if (exist_dest && exist_orig)
            {
                // game settings are more recent
                if (info_orig.LastWriteTime > info_dest.LastWriteTime)
                    File.Move(loc_path, loc_symlink, true);
                else
                    File.Delete(loc_path);
            }
            // missing dest
            else if (!exist_dest && exist_orig)
                File.Move(loc_path, loc_symlink, true);
            else if (game.Settings.ContainsKey(key))
                game.Settings.Remove(key);

            SymbolicLink = true;
            CreateSymbolicLink(loc_path, loc_symlink, type);
        }
    }

    [Serializable]
    public class Game
    {
        [JsonIgnore()] public bool isInitialized;
        [JsonIgnore()] public string Path { get; set; }

        public string Name { get; set; }

        public Dictionary<string, GameSettings> Settings { get; set; } = new();
        public List<string> Ignore { get; set; } = new();

        public Game(string Name, string Path)
        {
            this.Name = Name;
            this.Path = Path;
        }

        public void Serialize()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(this, options);

            // write to disk
            string target_folder = System.IO.Path.GetDirectoryName(this.Path);

            if (!Directory.Exists(target_folder))
                Directory.CreateDirectory(target_folder);

            File.WriteAllText(this.Path, jsonString);
        }

        public void Initialize()
        {
            List<GameSettings> _settings = Settings.Values.ToList();
            for (int i = 0; i < _settings.Count; i++)
            {
                GameSettings setting = _settings[i];
                setting.Initialize(this);
            }

            isInitialized = true;
        }

        public void SetJunctions()
        {
            foreach (GameSettings setting in Settings.Values.Where(a => a.type == SymbolicLinkType.File))
                setting.SetJunction();

            foreach (GameSettings setting in Settings.Values.Where(a => a.type == SymbolicLinkType.File))
                setting.SymbolicLink = false;

            Serialize();
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
