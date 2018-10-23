using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class MathHelper
{
    /// <summary>
    /// Min (inclusive), Max (inclusive).
    /// </summary>
    public struct IntRange
    {

        public int Min;
        public int Max;
        public float Weight;

        public IntRange(int min, int max, float weight)
        {
            Min = min;
            Max = max;
            Weight = weight;
        }

    }
    /// <summary>
    /// Min (inclusive), Max (inclusive).
    /// </summary>
    public struct FloatRange
    {
        public float Min;
        public float Max;
        public float Weight;


        public FloatRange(float min, float max, float weight)
        {
            Min = min;
            Max = max;
            Weight = weight;
        }
    }

    public static class RandomRange
    {
        /// <summary>
        /// Returns a weighted random integer based on input parameters.
        /// </summary>
        /// <param name="ranges"></param>
        /// <returns></returns>
        public static int WeightedRange(params IntRange[] ranges)
        {
            if (ranges.Length == 0) throw new System.ArgumentException("At least one range must be included.");
            if (ranges.Length == 1) return Random.Range(ranges[0].Max, ranges[0].Min);

            float total = 0f;
            for (int i = 0; i < ranges.Length; i++) total += ranges[i].Weight;

            float r = Random.value;
            float s = 0f;

            int cnt = ranges.Length - 1;
            for (int i = 0; i < cnt; i++)
            {
                s += ranges[i].Weight / total;
                if (s >= r)
                {
                    return Random.Range(ranges[i].Max, ranges[i].Min);
                }
            }

            return Random.Range(ranges[cnt].Max, ranges[cnt].Min);
        }

        /// <summary>
        /// Returns a weighted random float based on input parameters.
        /// </summary>
        /// <param name="ranges"></param>
        /// <returns></returns>
        public static float WeightedRange(params FloatRange[] ranges)
        {
            if (ranges.Length == 0) throw new System.ArgumentException("At least one range must be included.");
            if (ranges.Length == 1) return Random.Range(ranges[0].Max, ranges[0].Min);

            float total = 0f;
            for (int i = 0; i < ranges.Length; i++) total += ranges[i].Weight;

            float r = Random.value;
            float s = 0f;

            int cnt = ranges.Length - 1;
            for (int i = 0; i < cnt; i++)
            {
                s += ranges[i].Weight / total;
                if (s >= r)
                {
                    return Random.Range(ranges[i].Max, ranges[i].Min);
                }
            }

            return Random.Range(ranges[cnt].Max, ranges[cnt].Min);
        }
    }

    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
