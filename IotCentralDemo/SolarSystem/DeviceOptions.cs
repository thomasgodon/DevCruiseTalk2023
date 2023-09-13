namespace SolarSystem
{
    public class DeviceOptions
    {
        public string DeviceId { get; init; } = string.Empty;
        public string IdScope { get; init; } = string.Empty;
        public string PrimaryKey { get; set; } = string.Empty;
        public string SecondaryKey { get; set; } = string.Empty;
    }
}