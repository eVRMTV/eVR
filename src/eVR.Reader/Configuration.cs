namespace eVR.Reader
{
    /// <summary>
    /// Class representing the content of the part "Configuration" in file appsettings.json
    /// </summary>
    public class Configuration
    {
        public string CSCAFolder { get; set; } = string.Empty;
        public int CardAccessDelay { get; set; }
        public int ReadTimeout { get; set; }
        public int[] MonitorCardReaders { get; set; } = [];
    }
}
