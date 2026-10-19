using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class MovementScript: MonoBehaviour
{
    // bool
    private bool adventurer;
    private bool isMoving;
    public bool IsSelected => selectedAdventurer == this;
    private bool combatMoving;

    // Pathfinding
    private List<Vector3Int> currentPath;
    private int pathIndex;

    // Movement
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Transform target;
    [SerializeField] private float timeToMove = 0.2f;
    private Vector3 origPos, targetPos;

    // References
    private DetectorScript detector;
    private StatScript stats;

    // Adventurer selected
    private static MovementScript selectedAdventurer;

    // mouse click once per frame
    private static int lastMouseClickFrame = -1;

    // Sprite
    [SerializeField] private Color selectedColor = Color.yellow;
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;

    // Values
    private int combatMovesRemaining;

    // Getters
    public Tilemap GetTilemap() { return tilemap; }
    public bool GetCombatMoving() { return combatMoving; }

    void Awake()
    {
        detector = GetComponent<DetectorScript>();
        stats = GetComponent<StatScript>();

        if (gameObject.CompareTag("Adventurer"))
            adventurer = true;
        else if (gameObject.CompareTag("Enemy"))
            adventurer = false;

        // sprite handling
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        originalColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }
    }

    void Start()
    {
        // Snap to center of the tile its most on
        Vector3Int currentCell = tilemap.WorldToCell(transform.position);
        transform.position = tilemap.GetCellCenterWorld(currentCell);
    }

    void Update()
    {
        // Combat movement handled seperately
        if (GameManager.Instance.CurrentState != GameManager.GameState.Exploration)
            return;

        // Adventurers use mouse movement only
        if (!adventurer)
            return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && lastMouseClickFrame != Time.frameCount)
        {
            lastMouseClickFrame = Time.frameCount;
            SelectAdventurer();
        }

        if (selectedAdventurer != this)
            return;

        if (target == null)
            return;

        // Create path to target
        if (currentPath == null || pathIndex >= currentPath.Count)
            CreatePath();

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











    public void SetTarget(Vector3 targetPosition)
    {
        if (target == null)
        {
            GameObject targetObject = new GameObject("AITarget");
            target = targetObject.transform;
        }

        target.position = targetPosition;

        currentPath = null;
        pathIndex = 0;
    }

















    // MOUSE CLICK HANDLING


    private void SelectAdventurer()
    {
        MovementScript clickedCharacter = GetClickedCharacter();

        // If character was clicked
        if (clickedCharacter != null)
        {
            if (!clickedCharacter.adventurer)
                return;

            // Deselect chosen character
            if (selectedAdventurer == clickedCharacter)
            {
                clickedCharacter.SetSelectionHighlight(false);
                selectedAdventurer = null;

                Debug.Log(clickedCharacter.name + " deselected.");
            }
            else
            {
                if (selectedAdventurer != null)
                {
                    selectedAdventurer.SetSelectionHighlight(false);
                }

                selectedAdventurer = clickedCharacter;
                selectedAdventurer.SetSelectionHighlight(true);

                Debug.Log(clickedCharacter.name + " selected.");
            }

            return;
        }

        // Clicking somewhere else on screen moves selected character
        if (selectedAdventurer != null)
        {
            selectedAdventurer.SetTargetFromMouse();
        }
    }

    private MovementScript GetClickedCharacter()
    {
        if (Camera.main == null || Mouse.current == null)
            return null;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        RaycastHit2D[] hits2D = Physics2D.GetRayIntersectionAll(ray);

        foreach (RaycastHit2D hit in hits2D)
        {
            MovementScript character =
                hit.collider.GetComponentInParent<MovementScript>();

            if (character != null)
                return character;
        }

        return null;
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

    private void SetSelectionHighlight(bool selected)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] == null)
                continue;

            if (selected)
            {
                // selection color
                spriteRenderers[i].color = selectedColor;
            }
            else
            {
                // original color
                spriteRenderers[i].color = originalColors[i];
            }
        }
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


    public void MoveTowardsCombatTarget(Vector3 targetPosition)
    {
        // Already moving
        if (isMoving || combatMoving)
            return;

        if (tilemap == null)
        {
            Debug.LogError(name + " has no Tilemap assigned!");
            return;
        }

        if (stats == null)
        {
            Debug.LogError(name + " has no StatScript!");
            return;
        }

        // Path to target
        Vector3Int startCell = tilemap.WorldToCell(transform.position);
        Vector3Int targetCell = tilemap.WorldToCell(targetPosition);

        List<Vector3Int> bestPath = null;

        foreach (Vector3Int neighbor in GetNeighbors(targetCell))
        {
            if (!tilemap.HasTile(neighbor))
                continue;

            List<Vector3Int> candidatePath = FindPath(startCell, neighbor);

            if (candidatePath != null &&
                (bestPath == null || candidatePath.Count < bestPath.Count))
            {
                bestPath = candidatePath;
            }
        }

        // No path
        if (bestPath == null)
        {
            Debug.LogWarning(name + " could not find a path to a tile next to the target.");
            return;
        }

        // Already next to the target
        if (bestPath.Count == 0)
        {
            Debug.Log(name + " is already next to the target.");
            return;
        }

        currentPath = bestPath;
        pathIndex = 0;
        combatMovesRemaining = stats.GetSpeed();

        // Limit movement to the character's speed.
        combatMoving = true;

        StartCoroutine(MoveOnCombatPath());
    }










    // MOVEMENT


    private IEnumerator MoveOnCombatPath()
    {
        while (combatMovesRemaining > 0 && currentPath != null && pathIndex < currentPath.Count)
        {
            Vector3Int nextCell = currentPath[pathIndex];
            Vector3Int currentCell = tilemap.WorldToCell(transform.position);

            Vector3Int direction = nextCell - currentCell;

            yield return StartCoroutine(MoveDirection(direction));

            combatMovesRemaining--;
        }

        combatMoving = false;

        // Combat movement is finished
        currentPath = null;
        target = null;

        Debug.Log(name + " finished combat movement.");
    }


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

        if (!combatMoving)
            detector.CheckForEnemies();
    }

    // For getting into combat
    public void StopMovement()
    {
        StopAllCoroutines();

        isMoving = false;
        target = null;
        currentPath = null;
        combatMovesRemaining = 0;
        target = null;
        pathIndex = 0;

        if (selectedAdventurer == this)
        {
            SetSelectionHighlight(false);
            selectedAdventurer = null;
        }
    }
    
    // used for the greedy gambler within combat
    public void MoveToChest(Vector3Int chestCell)
    {
        // Create a target if one doesn't exist.
        if (target == null)
        {
            GameObject targetObject = new GameObject("Target");
            target = targetObject.transform;
        }

        target.position = tilemap.GetCellCenterWorld(chestCell);

        currentPath = null;
        pathIndex = 0;
    }
    // also used for the greedy gambler
    public int GetTilesToChest(Vector3Int chestCell)
    {
        // Find the tile the adventurer is currently on
        Vector3Int startCell =
            tilemap.WorldToCell(transform.position);

        // Calculate the path to the chest
        List<Vector3Int> path = FindPath(startCell, chestCell);

        // Return -1 if no path exists
        if (path == null)
        {
            return -1;
        }

        // Each cell in the path represents one tile to move
        return path.Count;
    }

    public void MoveTowardChest(Vector3Int chestCell, int speed)
    {
        Vector3Int startCell =
            tilemap.WorldToCell(transform.position);

        currentPath = FindPath(startCell, chestCell);
        pathIndex = 0;

        if (currentPath == null)
        {
            target = null;
            return;
        }

        int maxTilesThisTurn = Mathf.Min(speed, currentPath.Count);

        if (target == null)
        {
            GameObject targetObject = new GameObject("Target");
            target = targetObject.transform;
        }
    }
}
