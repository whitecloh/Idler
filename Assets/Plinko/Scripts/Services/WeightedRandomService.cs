using System;
using System.Collections.Generic;
using UnityEngine;

namespace Plinko.Scripts.Services
{
    public sealed class WeightedRandomService
    {
        public T Roll<T>(IReadOnlyList<T> values, Func<T, int> getWeight)
        {
            if (values == null || values.Count == 0)
            {
                return default;
            }

            var totalWeight = 0;
            foreach (var value in values)
            {
                totalWeight += Mathf.Max(0, getWeight(value));
            }

            if (totalWeight <= 0)
            {
                return values[UnityEngine.Random.Range(0, values.Count)];
            }

            var roll = UnityEngine.Random.Range(0, totalWeight);
            var accumulated = 0;
            foreach (var value in values)
            {
                accumulated += Mathf.Max(0, getWeight(value));
                if (roll < accumulated)
                {
                    return value;
                }
            }

            return values[^1];
        }
    }
}