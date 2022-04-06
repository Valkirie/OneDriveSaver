using SymbolicLinkSupport;
using System;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using static OneDriveSaver.SymLinkHelper;

namespace OneDriveSaver
{
    public enum GameSettingsCode
    {
        Pending = 0,
        ValidSymlink = 1,
        InvalidSymlink = 2,
    }

    [Serializable]
    public class GameSettings
    {
        [JsonIgnore()] public Game game { get; set; }

        [JsonIgnore()] public FileSystemWatcher pathWatcher { get; set; }
        [JsonIgnore()] public FileSystemWatcher symlinkWatcher { get; set; }

        [JsonIgnore()] public string fileName;
        [JsonIgnore()] public GameSettingsCode status = GameSettingsCode.Pending;

        public string path { get; set; }
        [JsonIgnore()] public string loc_path;

        public string symlink { get; set; }
        [JsonIgnore()] public string loc_symlink;

        public SymbolicLinkType type { get; set; }
        public string parent { get; set; }

        public GameSettings()
        { }

        public GameSettings(string path, string symlink, FileAttributes fileAtributes, GameSettings parent = null)
        {
            this.path = EnvironmentManager.ContractEnvironmentVariables(path);
            this.symlink = EnvironmentManager.ContractEnvironmentVariables(symlink);

            this.loc_path = path;
            this.loc_symlink = symlink;

            DirectoryInfo pathDirectoryInfo = new DirectoryInfo(path);
            DirectoryInfo symDirectoryInfo = new DirectoryInfo(symlink);

            // extract key from filename
            fileName = $"{pathDirectoryInfo}".ToLower();

            // dirty: what about symlink directories ?
            if (fileAtributes.HasFlag(FileAttributes.Directory))
                type = SymbolicLinkType.AllDirectories;
            else
                type = SymbolicLinkType.File;

            if (parent != null)
            {
                this.parent = parent.fileName;

                if (parent.game != null)
                {
                    this.game = parent.game;
                    this.game.EnqueueCreate(this);
                }
                else
                {
                    throw new Exception();
                }
            }
        }

        public bool SetSymlink()
        {
            try
            {
                return CreateSymbolicLink(loc_path, loc_symlink, type);
            }
            catch (Exception)
            { }
            return false;
        }

