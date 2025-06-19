namespace SuperBackendNR85IA.Models
{
    public partial record DamageData
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

        /// <summary>
        /// Quick helper to know if the car has any damage recorded.
        /// </summary>
        public bool HasAnyDamage()
        {
            return LfDamage > 0f || RfDamage > 0f || LrDamage > 0f || RrDamage > 0f
                   || FrontWingDamage > 0f || RearWingDamage > 0f
                   || EngineDamage > 0f || GearboxDamage > 0f
                   || SuspensionDamage > 0f || ChassisDamage > 0f;
        }
    }
}
