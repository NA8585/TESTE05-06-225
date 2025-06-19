namespace SuperBackendNR85IA.Models
{
    // Representa um carro próximo ao jogador para exibição no radar
    public class ProximityCar
    {
        public int CarIdx { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public int ClassId { get; set; }

        public float Distance => System.MathF.Sqrt((X * X) + (Y * Y));
    }
}
