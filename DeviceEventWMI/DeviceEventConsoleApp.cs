using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;

namespace DeviceEventConsole
{
    class DeviceEventConsoleApp
    {
        static void Main(string[] args)
        {
            Options options = new Options(args);
            if (options.list)
            {
                string _class = options._class;
                if (_class == null) _class = "Win32_PNPEntity";
                ClassInfo info = new ClassInfo(_class, options.filter);
                info.PrintDevices();
            }

            if (options.watch != null)
            {
                MonitorTask task = new MonitorTask(options.watch, options.verbose);
                task.Run();
            }

            if (!options.list && options.watch == null)
            {
                Console.WriteLine("Monitoring mode:");
                Console.WriteLine("  -w | --watch <file>        Start monitoring according to the specified JSON file.");
                Console.WriteLine("  -v | --verbose             Outputs events to the console.");
                Console.WriteLine();
                Console.WriteLine("Device Enumeration mode:");
                Console.WriteLine("  -l | --list                List available devices and Ids.");
                Console.WriteLine("  -c | --class <classId>     Device class to enumerate from (default: Win32_PNPEntity).");
                Console.WriteLine("  -f | --filter <string>     Filter devices whose name or description contains the string.");
            }
        }

    }
}
