namespace SuperBackendNR85IA.Models
{
    public class WeekendInfo
    {
        public string TrackName        { get; set; } = string.Empty;
        public string TrackDisplayName { get; set; } = string.Empty;
        public float  TrackLengthKm    { get; set; }
        public string TrackConfigName  { get; set; } = string.Empty;
        public string SessionType      { get; set; } = string.Empty;
        public string Skies            { get; set; } = string.Empty;
        public float  WindSpeed        { get; set; }
        public float  WindDir          { get; set; }
        public float  AirPressure      { get; set; }
        public float  RelativeHumidity { get; set; }
        public float  ChanceOfRain     { get; set; }
        public string ForecastType     { get; set; } = string.Empty;
        public float  TrackWindVel      { get; set; }
        public float  TrackAirTemp      { get; set; }
        public string TrackNumTurns     { get; set; } = string.Empty;
        public int    NumCarClasses     { get; set; }

        public float TrackLengthMiles => TrackLengthKm * 0.621371f;
    }
}
