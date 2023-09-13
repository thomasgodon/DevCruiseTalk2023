using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Telemetry
{
    internal class SolarPanelTelemetry : ITelemetry
    {
        public int Id { get; init; }
        public double Voltage { get; init; }
        public double Power { get; init; }

    }
}