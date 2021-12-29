using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.TaskScheduler;
using Task = Microsoft.Win32.TaskScheduler.Task;

namespace DropboxMe
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
    }
}
