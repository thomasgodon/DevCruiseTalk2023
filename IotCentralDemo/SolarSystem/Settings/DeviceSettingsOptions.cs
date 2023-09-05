using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Settings
{
    public class DeviceSettingsOptions
    {
        public const string Name = "DeviceSettings";
        public DeviceOptions SolarPanel { get; set; } = new();
        public DeviceOptions HomeBattery { get; set; } = new();
    }
}
