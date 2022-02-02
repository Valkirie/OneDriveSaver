using Microsoft.Win32.TaskScheduler;
using System;
using System.IO;
using Task = Microsoft.Win32.TaskScheduler.Task;

namespace OneDriveSaver
{
    class Utils
    {
        public static Task CurrentTask;
        public static void SetStartup(bool OnStartup, string ExecutablePath, string CurrentTaskName)
        {
            TaskService TaskServ = new TaskService();
            CurrentTask = TaskServ.FindTask(CurrentTaskName);

            TaskDefinition td = TaskService.Instance.NewTask();
            td.Principal.RunLevel = TaskRunLevel.Highest;
            td.Principal.LogonType = TaskLogonType.InteractiveToken;
            td.Settings.DisallowStartIfOnBatteries = false;
            td.Settings.StopIfGoingOnBatteries = false;
            td.Settings.ExecutionTimeLimit = TimeSpan.Zero;
            td.Settings.Enabled = false;
            td.Triggers.Add(new LogonTrigger());
            td.Actions.Add(new ExecAction(ExecutablePath));
            CurrentTask = TaskService.Instance.RootFolder.RegisterTaskDefinition(CurrentTaskName, td);

            CurrentTask.Enabled = OnStartup;
        }

        public static bool IsFileLocked(string file)
        {
            try
            {
                if (File.Exists(file))
                    using (var stream = File.OpenRead(file))
                        return false;
                else
                    return false;
            }
            catch (IOException)
            {
                return true;
            }
        }
    }
}
