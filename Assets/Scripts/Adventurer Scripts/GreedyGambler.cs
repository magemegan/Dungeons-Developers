using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class GreedyGambler : MonoBehaviour
{
    [SerializeField] private int lootRadius = 2;
    public LayerMask lootLayer;
    [SerializeField] private Tilemap floorTilemap;
    private Vector3Int tilePosition;
    private int tilesToChest;
    
    private AgentState state;

    private MovementScript movement;
    private StatScript stats;
    private int speed;

    public enum AgentState
    {
        PlayerControlled,
        Combat,
        MovingToLoot,
        CollectingLoot,
        Attacking
    }

    void CheckForLoot()
    {
        Collider2D[] nearbyLoot = Physics2D.OverlapCircleAll(transform.position, lootRadius, lootLayer);

        foreach (Collider2D loot in nearbyLoot)
        {
            Vector3 lootPosition = loot.transform.position;
            tilePosition = floorTilemap.WorldToCell(lootPosition);
            
            tilesToChest = movement.GetTilesToChest(tilePosition);
            
            Debug.Log("Detected loot: " + loot.name);
            Debug.Log("Loot position: " + lootPosition);
        }
    }

    private void Start()
    {
        movement = GetComponent<MovementScript>();
        stats = GetComponent<StatScript>();
        speed = stats.GetSpeed();
        
        
        state = AgentState.PlayerControlled;
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.Combat)
        {
            CheckForLoot();
        }
    }

    void Move()
    {
        movement.MoveToChest(tilePosition);
    }

    void Attack()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lootRadius);
    }
}
