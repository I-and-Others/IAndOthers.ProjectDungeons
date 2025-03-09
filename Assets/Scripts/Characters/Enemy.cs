using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Enemy : Character
{
    private Character targetCharacter;
    private CharacterMovement movement;

    private void Awake()
    {
        movement = GetComponent<CharacterMovement>();
    }

    public void StartTurn()
    {
        base.StartTurn();
        ExecuteTurn();
    }

    public void ExecuteTurn()
    {
        // Find closest hero
        targetCharacter = FindClosestHero();
        
        if (targetCharacter != null)
        {
            // Move towards target if not in attack range
            MoveTowardsTarget();
        }
        
        // End turn after movement
        GameManager.Instance.EndTurn();
    }

    private Character FindClosestHero()
    {
        var heroes = FindObjectsOfType<Character>()
            .Where(c => !(c is Enemy) && c.gameObject.activeInHierarchy)
            .ToList();

        if (heroes.Count == 0) return null;

        return heroes.OrderBy(h => Vector3.Distance(transform.position, h.transform.position))
            .FirstOrDefault();
    }

    private void MoveTowardsTarget()
    {
        if (targetCharacter == null || movement == null) return;

        // Get the target's hex cell
        HexCell targetCell = targetCharacter.CurrentTile;
        if (targetCell == null) return;

        // Get current position
        HexCell currentCell = CurrentTile;
        if (currentCell == null) return;

        // Get all reachable cells with our current movement points
        var reachableCells = movement.FindReachableCells();
        if (reachableCells.Count == 0) return;

        // Filter out the current cell and find the closest to target
        HexCell bestCell = reachableCells
            .Where(cell => cell != currentCell) // Exclude current cell
            .OrderBy(cell => HexDistance(cell, targetCell)) // Use hex distance instead of Vector3 distance
            .FirstOrDefault();

        // If we found a valid cell to move to
        if (bestCell != null)
        {
            movement.MoveTo(bestCell);
            Debug.Log($"Enemy moving to: ({bestCell.q}, {bestCell.r})");
        }
    }

    private int HexDistance(HexCell a, HexCell b)
    {
        return (Mathf.Abs(a.q - b.q) 
                + Mathf.Abs(a.q + a.r - b.q - b.r)
                + Mathf.Abs(a.r - b.r)) / 2;
    }
} 