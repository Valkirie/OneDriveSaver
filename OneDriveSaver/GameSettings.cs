using OneDriveSaver.Properties;
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

            if (fileAtributes.HasFlag(FileAttributes.Directory))
            {
                if (parent is not null)
                    type = SymbolicLinkType.AllDirectories;
                else
                    type = SymbolicLinkType.TopDirectoryOnly;
            }
            else
                type = SymbolicLinkType.File;

            if (parent != null)
            {
                this.parent = parent.fileName;

                if (parent.game != null)
                    this.game = parent.game;
                else
                {
                    LogManager.LogError("ERROR!!!???");
                    return;
                }
            }

            this.game.EnqueueCreate(this);
        }

        public bool SetSymlink()
        {
            DirectoryInfo directoryInfo = Directory.GetParent(this.loc_path.TrimEnd('\\'));
            if (!Directory.Exists(directoryInfo.FullName))
                Directory.CreateDirectory(directoryInfo.FullName);

            FileSystemInfo info;
            switch (type)
            {
                default:
                case SymbolicLinkType.File:
                    info = File.CreateSymbolicLink(this.loc_path, this.loc_symlink);
                    break;
                case SymbolicLinkType.TopDirectoryOnly:
                    info = Directory.CreateSymbolicLink(this.loc_path, this.loc_symlink);
                    break;
            }

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
                    {
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
                                LogManager.LogInformation("Deleting broken symbolic link {0}", pathFileInfo.FullName);

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
                                    LogManager.LogInformation("Deleting outdated backup file {0}", symFileInfo.FullName);

                                    // move the backup so it's ready for symlink process
                                    pathFileInfo.MoveTo(symFileInfo.FullName);
                                    LogManager.LogInformation("Moving file {0} to {1}", pathFileInfo.FullName, symFileInfo.FullName);
                                }
                                else
                                {
                                    // delete the outdated existing file
                                    pathFileInfo.Delete();
                                    LogManager.LogInformation("Deleting file {0} as we've got more recent backup", pathFileInfo.FullName);
                                }
                            }
                            else
                            {
                                // move the backup so it's ready for symlink process
                                pathFileInfo.MoveTo(symFileInfo.FullName);
                                LogManager.LogInformation("Moving file {0} to {1}", pathFileInfo.FullName, symFileInfo.FullName);
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

                            LogManager.LogInformation("Deleting broken symlink {0}", this.fileName);
                            return;
                        }
                    }
                    break;

                case SymbolicLinkType.TopDirectoryOnly:
                    {
                        // existing directory is a symbolic link already
                        if (pathDirectoryInfo.IsSymbolicLink())
                        {
                            // target is present, all good!
                            if (symDirectoryInfo.Exists)
                            {
                                status = GameSettingsCode.ValidSymlink;
                                return;
                            }
                            else
                            {
                                // symbolic target is missing
                                status = GameSettingsCode.InvalidSymlink;

                                pathDirectoryInfo.Delete(true);
                                LogManager.LogInformation("Deleting broken symbolic link {0}", pathDirectoryInfo.FullName);

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
                                // exitsting directory is newer than backup
                                if (pathFileInfo.LastWriteTime > symDirectoryInfo.LastWriteTime)
                                {
                                    // Get the folder name and the parent directory
                                    string folderName = Path.GetFileName(pathDirectoryInfo.FullName);
                                    string parentDirectory = Path.GetDirectoryName(pathDirectoryInfo.FullName);

                                    // Concatenate "-old" to the folder name
                                    string newFolderName = folderName + "-old";

                                    // Return the new folder path
                                    // create directory backup
                                    pathDirectoryInfo.MoveTo(Path.Combine(parentDirectory, newFolderName));

                                    /*
                                    // delete the outdated backup
                                    symDirectoryInfo.Delete(true);
                                    LogManager.LogInformation("Deleting outdated backup path {0}", symDirectoryInfo.FullName);

                                    // move the directory so it's ready for symlink process
                                    pathDirectoryInfo.MoveTo(symDirectoryInfo.FullName);
                                    LogManager.LogInformation("Moving path {0} to {1}", pathDirectoryInfo.FullName, symDirectoryInfo.FullName);
                                    */
                                }
                                else
                                {
                                    // delete the outdated existing directory
                                    pathDirectoryInfo.Delete(true);
                                    LogManager.LogInformation("Deleting path {0} as we've got more recent backup", pathDirectoryInfo.FullName);
                                }
                            }
                            else
                            {
                                // move the backup so it's ready for symlink process
                                pathDirectoryInfo.MoveTo(symDirectoryInfo.FullName);
                                LogManager.LogInformation("Moving path {0} to {1}", pathDirectoryInfo.FullName, symDirectoryInfo.FullName);
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

                            LogManager.LogInformation("Deleting broken symlink {0}", this.fileName);
                            return;
                        }

                        // we're good to go ?
                        this.SetSymlink();
                    }
                    break;

                case SymbolicLinkType.AllDirectories:
                    {
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
                    }
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
