using System;

namespace SuperBackendNR85IA.Utilities
{
    public static class DataValidator
    {
        public static float EnsurePositive(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value) || value < 0f ? 0f : value;
        }

        public static double EnsurePositive(double value)
        {
            return double.IsNaN(value) || double.IsInfinity(value) || value < 0.0 ? 0.0 : value;
        }

        public static int EnsureNonNegative(int value)
        {
            return value < 0 ? 0 : value;
        }

        public static long EnsureNonNegative(long value)
        {
            return value < 0 ? 0 : value;
        }

        public static void EnsureArraySize<T>(ref T[] array, int size)
        {
            if (array == null)
                array = new T[size];
            else if (array.Length < size)
                Array.Resize(ref array, size);
        }
    }
}
