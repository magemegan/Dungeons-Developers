using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class GreedyGambler : MonoBehaviour, AICombatBase
{
    [Header("Loot Detection")]
    [SerializeField] private float lootRadius = 2f;
    [SerializeField] private LayerMask lootLayer;
    [SerializeField] private Tilemap floorTilemap;
    
    [Header("Combat")]
    [SerializeField] private float attackDamage = 3f;

    private MovementScript movement;
    [SerializeField] private StatScript stats;
    private DetectorScript detector;
    
    private AgentState state;
    private bool turnIsActive = false;

    private int speed;
    private Vector3Int targetTile;
    private Chest targetChest;
    
    public enum AgentState
    {
        PlayerControlled,
        Combat,
        MovingToLoot,
        CollectingLoot,
        Attacking
    }

    void Awake()
    {
        movement = GetComponent<MovementScript>();
        stats = GetComponent<StatScript>();
        detector = GetComponent<DetectorScript>();
    }

    void Start()
    {
        state = AgentState.PlayerControlled;
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Combat)
        {
            return;
        }

        if (state == AgentState.PlayerControlled)
        {
            state = AgentState.Combat;
        }
    }

    /*void CheckForLoot()
    {
        Collider2D[] nearbyLoot = Physics2D.OverlapCircleAll(transform.position, lootRadius, lootLayer);

        foreach (Collider2D loot in nearbyLoot)
        {
            Vector3 lootPosition = loot.transform.position;
            tilePosition = floorTilemap.WorldToCell(lootPosition);
            
            tilesToChest = movement.GetTilesToChest(tilePosition);
            // find the highest value chest then return its position
            
            
            Debug.Log("Detected loot: " + loot.name);
            Debug.Log("Loot position: " + lootPosition);
        }
    }
    */
    
    public void StartTurn()
    {
        if (turnIsActive)
            return;
        
        turnIsActive = true;
        
        DecideAction();
    }
    
    public void EndTurn()
    {
        CombatManager.Instance.EndTurn(gameObject);
    }

    public void DecideAction()
    {
        Debug.Log("Base Adventurer is deciding their action");

        if (!turnIsActive)
            return;
        
        if (movement == null)
        {
            Debug.LogError(name + ": MovementScript is missing!", this);
            FinishTurn();
            return;
        }

        if (stats == null)
        {
            Debug.LogError(name + ": StatScript is missing!", this);
            FinishTurn();
            return;
        }

        if (detector == null)
        {
            Debug.LogError(name + ": DetectorScript is missing!", this);
            FinishTurn();
            return;
        }
        
        Chest bestChest = FindBestChest();
        if (bestChest != null)
        {
            Debug.Log("Gambler moving towards loot");
            StartCoroutine(MoveToLoot(bestChest));
        }
        else
        {
            Debug.Log("Gambler is starting to attack");
            StartCoroutine(Attack());
        }
    }

    private Chest FindBestChest()
    {
        Collider2D[] nearbyLoot =  Physics2D.OverlapCircleAll(
            transform.position,
            lootRadius,
            lootLayer
        );
        
        Chest bestChest = null;
        float bestScore = float.MinValue;

        foreach (Collider2D loot in nearbyLoot)
        {
            Chest chest = loot.GetComponentInParent<Chest>();

            if (chest != null)
                continue;
            Vector3Int chestTile = floorTilemap.WorldToCell(chest.transform.position);
            int distance = movement.GetDistance(floorTilemap.WorldToCell(transform.position), chestTile);
            
            // Doesn't select chests we cannot reach this turn
            if (distance < speed)
                continue;

            float value = GetLootValue(chest);

            float score = value / (distance + 1f);

            if (score > bestScore)
            {
                bestScore = score;
                bestChest = chest;
                targetTile = chestTile;
            }
        }
        
        return bestChest;
    }

    private float GetLootValue(Chest chest)
    {
        // TODO: Read the gold value from you LootData
        LootData lootData = chest.GetComponent<LootData>();

        if (lootData == null)
            return 0f;

        return lootData.getValue();
    }

    private IEnumerator MoveToLoot(Chest chest)
    {
        state = AgentState.MovingToLoot;

        if (chest == null)
        {
            //FinishTurn();
            yield break;
        }

        Vector3Int chestTile = floorTilemap.WorldToCell(chest.transform.position);

        Debug.Log(name + " is moving toward " + chest.name + " containg " + chest.GetComponent<LootData>().getValue() + " gold.");

        movement.MoveTowardChest(chestTile, speed);

        while (movement.GetCombatMoving())
        {
            yield return null;
        }

        if (chest == null)
        {
            state = AgentState.CollectingLoot;

            Debug.Log(name + " collected chest's loot.");
        }
        else
        {
            {
                Debug.Log(name + " finished moving toward the chest.");
            }

           // FinishTurn();
        }
    }

    private IEnumerator Attack()
    {
        state = AgentState.Attacking;
        
        Debug.Log(gameObject.name + " attacks!");

        GameObject enemy = detector.GetClosestOpponent();

        if (enemy != null)
        {
            StatScript enemyStats = enemy.GetComponent<StatScript>();

            if (enemyStats != null)
            {
                enemyStats.TakeDamage(attackDamage);
            }
        }
        else
        {
            Debug.Log(name + " found no enemy to attack.");
        }
        
        yield return null;
        //FinishTurn();
    }

    private void FinishTurn()
    {
        turnIsActive = false;
        state = AgentState.Combat;
        
        CombatManager.Instance.EndTurn(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lootRadius);
    }
}