        public void Initialize(Game game)
        {
            this.game = game;

            path = path.ToLower();
            symlink = symlink.ToLower();

            loc_path = Environment.ExpandEnvironmentVariables(path).ToLower();
            loc_symlink = Environment.ExpandEnvironmentVariables(symlink).ToLower();

            FileInfo pathFileInfo = new FileInfo(loc_path);
            FileInfo symFileInfo = new FileInfo(loc_symlink);

            DirectoryInfo pathDirectoryInfo = new DirectoryInfo(loc_path);
            DirectoryInfo symDirectoryInfo = new DirectoryInfo(loc_symlink);

            // extract key from filename
            fileName = $"{pathDirectoryInfo}".ToLower();

            switch (type)
            {
                case SymbolicLinkType.File:

                    // existing file is a symbolic link already
                    if (pathFileInfo.Exists && pathFileInfo.IsSymbolicLink())
                    {
                        // target is present, all good!
                        if (pathFileInfo.IsSymbolicLinkValid())
                            status = GameSettingsCode.ValidSymlink;
                        else
                        {
                            // symbolic target is missing
                            status = GameSettingsCode.InvalidSymlink;

                            pathFileInfo.Delete();
                            this.game.EnqueueDelete(this); // do it here ?
                            return;
                        }
                    }
                    // existing file is not a symbolic link
                    else if (pathFileInfo.Exists)
                    {
                        // we already have a backup, keep the most recent one?
                        if (symFileInfo.Exists)
                        {
                            // delete the outdated backup
                            if (pathFileInfo.LastWriteTime > symFileInfo.LastWriteTime)
                            {
                                symFileInfo.Delete();
                                // move the backup so it's ready for symlink process
                                pathFileInfo.MoveTo(symFileInfo.FullName);
                            }
                            else
                            {
                                // delete the outdated existing file
                                pathFileInfo.Delete();
                            }
                        }
                        else
                        {
                            // move the backup so it's ready for symlink process
                            pathFileInfo.MoveTo(symFileInfo.FullName);
                        }
                    }
                    else if (symFileInfo.Exists)
                    {
                        // do nothing
                    }
                    else
                    {
                        // broken symlink
                        status = GameSettingsCode.InvalidSymlink;
                        this.game.EnqueueDelete(this); // do it here ?
                        return;
                    }

                    break;

                case SymbolicLinkType.TopDirectoryOnly:

                    // existing directory is a symbolic link already
                    if (pathDirectoryInfo.Exists && pathDirectoryInfo.IsSymbolicLink())
                    {
                        // target is present, all good!
                        if (symDirectoryInfo.Exists)
                            status = GameSettingsCode.ValidSymlink;
                        else
                        {
                            // symbolic target is missing
                            status = GameSettingsCode.InvalidSymlink;

                            pathDirectoryInfo.Delete(true);
                            this.game.EnqueueDelete(this); // do it here ?
                            return;
                        }
                    }
                    // existing directory is not a symbolic link
                    else if (pathDirectoryInfo.Exists)
                    {
                        // we already have a backup, keep the most recent one?
                        if (symDirectoryInfo.Exists)
                        {
                            // delete the outdated backup
                            if (pathFileInfo.LastWriteTime > symDirectoryInfo.LastWriteTime)
                            {
                                symDirectoryInfo.Delete(true);
                                // move the backup so it's ready for symlink process
                                pathDirectoryInfo.MoveTo(symDirectoryInfo.FullName);
                            }
                            else
                            {
                                // delete the outdated existing directory
                                pathDirectoryInfo.Delete(true);
                            }
                        }
                        else
                        {
                            // move the backup so it's ready for symlink process
                            pathDirectoryInfo.MoveTo(symDirectoryInfo.FullName);
                        }
                    }
                    else if (symDirectoryInfo.Exists)
                    {
                        // do nothing
                    }
                    else
                    {
                        // broken symlink
                        status = GameSettingsCode.InvalidSymlink;
                        this.game.EnqueueDelete(this); // do it here ?
                        return;
                    }

                    break;

                case SymbolicLinkType.AllDirectories:

                    if (!pathDirectoryInfo.Exists && !symDirectoryInfo.Exists)
                    {
                        // broken symlink
                        status = GameSettingsCode.InvalidSymlink;
                        this.game.EnqueueDelete(this); // do it here ?
                        return;
                    }

                    if (!pathDirectoryInfo.Exists)
                        pathDirectoryInfo.Create();

                    if (!symDirectoryInfo.Exists)
                        symDirectoryInfo.Create();

                    string[] fileEntries = Directory.GetDirectories(loc_path, "*.*", SearchOption.TopDirectoryOnly);
                    foreach (string fileName in fileEntries)
                        ProcessPath(fileName, this);

                    fileEntries = Directory.GetFiles(loc_path, "*.*", SearchOption.TopDirectoryOnly);
                    foreach (string fileName in fileEntries)
                        ProcessPath(fileName, this);

                    fileEntries = Directory.GetDirectories(loc_symlink, "*.*", SearchOption.TopDirectoryOnly);
                    foreach (string fileName in fileEntries)
                        ProcessSymlink(fileName, this);

                    fileEntries = Directory.GetFiles(loc_symlink, "*.*", SearchOption.TopDirectoryOnly);
                    foreach (string fileName in fileEntries)
                        ProcessSymlink(fileName, this);

                    pathWatcher = new FileSystemWatcher()
                    {
                        Path = loc_path,
                        EnableRaisingEvents = true,
                        IncludeSubdirectories = false
                    };
                    pathWatcher.Created += PathCreated;
                    pathWatcher.Deleted += PathDeleted;

                    // not fan of the below
                    status = GameSettingsCode.ValidSymlink;

                    break;
            }
        }

