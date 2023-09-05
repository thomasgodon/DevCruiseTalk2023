using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem.Telemetry
{
    internal class HomeBatteryTelemetry : ITelemetry
    {
        public int Id { get; init; }
        public double Poc { get; init; }

    }
}
