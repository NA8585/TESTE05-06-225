// File: SessionInfoModels.cs

using System.Collections.Generic;
using YamlDotNet.Serialization; // Necessário para atributos como YamlMember

namespace SuperBackendNR85IA.Collectors
{
    // Enum para os valores de TrackSurface (útil para verificar se está no box)
    public enum TrackSurface
    {
        NotInWorld = 0,
        OffTrack = 1,
        InPitStall = 2,
        ApproachingPits = 3,
        OnTrack = 4
    }

    // Classes para desserializar o YAML da SessionInfo
    // As propriedades devem corresponder aos nomes no YAML (camelCase)

    public class SessionInfoData
    {
        [YamlMember(Alias = "DriverInfo")]
        public DriverInfo DriverInfo { get; set; }

        [YamlMember(Alias = "WeekendInfo")]
        public WeekendInfo WeekendInfo { get; set; }
        // Adicione outras seções de nível superior do YAML da SessionInfo se precisar
    }

    public class DriverInfo
    {
        [YamlMember(Alias = "DriverCarIdx")]
        public int DriverCarIdx { get; set; }

        [YamlMember(Alias = "Drivers")]
        public List<Driver> Drivers { get; set; }
    }

    public class Driver
    {
        [YamlMember(Alias = "CarIdx")]
        public int CarIdx { get; set; }

        [YamlMember(Alias = "CarSetup")]
        public CarSetup CarSetup { get; set; }
        // Adicione outras propriedades do driver se precisar
    }

    public class CarSetup
    {
        [YamlMember(Alias = "Tires")]
        public Tires Tires { get; set; }
        // Adicione outras seções de setup se precisar
    }

    public class Tires
    {
        [YamlMember(Alias = "Compound")]
        public string Compound { get; set; }
        // Adicione outras propriedades de setup de pneus se precisar
    }

    public class WeekendInfo
    {
        [YamlMember(Alias = "WeekendOptions")]
        public WeekendOptions WeekendOptions { get; set; }
        // Adicione outras informações de fim de semana se precisar
    }

    public class WeekendOptions
    {
        [YamlMember(Alias = "TireCompound")]
        public string TireCompound { get; set; }
        // Adicione outras opções de fim de semana se precisar
    }
}
