using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace DeviceEventWMI
{
    class ClassInfo
    {
        private string type;
        private string filter;

        public ClassInfo(string type, string filter)
        {
            this.type = type;
            this.filter = filter;
        }

        public void PrintDevices()
        {
            var devices = GetDevices();

            Console.WriteLine("Devices of class " + type + ":");
            foreach (var device in devices)
            {
                Console.WriteLine("Name: {2} ({3}), ID={0}",
                    device.DeviceID, device.PnpDeviceID, device.Name, device.Description);
            }
        }

        private List<DeviceInfo> GetDevices()
        {
            List<DeviceInfo> devices = new List<DeviceInfo>();

            string query = "Select * From " + type;
            if (filter != null)
            {
                query += " Where Description LIKE \"%" + filter + "%\" Or Name LIKE \"%" + filter + "%\"";
            }

            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(query))
            {
                collection = searcher.Get();
            }

            foreach (var device in collection)
            {
                devices.Add(new DeviceInfo(
                    (string)device.GetPropertyValue("DeviceID"),
                    (string)device.GetPropertyValue("PNPDeviceID"),
                    (string)device.GetPropertyValue("Name"),
                    (string)device.GetPropertyValue("Description")
                    ));
            }

            collection.Dispose();
            return devices;
        }
    }

    class DeviceInfo
    {
        public DeviceInfo(string deviceID, string pnpDeviceID, string name, string description)
        {
            this.DeviceID = deviceID;
            this.PnpDeviceID = pnpDeviceID;
            this.Name = name;
            this.Description = description;
        }
        public string DeviceID { get; private set; }
        public string PnpDeviceID { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
    }

}
