using UnityEngine;

public class HexCell : MonoBehaviour
{
    public int q;  // Axial coordinate q
    public int r;  // Axial coordinate r
    public int s;  // Axial coordinate s => -q - r

    public TerrainType terrain;
    public int movementCost;
    public GameObject occupant;  // Could be null if no occupant on the tile

    public void Initialize(int q, int r, TerrainType terrainType, int movementCost)
    {
        this.q = q;
        this.r = r;
        this.s = -q - r;

        this.terrain = terrainType;
        this.movementCost = movementCost;
        occupant = null; // No occupant by default, can be assigned later
    }
}
