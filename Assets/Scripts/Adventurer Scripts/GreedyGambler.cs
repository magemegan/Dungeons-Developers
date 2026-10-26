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
    private enum ActionType
    {
        Loot,
        Attack,
        Defend
    }

    void Awake()
    {
        movement = GetComponent<MovementScript>();
        stats = GetComponent<StatScript>();
        detector = GetComponent<DetectorScript>();
        speed = stats.GetSpeed();
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
        if (!turnIsActive)
            return;

        // Find the best available loot target.
        Chest bestChest = FindBestChest();

        // Find the best available opponent.
        GameObject bestEnemy = detector != null
            ? detector.GetClosestOpponent()
            : null;

        // Calculate the utility of each action.
        float lootUtility = CalculateLootUtility(bestChest);
        float attackUtility = CalculateAttackUtility(bestEnemy);
        //float defendUtility = CalculateDefendUtility();

        Debug.Log(
            $"Utilities for {name}: " +
            $"Loot = {lootUtility:F2}, " +
            $"Attack = {attackUtility:F2}"
            //$"Defend = {defendUtility:F2}"
        );

        // Begin with the highest-scoring action.
        ActionType chosenAction = ActionType.Attack;
        float highestUtility = attackUtility;

        if (lootUtility > highestUtility)
        {
            highestUtility = lootUtility;
            chosenAction = ActionType.Loot;
        }

        /*if (defendUtility > highestUtility)
        {
            highestUtility = defendUtility;
            chosenAction = ActionType.Defend;
        }*/

        // Execute the chosen action.
        switch (chosenAction)
        {
            case ActionType.Loot:
                if (bestChest != null)
                    StartCoroutine(MoveToLoot(bestChest));
                else
                    StartCoroutine(Attack());
                break;

            case ActionType.Attack:
                StartCoroutine(Attack());
                break;

            case ActionType.Defend:
                //StartCoroutine(Defend());
                break;
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

            if (chest == null)
                continue;
            
            Vector3Int chestTile = floorTilemap.WorldToCell(chest.transform.position);
            int distance = movement.GetDistance(floorTilemap.WorldToCell(transform.position), chestTile);
            
            // Doesn't select chests we cannot reach this turn
            if (distance > speed)
                continue;

            float value = GetLootValue(chest);
            
            Debug.Log($"Chest: {chest.name}, Gold: {value}, Distance: {distance}");

            float utility = CalculateLootUtility(value, distance);

            if (utility > bestScore)
            {
                bestScore = utility;
                bestChest = chest;
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
            FinishTurn();
            yield break;
        }

        Vector3Int chestTile = floorTilemap.WorldToCell(chest.transform.position);

        Debug.Log(name + " is moving toward " + chest.name + " containg " + chest.GetComponent<LootData>().getValue() + " gold.");

        movement.MoveTowardChest(chestTile, speed);

        while (movement.GetCombatMoving())
        {
            yield return null;
        }

       // Check whether the chest was collected or whether the agent simply reached it.
        if (chest == null)
        {
            state = AgentState.CollectingLoot;
            Debug.Log(name + " collected the chest's loot.");
        }
        else
        {
            Debug.Log(name + " finished moving toward the chest.");
        }

        FinishTurn();
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
        FinishTurn();
    }
    
    // used for FindBestChest()
    private float CalculateLootUtility(float value, int distance)
    {
        if (distance < 0)
            return float.MinValue;

        return value / (distance + 1f);
    }

    private float CalculateLootUtility(Chest chest)
    {
        if (chest == null)
        {
            Debug.LogWarning(name + ": No chest was found.");
            return float.MinValue;
        }

        if (movement == null)
        {
            Debug.LogError(name + ": MovementScript is missing.");
            return float.MinValue;
        }

        if (floorTilemap == null)
        {
            Debug.LogError(name + ": Floor Tilemap is missing.");
            return float.MinValue;
        }

        LootData lootData = chest.GetComponent<LootData>();

        if (lootData == null)
        {
            Debug.LogError(chest.name + ": LootData is missing.");
            return float.MinValue;
        }

        Vector3Int chestTile =
            floorTilemap.WorldToCell(chest.transform.position);

        int distance = movement.GetTilesToChest(chestTile);

        Debug.Log($"{name}: Chest={chest.name}, " +
                  $"Value={lootData.getValue()}, Distance={distance}");

        if (distance < 0)
        {
            Debug.LogWarning(name + ": No path to chest.");
            return float.MinValue;
        }

        float lootValue = lootData.getValue();
        float utility = lootValue / (distance + 1f);

        Debug.Log($"{name}: Loot utility = {utility}");

        return utility;
    }
    private float CalculateAttackUtility(GameObject enemy)
    {
        if (enemy == null)
            return float.MinValue;

        StatScript enemyStats = enemy.GetComponent<StatScript>();

        if (enemyStats == null)
            return float.MinValue;

        // Attacking is normally a low priority.
        float utility = 1f;

        // Only a small bonus for finishing off an enemy.
        if (enemyStats.GetHealth() <= attackDamage)
        {
            utility += 1f;
        }

        return utility;
    }

    private void FinishTurn()
    {
        if (!turnIsActive)
            return;
        
        turnIsActive = false;
        state = AgentState.Combat;
        
        EndTurn();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lootRadius);
    }
}
