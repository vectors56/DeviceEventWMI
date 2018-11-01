using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace DeviceEventConsole
{
    public class MonitorTask
    {
        private readonly Monitor monitor;
        private readonly ManagementEventWatcher watcher;

        public MonitorTask(string monitorFile, bool verbose)
        {
            string spec;
            using (StreamReader reader = new StreamReader(monitorFile))
            {
                spec = reader.ReadToEnd();
            }
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Error;
            MonitorDescr descr = JsonConvert.DeserializeObject<MonitorDescr>(spec, settings);
            monitor = new Monitor(descr, verbose);

            watcher = new ManagementEventWatcher("Select * From Win32_DeviceChangeEvent Within 1 Where EventType = 2 Or EventType = 3");
        }

        public void Run()
        {
            while (true)
            {
                ManagementBaseObject e = watcher.WaitForNextEvent();
                monitor.Update();
            }
        }

        public void Start()
        {
            watcher.EventArrived += (s, e) => monitor.Update();
            watcher.Start();
        }

        public void Stop()
        {
            watcher.Stop();
        }

    }
}
