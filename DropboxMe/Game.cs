using SymbolicLinkSupport;
using System;
using System.Collections.Concurrent;
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

        [JsonIgnore()] public string loc_path { get; set; }
        [JsonIgnore()] public string loc_symlink { get; set; }
        [JsonIgnore()] public bool SymbolicLink { get; set; } // dirty
        [JsonIgnore()] public string key { get; set; }

        public string path { get; set; }
        public string symlink { get; set; }
        public SymbolicLinkType type { get; set; }
        public string parent { get; set; }

        public event HasChangedEventHandler HasChanged;
        public delegate void HasChangedEventHandler(Object sender);

        public GameSettings()
        {
        }

        public GameSettings(string _path, string _symlink, SymbolicLinkType _type, GameSettings parent = null)
        {
            path = LocalEnvironment.ContractEnvironmentVariables(_path);
            symlink = LocalEnvironment.ContractEnvironmentVariables(_symlink);
            loc_path = Environment.ExpandEnvironmentVariables(_path);
            loc_symlink = Environment.ExpandEnvironmentVariables(_symlink);

            // set key
            FileAttributes attr = FileAttributes.Directory;

            if (_type == SymbolicLinkType.File)
            {
                if (File.Exists(loc_path))
                    attr = File.GetAttributes(loc_path);
            }
            else if (_type == SymbolicLinkType.Directory)
            {
                if (Directory.Exists(loc_path))
                    attr = FileAttributes.Directory;
            }
            else
                return;

            FileSystemInfo info = attr == FileAttributes.Directory ? new DirectoryInfo(loc_path) : new FileInfo(loc_path);
            key = info.Name.ToLower();

            if (parent != null)
                this.parent = parent.key;
        }

        public void Initialize(Game _game)
        {
            // set game instance
            game = _game;

            // set pathes
            loc_path = Environment.ExpandEnvironmentVariables(path);
            loc_symlink = Environment.ExpandEnvironmentVariables(symlink);

            // set key
            FileAttributes attr = FileAttributes.Directory;

            if (type == SymbolicLinkType.File)
            {
                if (File.Exists(loc_path))
                    attr = File.GetAttributes(loc_path);
            }
            else if (type == SymbolicLinkType.Directory)
            {
                if (Directory.Exists(loc_path))
                    attr = FileAttributes.Directory;
            }
            else
                return;

            FileSystemInfo info = attr == FileAttributes.Directory ? new DirectoryInfo(loc_path) : new FileInfo(loc_path);
            key = info.Name.ToLower();

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
                    ProcessPath(fileName, this);

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
                    ProcessSymlink(fileName, this);
            }
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

            ProcessPath(e.FullPath, this);
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

            ProcessSymlink(e.FullPath, this);
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

                game.Settings.TryRemove(key, out setting);
                HasChanged?.Invoke(this);
            }
        }

        private void ProcessSymlink(string FullPath, GameSettings parent)
        {
            string relative = Path.GetRelativePath(loc_symlink, FullPath);
            string target = Path.Combine(loc_path, relative);
            string target_folder = Path.GetDirectoryName(target);

            if (!Directory.Exists(target_folder))
                Directory.CreateDirectory(target_folder);

            FileSystemInfo info = new FileInfo(FullPath);
            string key = info.Name.ToLower();

            try
            {
                // sometimes file creation process is an obstacle to the below
                if (((FileInfo)info).IsSymbolicLink())
                {
                    // force update key (temporary)
                    if (game.Settings.ContainsKey(key))
                    {
                        game.Settings[key].parent = parent.key;
                        return;
                    }
                }
            }
            catch (Exception)
            { }

            if (game.Ignore.Contains(info.Name))
                return;

            GameSettings setting = new GameSettings(target, FullPath, info.Attributes == FileAttributes.Directory ? SymbolicLinkType.Directory : SymbolicLinkType.File, parent);
            setting.Initialize(game);
            bool success = setting.SetJunction();

            if (!game.Settings.ContainsKey(key))
            {
                if (success)
                {
                    success = game.Settings.TryAdd(key, setting);
                    if (success) HasChanged?.Invoke(this);
                }
                else
                    game.Queue.Enqueue(setting);
            }
        }

        private void RemovePath(string FullPath)
        {
            string relative = Path.GetRelativePath(loc_path, FullPath);
            string target = Path.Combine(loc_symlink, relative);

            FileSystemInfo info = new FileInfo(FullPath);
            string key = info.Name.ToLower();

            if (game.Settings.ContainsKey(key))
            {
                if (File.Exists(target) || Directory.Exists(target))
                    File.Delete(target);

                GameSettings result;
                game.Settings.TryRemove(key, out result);
                HasChanged?.Invoke(this);
            }
        }

        private void ProcessPath(string FullPath, GameSettings parent)
        {
            string relative = Path.GetRelativePath(loc_path, FullPath);
            string target = Path.Combine(loc_symlink, relative);
            string target_folder = Path.GetDirectoryName(target);

            if (!Directory.Exists(target_folder))
                Directory.CreateDirectory(target_folder);

            FileAttributes attr = File.GetAttributes(FullPath);
            FileSystemInfo info = attr == FileAttributes.Directory ? new DirectoryInfo(FullPath) : new FileInfo(FullPath);
            string key = info.Name.ToLower();

            try
            {
                // sometimes file creation process is an obstacle to the below
                if (((FileInfo)info).IsSymbolicLink())
                {
                    // force update key (temporary)
                    if (game.Settings.ContainsKey(key))
                    {
                        game.Settings[key].parent = parent.key;
                        return;
                    }
                }
            }
            catch (Exception)
            { }

            if (game.Ignore.Contains(info.Name))
                return;

            GameSettings setting = new GameSettings(FullPath, target, info.Attributes == FileAttributes.Directory ? SymbolicLinkType.Directory : SymbolicLinkType.File, parent);
            bool success = setting.SetJunction();
            
            if (!game.Settings.ContainsKey(key))
            {
                if (success)
                {
                    success = game.Settings.TryAdd(key, setting);
                    if (success) HasChanged?.Invoke(this);
                }
                else
                    game.Queue.Enqueue(setting);
            }
        }

        public bool HasJunction()
        {
            if (type == SymbolicLinkType.File)
            {
                FileInfo file = new FileInfo(loc_path);
                if (!File.Exists(loc_path))
                    return false;
                else
                    return file.IsSymbolicLink();
            }
            else if (type == SymbolicLinkType.Directory)
            {
                DirectoryInfo dir = new DirectoryInfo(loc_path);
                if (!Directory.Exists(loc_path))
                    return false;
                else
                    return dir.IsSymbolicLink();
            }
            return false;
        }

        public bool SetJunction()
        {
            if (type == SymbolicLinkType.Directory)
                return true;

            // do we have an existing junction already
            bool junction = HasJunction();
            if (junction)
                return true; // looks like its all set already

            FileInfo info_orig = new FileInfo(loc_path);
            FileInfo info_dest = new FileInfo(loc_symlink);
            string key = info_orig.Name.ToLower();

            // if file is being used, send it to buffer for later execution
            if (Utils.IsFileLocked(loc_path))
                return false;

            GameSettings setting = this;

            // both file exists
            if (info_dest.Exists && info_orig.Exists)
            {
                // game settings are more recent
                if (info_orig.LastWriteTime > info_dest.LastWriteTime)
                    File.Move(loc_path, loc_symlink, true);
                else
                    File.Delete(loc_path);
            }
            // missing dest
            else if (!info_dest.Exists && info_orig.Exists)
                File.Move(loc_path, loc_symlink, true);
            else if (game.Settings.ContainsKey(key))
                game.Settings.TryRemove(key, out setting);

            SymbolicLink = true;
            return CreateSymbolicLink(loc_path, loc_symlink, type);
        }
    }

    [Serializable]
    public class Game
    {
        [JsonIgnore()] public bool isInitialized;
        [JsonIgnore()] public string Path { get; set; }

        public string Name { get; set; }

        [JsonIgnore()] public ConcurrentQueue<GameSettings> Queue = new();
        public ConcurrentDictionary<string, GameSettings> Settings { get; set; } = new();

        public List<string> Ignore { get; set; } = new();

        [JsonIgnore()] private Timer queue_timer;
        [JsonIgnore()] private Timer serialize_timer;

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
            // monitors settings queu
            queue_timer = new Timer(1000) { Enabled = true, AutoReset = true };
            queue_timer.Elapsed += TryQueue;

            serialize_timer = new Timer(500) { Enabled = false, AutoReset = false };
            serialize_timer.Elapsed += TimerSerialize_Elapsed;

            List<GameSettings> _settings = Settings.Values.ToList();
            for (int i = 0; i < _settings.Count; i++)
            {
                GameSettings setting = _settings[i];
                setting.Initialize(this);
                setting.HasChanged += Setting_HasChanged;
            }

            isInitialized = true;
        }

        private void TryQueue(object sender, ElapsedEventArgs e)
        {
            foreach (GameSettings setting in Queue)
            {
                bool success = setting.SetJunction();

                if (success)
                {
                    GameSettings result;
                    success = Queue.TryDequeue(out result);
                    if (success)
                    {
                        success = Settings.TryAdd(setting.key, setting);
                        if (success) Setting_HasChanged(setting);
                    }
                }
            }
        }

        private void TimerSerialize_Elapsed(object sender, ElapsedEventArgs e)
        {
            Serialize();
        }

        private void Setting_HasChanged(object sender)
        {
            serialize_timer.Stop();
            serialize_timer.Start();
        }

        public void SetJunctions()
        {
            List<GameSettings> _settings = Settings.Values.ToList();
            for (int i = 0; i < _settings.Count; i++)
            {
                GameSettings setting = _settings[i];
                bool success = setting.SetJunction();

                if (!success)
                {
                    Settings.TryRemove(setting.key, out setting);
                    Queue.Enqueue(setting);
                }
            }

            _settings = Settings.Values.ToList();
            for (int i = 0; i < _settings.Count; i++)
            {
                GameSettings setting = _settings[i];
                setting.SymbolicLink = false;
            }

            Serialize();
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
