using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace DeviceEventConsole
{
    class Monitor
    {
        private string query;
        private List<Task> onAttach = new List<Task>();
        private List<Task> onDetach = new List<Task>();
        private TaskFactory taskFactory;

        public bool Attached { get; private set; }

        public Monitor(MonitorDescr descr, bool verbose)
        {
            this.taskFactory = new TaskFactory(verbose);
            this.query = "Select * From " + descr.ClassId + " Where DeviceID = \"" + descr.DeviceId.Replace("\\", "\\\\") + "\"";
            DoUpdate();
            if (verbose || descr.ShowEvent)
            {
                string id = descr.DeviceId;
                onAttach.Add(new LogTask("Device " + id + " attached."));
                onDetach.Add(new LogTask("Device " + id + " detached."));
                Console.WriteLine(Attached ? "Device " + id + " initially attached." : "Device " + id + " initally detached.");
            }
            foreach (TaskDescr d in descr.OnAttach)
            {
                onAttach.Add(taskFactory.Create(d));
            }
            foreach (TaskDescr d in descr.OnDetach)
            {
                onDetach.Add(taskFactory.Create(d));
            }
        }

        public bool Update()
        {
            bool ret = DoUpdate();
            if (ret)
            {
                List<Task> todo = Attached ? onAttach : onDetach;
                todo.ForEach(t => t.Run());
            }
            return ret;
        }

        private bool DoUpdate()
        {
            bool old = Attached;
            this.Attached = IsDeviceAttached();
            return old != Attached;
        }

        private bool IsDeviceAttached()
        {
            using (var searcher = new ManagementObjectSearcher(query))
            {
                using (var collection = searcher.Get())
                {
                    return collection.Count > 0;
                }
            }
        }

    }

    interface Task
    {
        void Run();

    }

    class TaskFactory
    {
        private bool verbose;

        public TaskFactory(bool verbose)
        {
            this.verbose = verbose;
        }

        public Task Create(TaskDescr descr)
        {
            Task task = CreateBare(descr);
            if (descr.Delay > 0)
            {
                task = new DelayedTask(task, descr.Delay, verbose);
            }
            return task;
        }

        private Task CreateBare(TaskDescr descr)
        {
            return new ProgramTask(descr, verbose);
        }
    }

    class LogTask : Task
    {
        private string message;

        public LogTask(string message)
        {
            this.message = message;
        }

        public void Run()
        {
            Console.WriteLine(message);
        }
    }

    abstract class BaseTask : Task
    {
        protected readonly bool verbose;

        public BaseTask(bool verbose)
        {
            this.verbose = verbose;
        }

        public abstract void Run();

        protected void Log(string message)
        {
            if (verbose) Console.WriteLine(ToString() + ": " + message);
        }
    }

    class ProgramTask : BaseTask
    {
        private ProcessStartInfo startInfo;
        private string unlessProcessRunning;

        public ProgramTask(TaskDescr descr, bool verbose): base(verbose)
        {
            startInfo = new ProcessStartInfo();
            startInfo.FileName = descr.Run;
            startInfo.Arguments = descr.Arguments;
            startInfo.WindowStyle = descr.ShowWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = false;
            if (verbose)
            {
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
            }
            this.unlessProcessRunning = descr.UnlessProcessRunning;
        }

        public override void Run()
        {
            if (unlessProcessRunning != null)
            {
                Process[] processes = Process.GetProcessesByName(unlessProcessRunning);
                if (processes.Any())
                {
                    Log(String.Format("Found {0} process(es) already running, skipping program launch.", processes.Count()));
                    return;
                }
            }
            Log("Launching.");
            Process p = new Process();
            p.StartInfo = startInfo;
            bool ok = p.Start();
            Log(ok ? "Launched." : "Failed.");
        }

        public override string ToString()
        {
            return "[" + startInfo.FileName + "]";
        }
    }

    class DelayedTask : BaseTask
    {
        private Task task;
        private int delay;

        public DelayedTask(Task task, int delay, bool verbose): base(verbose)
        {
            this.task = task;
            this.delay = delay;
        }

        public override void Run()
        {
            Log("Scheduling execution.");
            System.Threading.Tasks.Task.Delay(delay).ContinueWith(t => task.Run());
        }

        public override string ToString()
        {
            return "[Delay " + delay + "]" + task;
        }
    }

}
