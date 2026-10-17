using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class MovementTemplate : MonoBehaviour
{
    private bool isMoving;
    private Vector3 origPos, targetPos;

    [SerializeField] private Tilemap tilemap;
    [SerializeField] private float timeToMove = 0.2f;

    void Start()
    {
        // Snap to center of the tile its most on
        Vector3Int currentCell = tilemap.WorldToCell(transform.position);
        transform.position = tilemap.GetCellCenterWorld(currentCell);
    }

    void Update()
    {
        if (Keyboard.current.wKey.isPressed && !isMoving)
            StartCoroutine(MovePlayer(Vector3Int.up));

        if (Keyboard.current.aKey.isPressed && !isMoving)
            StartCoroutine(MovePlayer(Vector3Int.left));

        if (Keyboard.current.sKey.isPressed && !isMoving)
            StartCoroutine(MovePlayer(Vector3Int.down));

        if (Keyboard.current.dKey.isPressed && !isMoving)
            StartCoroutine(MovePlayer(Vector3Int.right));
    }
    

    private IEnumerator MovePlayer(Vector3Int direction)
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

        //origPos = transform.position;
        //targetPos = origPos + direction;

        while(elapsedTime < timeToMove)
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
