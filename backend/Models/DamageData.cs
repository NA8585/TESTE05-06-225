namespace SuperBackendNR85IA.Models
{
    public record DamageData
    {
        public float LfDamage { get; set; }
        public float RfDamage { get; set; }
        public float LrDamage { get; set; }
        public float RrDamage { get; set; }
        public float FrontWingDamage { get; set; }
        public float RearWingDamage { get; set; }
        public float EngineDamage { get; set; }
        public float GearboxDamage { get; set; }
        public float SuspensionDamage { get; set; }
        public float ChassisDamage { get; set; }
    }
}
