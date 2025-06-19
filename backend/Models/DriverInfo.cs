namespace SuperBackendNR85IA.Models
{
    public class DriverInfo
    {
        public int    CarIdx            { get; set; }
        public string UserName          { get; set; } = string.Empty;
        public string TeamName          { get; set; } = string.Empty;
        public int    UserID            { get; set; }
        public int    TeamID            { get; set; }
        public string CarNumber         { get; set; } = string.Empty;
        public int    IRating           { get; set; }
        public string LicString         { get; set; } = string.Empty;
        public int    LicLevel          { get; set; }
        public int    LicSubLevel       { get; set; }
        public string CarPath           { get; set; } = string.Empty;
        public int    CarClassID        { get; set; }
        public string CarClassShortName { get; set; } = string.Empty;
        public float  CarClassRelSpeed  { get; set; }
        public float  CarClassEstLapTime { get; set; }
        public string TireCompound { get; set; } = string.Empty;
        public int    TeamIncidentCount { get; set; }

        /// <summary>Formatted display name combining car number and user.</summary>
        public string DisplayName => $"#{CarNumber} - {UserName}";
    }
}
