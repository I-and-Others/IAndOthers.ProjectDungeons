using UnityEngine;

[System.Serializable]
public class BiomeDefinition
{
    public BiomeType biomeType;

    [Header("Temperature Range")]
    [Range(-1f, 1f)] public float minTemp;
    [Range(-1f, 1f)] public float maxTemp;

    [Header("Moisture Range")]
    [Range(0f, 1f)] public float minMoisture;
    [Range(0f, 1f)] public float maxMoisture;

    [Header("Terrain Settings")]
    public TerrainType terrainType;  // e.g. Water, Forest, Ground, etc.
    public GameObject prefab;        // The visual prefab
    public int movementCost = 1;     // Base cost for movement
}
