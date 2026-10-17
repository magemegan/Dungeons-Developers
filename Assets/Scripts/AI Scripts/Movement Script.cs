using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class MovementScript: MonoBehaviour
{
    private bool adventurer;
    private bool isMoving;

    // Pathfinding
    private List<Vector3Int> currentPath;
    private int pathIndex;

    // Movement
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Transform target;
    [SerializeField] private float timeToMove = 0.2f;
    private Vector3 origPos, targetPos;

    private DetectorScript detector;
    private StatScript stats;

    void Awake()
    {
        detector = GetComponent<DetectorScript>();
        stats = GetComponent<StatScript>();
        adventurer = stats.GetAdventurer();
    }

    void Start()
    {
        // Snap to center of the tile its most on
        Vector3Int currentCell = tilemap.WorldToCell(transform.position);
        transform.position = tilemap.GetCellCenterWorld(currentCell);
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Exploration)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame && adventurer)
        {
            SetTargetFromMouse();
        } else
        {
            // Enemy will occassionally move randomly
        }

        if (target == null)
            return;

        // Create path to target
        if (currentPath == null || pathIndex >= currentPath.Count)
        {
            CreatePath();
        }

        // Already moving
        if (isMoving)
            return;

        // Follow path.
        if (currentPath != null && pathIndex < currentPath.Count)
        {
            Vector3Int nextCell = currentPath[pathIndex];
            Vector3Int currentCell = tilemap.WorldToCell(transform.position);
            Vector3Int direction = nextCell - currentCell;

            StartCoroutine(MoveDirection(direction));
        }
        else
        {
            // target is reached
            currentPath = null;
            target = null;
        }
    }

    private void SetTargetFromMouse()
    {
        // Mouse position to world position
        Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        // put click to tile
        mouseWorldPosition.z = 0f;
        Vector3Int clickedCell = tilemap.WorldToCell(mouseWorldPosition);

        // make sure there is a tile there
        if (!tilemap.HasTile(clickedCell))
            return;

        // Get center of the tile
        Vector3 clickedTileCenter = tilemap.GetCellCenterWorld(clickedCell);

        // Create target on tile
        if (target == null)
        {
            GameObject targetObject = new GameObject("Target");
            target = targetObject.transform;
        }

        // target on center of tile
        target.position = clickedTileCenter;
    }








    // PATHFINDING


    private void CreatePath()
    {
        Vector3Int startCell = tilemap.WorldToCell(transform.position);

        Vector3Int targetCell = tilemap.WorldToCell(target.position);

        currentPath = FindPath(startCell, targetCell);

        pathIndex = 0;

        // No path found
        if (currentPath == null)
        {
            Debug.Log("No path found!");

            target = null;
        }
    }

    private List<Vector3Int> FindPath(Vector3Int start, Vector3Int goal)
    {
        List<Vector3Int> openList = new List<Vector3Int>();
        HashSet<Vector3Int> closedList = new HashSet<Vector3Int>();

        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        // cost from start to each tile
        Dictionary<Vector3Int, int> gCost = new Dictionary<Vector3Int, int>();

        // Total cost
        Dictionary<Vector3Int, int> fCost = new Dictionary<Vector3Int, int>();


        // Search starts at NPC
        openList.Add(start);
        gCost[start] = 0;
        fCost[start] = GetDistance(start, goal);

        while (openList.Count > 0)
        {
            // find tile with lowest cost
            Vector3Int current = GetLowestFCost(openList, fCost);


            // target reached
            if (current == goal)
            {
                return ReconstructPath(cameFrom, current, start);
            }


            // move current
            openList.Remove(current);
            closedList.Add(current);


            // check neighbor tiles
            foreach (Vector3Int neighbor in GetNeighbors(current))
            {
                // Don't walk through empty tiles
                if (!tilemap.HasTile(neighbor))
                    continue;

                // Don't examine same tile
                if (closedList.Contains(neighbor))
                    continue;


                // Moving one tile costs 1
                int tentativeGCost = gCost[current] + 1;


                // Check if this is a new tile
                if (!gCost.ContainsKey(neighbor))
                {
                    gCost[neighbor] = tentativeGCost;
                    fCost[neighbor] = tentativeGCost + GetDistance(neighbor, goal);
                    cameFrom[neighbor] = current;
                    openList.Add(neighbor);
                }

                // Is this a better route to the tile?
                else if (tentativeGCost < gCost[neighbor])
                {
                    gCost[neighbor] = tentativeGCost;
                    fCost[neighbor] = tentativeGCost + GetDistance(neighbor, goal);
                    cameFrom[neighbor] = current;
                }
            }
        }
        // No path exists
        return null;
    }

    private List<Vector3Int> GetNeighbors(Vector3Int cell)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>();

        neighbors.Add(cell + Vector3Int.up);
        neighbors.Add(cell + Vector3Int.down);
        neighbors.Add(cell + Vector3Int.left);
        neighbors.Add(cell + Vector3Int.right);

        return neighbors;
    }

    private int GetDistance(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private Vector3Int GetLowestFCost(List<Vector3Int> openList, Dictionary<Vector3Int, int> fCost)
    {
        Vector3Int lowest = openList[0];

        for (int i = 1; i < openList.Count; i++)
        {
            Vector3Int current = openList[i];

            if (fCost[current] < fCost[lowest])
                lowest = current;
        }

        return lowest;
    }

    private List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current, Vector3Int start)
    {
        List<Vector3Int> path = new List<Vector3Int>();

        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        // Reverse so the path goes from NPC to target.
        path.Reverse();

        return path;
    }











    // MOVEMENT


    private IEnumerator MoveDirection(Vector3Int direction)
    {
        isMoving = true;

        // current tile
        Vector3Int currentCell = tilemap.WorldToCell(transform.position);

        // Next tile
        Vector3Int targetCell = currentCell + direction;

        // Don't move if there isn't a tile
        if (!tilemap.HasTile(targetCell))
        {
            isMoving = false;
            yield break;
        }

        //Tile to world position
        origPos = tilemap.GetCellCenterWorld(currentCell);
        targetPos = tilemap.GetCellCenterWorld(targetCell);

        float elapsedTime = 0;

        while (elapsedTime < timeToMove)
        {
            transform.position = Vector3.Lerp(origPos, targetPos, (elapsedTime / timeToMove));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Make sure NPC is in the middle of next tile
        transform.position = targetPos;

        isMoving = false;

        pathIndex++;

        detector.CheckForEnemies();
    }

    // For getting into combat
    public void StopMovement()
    {
        StopAllCoroutines();
        isMoving = false;
        currentPath = null;
    }
}
