using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BiomeSettings", menuName = "World Generation/Biome Settings")]
public class BiomeSettings : ScriptableObject
{
    [System.Serializable]
    public class BiomeDistribution
    {
        public BiomeType biomeType;
        [Range(0, 100)]
        public float targetPercentage;
        public bool enabled = true;
    }

    [Header("Global Settings")]
    public bool enableSeaBorder = true;
    public bool enableIceCaps = true;
    public bool enableMountainRanges = true;

    [Header("Biome Distribution")]
    public List<BiomeDistribution> biomeDistributions = new List<BiomeDistribution>();

    [Header("Special Rules")]
    [Range(0, 1)] public float coastalBiomeWidth = 0.2f;
    [Range(0, 1)] public float mountainRangeDensity = 0.5f;
    public bool allowIslands = true;
    public bool enforceMinimumBiomeSize = true;
} 