using System;

namespace AshesOfTheEarth.World.Generation
{
    public class PerlinNoise
    {
        private const int GRADIENT_SIZE_MAX = 256;
        private readonly Random _random;
        private readonly int[] _p = new int[GRADIENT_SIZE_MAX << 1]; // Permutation table

        public PerlinNoise(int seed)
        {
            _random = new Random(seed);
            InitPermutationTable();
        }

        private void InitPermutationTable()
        {
            int[] source = new int[GRADIENT_SIZE_MAX];
            for (int i = 0; i < GRADIENT_SIZE_MAX; i++)
            {
                source[i] = i;
            }

            // Shuffle
            for (int i = source.Length - 1; i >= 0; i--)
            {
                int k = _random.Next(i + 1);
                int T = source[i];
                source[i] = source[k];
                source[k] = T;
            }

            // Duplicate for wrapping
            for (int i = 0; i < GRADIENT_SIZE_MAX; i++)
            {
                _p[i] = source[i];
                _p[GRADIENT_SIZE_MAX + i] = source[i];
            }
        }

        public double Noise(double x, double y, double z = 0.0)
        {
            int X = (int)Math.Floor(x) & (GRADIENT_SIZE_MAX - 1);
            int Y = (int)Math.Floor(y) & (GRADIENT_SIZE_MAX - 1);
            int Z = (int)Math.Floor(z) & (GRADIENT_SIZE_MAX - 1);

            x -= Math.Floor(x);
            y -= Math.Floor(y);
            z -= Math.Floor(z);

            double u = Fade(x);
            double v = Fade(y);
            double w = Fade(z);

            int A = _p[X] + Y;
            int AA = _p[A] + Z;
            int AB = _p[A + 1] + Z;
            int B = _p[X + 1] + Y;
            int BA = _p[B] + Z;
            int BB = _p[B + 1] + Z;

            return Lerp(w, Lerp(v, Lerp(u, Grad(_p[AA], x, y, z), Grad(_p[BA], x - 1, y, z)),
                                   Lerp(u, Grad(_p[AB], x, y - 1, z), Grad(_p[BB], x - 1, y - 1, z))),
                           Lerp(v, Lerp(u, Grad(_p[AA + 1], x, y, z - 1), Grad(_p[BA + 1], x - 1, y, z - 1)),
                                   Lerp(u, Grad(_p[AB + 1], x, y - 1, z - 1), Grad(_p[BB + 1], x - 1, y - 1, z - 1))));
        }

        private static double Fade(double t) => t * t * t * (t * (t * 6 - 15) + 10);
        private static double Lerp(double t, double a, double b) => a + t * (b - a);

        private static double Grad(int hash, double x, double y, double z)
        {
            int h = hash & 15;
            double u = h < 8 ? x : y;
            double v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        // Octave noise for more detail
        public double OctaveNoise(double x, double y, double z, int octaves, double persistence)
        {
            double total = 0;
            double frequency = 1;
            double amplitude = 1;
            double maxValue = 0;  // Used for normalizing result to 0.0 - 1.0

            for (int i = 0; i < octaves; i++)
            {
                total += Noise(x * frequency, y * frequency, z * frequency) * amplitude;
                maxValue += amplitude;
                amplitude *= persistence;
                frequency *= 2;
            }
            return total / maxValue;
        }
    }
}