        private void PathDeleted(object sender, FileSystemEventArgs e)
        {
            if (status == GameSettingsCode.Pending)
                return;

            string relative = Path.GetRelativePath(loc_path, e.FullPath);
            string symName = Path.Combine(loc_symlink, relative);

            string symPath = Path.GetDirectoryName(symName);

            FileInfo symFileInfo = new FileInfo(symName);

            DirectoryInfo pathDirectoryInfo = new DirectoryInfo(e.FullPath);
            DirectoryInfo symDirectoryInfo = new DirectoryInfo(symPath);

            // extract key from filename
            string key = $"{pathDirectoryInfo}".ToLower();

            if (game.Settings.ContainsKey(key))
            {
                GameSettings setting = game.Settings[key];

                switch (setting.type)
                {
                    case SymbolicLinkType.File:

                        if (symFileInfo.Exists)
                            symFileInfo.Delete();

                        break;

                    case SymbolicLinkType.TopDirectoryOnly:

                        if (symDirectoryInfo.Exists)
                            symDirectoryInfo.Delete(true);

                        break;

                    case SymbolicLinkType.AllDirectories:

                        if (symDirectoryInfo.Exists)
                            symDirectoryInfo.Delete(true);

                        break;
                }

                this.game.EnqueueDelete(setting); // do it here ?
            }
        }

        private void PathCreated(object sender, FileSystemEventArgs e)
        {
            if (status == GameSettingsCode.Pending)
                return;

            ProcessPath(e.FullPath, this);
        }

        private void ProcessPath(string fileName, GameSettings parent)
        {
            string relative = Path.GetRelativePath(loc_path, fileName);
            string symName = Path.Combine(loc_symlink, relative);

            string symPath = Path.GetDirectoryName(symName);

            FileAttributes fileAttributes = File.GetAttributes(fileName);

            DirectoryInfo pathDirectoryInfo = new DirectoryInfo(fileName);
            DirectoryInfo symDirectoryInfo = fileAttributes == FileAttributes.Directory ? new DirectoryInfo(symName) : new DirectoryInfo(symPath);

            // create destination folder
            if (!symDirectoryInfo.Exists)
                symDirectoryInfo.Create();

            // extract key from filename
            string key = $"{pathDirectoryInfo}".ToLower();

            // skip this file if in ignore list
            if (game.Ignore.Contains(key, StringComparer.CurrentCultureIgnoreCase))
                return;

            // skip this file if in settings list
            if (game.Settings.ContainsKey(key))
                return;

            new GameSettings(fileName, symName, fileAttributes, parent);
        }

        private void ProcessSymlink(string symName, GameSettings parent)
        {
            string relative = Path.GetRelativePath(loc_symlink, symName);
            string fileName = Path.Combine(loc_path, relative);

            string filePath = Path.GetDirectoryName(fileName);

            FileAttributes fileAttributes = File.GetAttributes(symName);

            DirectoryInfo pathDirectoryInfo = fileAttributes == FileAttributes.Directory ? new DirectoryInfo(fileName) : new DirectoryInfo(filePath);
            DirectoryInfo symDirectoryInfo = new DirectoryInfo(symName);

            // create destination folder
            if (!pathDirectoryInfo.Exists)
                pathDirectoryInfo.Create();

            // extract key from filename
            string key = $"{symDirectoryInfo}".ToLower();

            // skip this file if in ignore list
            if (game.Ignore.Contains(key, StringComparer.CurrentCultureIgnoreCase))
                return;

            // skip this file if in settings list
            if (game.Settings.ContainsKey(key))
                return;

            new GameSettings(fileName, symName, fileAttributes, parent);
        }
    }
}
