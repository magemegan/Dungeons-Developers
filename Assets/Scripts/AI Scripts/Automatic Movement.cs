using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class AutomaticMovement : MonoBehaviour
{
    private bool isMoving;
    private Vector3 origPos, targetPos;

    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Transform target;
    [SerializeField] private float timeToMove = 0.2f;

    void Start()
    {
        // Snap to center of the tile its most on
        Vector3Int currentCell = tilemap.WorldToCell(transform.position);
        transform.position = tilemap.GetCellCenterWorld(currentCell);
    }

    void Update()
    {
        // Already moving
        if (isMoving)
            return;

        if (target == null)
            return;

        // Find tile the NPC + target are on
        Vector3Int currentCell = tilemap.WorldToCell(transform.position);
        Vector3Int targetCell = tilemap.WorldToCell(target.position);

        // Reached target
        if (currentCell == targetCell)
            return;

        // determine direction
        Vector3Int direction = Vector3Int.zero;

        // horizontal movement first
        if (currentCell.x < targetCell.x)
        {
            direction = Vector3Int.right;
        }
        else if (currentCell.x > targetCell.x)
        {
            direction = Vector3Int.left;
        }
        else if (currentCell.y < targetCell.y)
        {
            direction = Vector3Int.up;
        }
        else if (currentCell.y > targetCell.y)
        {
            direction = Vector3Int.down;
        }

        // Start moving toward the target.
        StartCoroutine(MoveDirection(direction));
    }


    private IEnumerator MoveDirection(Vector3Int direction)
    {
        isMoving = true;

        // Tile NPC is on
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
    }
}
