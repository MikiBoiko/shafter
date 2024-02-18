using UnityEngine;

namespace Shafter
{
    public class Noise
    {
        public abstract class Algorithm
        {
            public abstract float Calculate(Vector2 at);
        }

        public class SimplePerlinAlgorithm : Algorithm
        {
            public override float Calculate(Vector2 at)
            {
                Vector2 modulated = at * .075f;
                return Mathf.PerlinNoise(modulated.x, modulated.y);
            }
        }

        public class Generator
        {
            public Algorithm Algorithm { get; set; }

            public float[,] Generate2D(Vector2Int at, int size)
            {
                float[,] result = new float[size, size];
                
                for (int x = 0; x < size; x++)
                    for (int y = 0; y < size; y++)
                    {
                        result[x, y] = Algorithm.Calculate(at + new Vector2Int(x, y));
                    }

                return result;
            }
        }
    }
}
