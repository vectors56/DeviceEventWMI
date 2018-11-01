using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using DeviceEventConsole;
using System.Threading;

namespace DeviceEventTray
{
    class DeviceEventTrayApp : Form
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                MessageBox.Show("Command line: DeviceEventTray <monitorfile>.json", "DeviceEvent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Application.Run(new DeviceEventTrayApp(args[0]));
        }

        private string monitorFile;
        private NotifyIcon icon;
        private MonitorTask monitorTask;

        public DeviceEventTrayApp(string monitorFile)
        {
            this.monitorFile = monitorFile;

            icon = new NotifyIcon();
            icon.Text = "Device Event";
            icon.Icon = new Icon(SystemIcons.Application, 40, 40);

            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add("Exit", OnExit);

            icon.ContextMenu = menu;
            icon.Visible = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            runMonitor();

            base.OnLoad(e);
        }

        private void runMonitor()
        {
            DeviceEventTrayApp app = this;
            monitorTask = new MonitorTask(monitorFile, false);
            try
            {
                monitorTask.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error launching device monitoring. Check with the console app that the JSON file is valid.\nException message: " + e.Message, "DeviceEvent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            Close();
            //Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (monitorTask != null)
                {
                    monitorTask.Stop();
                }
                icon.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
