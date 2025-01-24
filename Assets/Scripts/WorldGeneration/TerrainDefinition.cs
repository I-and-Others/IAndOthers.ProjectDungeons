using UnityEngine;

[System.Serializable]
public class TerrainDefinition
{
    public TerrainType terrainType;
    [Range(0f, 1f)] public float minThreshold;  // e.g. 0.0
    [Range(0f, 1f)] public float maxThreshold;  // e.g. 0.3
    public GameObject prefab;                   // The hex prefab for this terrain
}
