using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Character))]
public class CharacterMovement : MonoBehaviour
{
    private Character character;
    private List<HexCell> currentPath = new List<HexCell>();
    private bool isMoving = false;

    [SerializeField] private float moveSpeed = 5f;
    
    private Dictionary<HexCell, int> pathCosts = new Dictionary<HexCell, int>();
    private Dictionary<HexCell, HexCell> cameFrom = new Dictionary<HexCell, HexCell>();

    private void Start()
    {
        character = GetComponent<Character>();
        GameManager.Instance.onTurnStart.AddListener(OnTurnStart);
    }

    private void OnTurnStart()
    {
        if (character == GameManager.Instance.GetActiveCharacter())
        {
            Debug.Log($"Turn started for {character.data.characterName} with {character.MovementPoints} movement points");
            ShowMovementRange();
        }
    }

    public void ShowMovementRange()
    {
        var reachableCells = FindReachableCells();
        // Here you would highlight these cells
        // Implementation depends on your highlighting system
    }

    public List<HexCell> FindReachableCells()
    {
        var reachable = new List<HexCell>();
        var visited = new HashSet<HexCell>();
        var queue = new Queue<(HexCell cell, int distance)>();

        queue.Enqueue((character.CurrentTile, 0));
        visited.Add(character.CurrentTile);

        while (queue.Count > 0)
        {
            var (current, distance) = queue.Dequeue();
            reachable.Add(current);

            if (distance >= character.MovementPoints) continue;

            foreach (var neighbor in GetNeighbors(current))
            {
                if (!visited.Contains(neighbor) && IsWalkable(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue((neighbor, distance + 1));
                }
            }
        }

        return reachable;
    }

    private bool IsWalkable(HexCell cell)
    {
        return cell.terrain != TerrainType.Water && cell.terrain != TerrainType.Mountain;
    }

    private List<HexCell> GetNeighbors(HexCell cell)
    {
        List<HexCell> neighbors = new List<HexCell>();
        // Hex directions (axial coordinates)
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),   // Right
            new Vector2Int(1, -1),  // Bottom Right
            new Vector2Int(0, -1),  // Bottom Left
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(-1, 1),  // Top Left
            new Vector2Int(0, 1)    // Top Right
        };

        foreach (var dir in directions)
        {
            int neighborQ = cell.q + dir.x;
            int neighborR = cell.r + dir.y;

            // Find the hex at these coordinates
            HexCell neighbor = FindObjectsOfType<HexCell>()
                .FirstOrDefault(h => h.q == neighborQ && h.r == neighborR);

            if (neighbor != null)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    private List<HexCell> FindPath(HexCell start, HexCell goal)
    {
        if (!IsWalkable(goal)) return null;

        var frontier = new PriorityQueue<HexCell>();
        frontier.Enqueue(start, 0);

        cameFrom.Clear();
        pathCosts.Clear();

        cameFrom[start] = null;
        pathCosts[start] = 0;

        while (frontier.Count > 0)
        {
            var currentCell = frontier.Dequeue();

            if (currentCell == goal)
            {
                break;
            }

            foreach (var next in GetNeighbors(currentCell))
            {
                if (!IsWalkable(next)) continue;

                int newCost = pathCosts[currentCell] + 1; // Assuming each step costs 1
                if (!pathCosts.ContainsKey(next) || newCost < pathCosts[next])
                {
                    pathCosts[next] = newCost;
                    int priority = newCost + HexDistance(next, goal);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = currentCell;
                }
            }
        }

        // If we couldn't reach the goal
        if (!cameFrom.ContainsKey(goal))
            return null;

        // Reconstruct path
        var path = new List<HexCell>();
        var current = goal;

        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Reverse();

        return path;
    }

    private int HexDistance(HexCell a, HexCell b)
    {
        return (Mathf.Abs(a.q - b.q) 
                + Mathf.Abs(a.q + a.r - b.q - b.r)
                + Mathf.Abs(a.r - b.r)) / 2;
    }

    public void MoveTo(HexCell targetCell)
    {
        if (isMoving || character != GameManager.Instance.GetActiveCharacter())
        {
            Debug.Log("Cannot move: Either already moving or not active character");
            return;
        }

        var path = FindPath(character.CurrentTile, targetCell);
        if (path == null)
        {
            Debug.Log("No valid path found to target cell");
            return;
        }

        if (path.Count > character.MovementPoints)
        {
            Debug.Log($"Path too long: {path.Count} steps but only {character.MovementPoints} movement points available");
            return;
        }

        Debug.Log($"Starting movement along path of {path.Count} steps");
        StartCoroutine(FollowPath(path));
    }

    private System.Collections.IEnumerator FollowPath(List<HexCell> path)
    {
        isMoving = true;
        currentPath = path;

        foreach (var cell in path)
        {
            Vector3 targetPos = cell.transform.position + Vector3.up * 0.1f;
            
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }

            character.CurrentTile = cell;
            character.MovementPoints--;
        }

        isMoving = false;
        currentPath.Clear();
    }
}

// Helper class for pathfinding
public class PriorityQueue<T>
{
    private List<(T item, int priority)> elements = new List<(T, int)>();

    public int Count => elements.Count;

    public void Enqueue(T item, int priority)
    {
        elements.Add((item, priority));
        elements.Sort((a, b) => a.priority.CompareTo(b.priority));
    }

    public T Dequeue()
    {
        if (elements.Count == 0)
            throw new System.InvalidOperationException("Queue is empty");

        T item = elements[0].item;
        elements.RemoveAt(0);
        return item;
    }
} 