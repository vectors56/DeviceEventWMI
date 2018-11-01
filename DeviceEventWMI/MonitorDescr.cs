using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace DeviceEventConsole
{
    class MonitorDescr
    {
        public string DeviceId { get; set; }

        public string ClassId { get; set; } = "Win32_PNPEntity";

        public List<TaskDescr> OnAttach { get; set; }

        public List<TaskDescr> OnDetach { get; set; }

        public bool ShowEvent { get; set; }

        public bool IsDeviceAttached()
        {
            var query = "Select * From " + ClassId + " Where DeviceID = \"" + DeviceId.Replace("\\", "\\\\") + "\"";
            using (var searcher = new ManagementObjectSearcher(query))
            {
                using (var collection = searcher.Get())
                {
                    return collection.Count > 0;
                }
            }
        }
    }

    class TaskDescr
    {
        public string Run { get; set; }

        public string Arguments { get; set; }

        public bool ShowWindow { get; set; } = true;

        public int Delay { get; set; } = 0;

        public string UnlessProcessRunning { get; set; }

    }
}
