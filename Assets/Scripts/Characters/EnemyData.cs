using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Game/Enemy Data")]
public class EnemyData : CharacterData
{
    [Header("Enemy Specific Settings")]
    public int detectionRange = 5;    // How far the enemy can see
    public int preferredRange = 1;    // Preferred distance to target
    public bool isAggressive = true;  // Whether enemy actively seeks targets
} 