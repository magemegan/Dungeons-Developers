using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoboldScript : MonoBehaviour, AICombatBase
{
    // Actions the Cobold can choose
    private enum CoboldAction
    {
        Steal,
        ReturnToDen,
        Attack,
        Flee
    }

    // Actions an adventurer might take
    private enum AdventurerAction
    {
        Attack,
        MoveAway,
        Wait
    }

    // A hypothetical state for Alpha-Beta
    private class CoboldState
    {
        public bool hasGold;
        public bool guildHasGold;
        public bool atDen;

        public float coboldHealth;
        public float playerHealth;
        public float distanceFromDen;
        public float distanceToPlayer;
    }

    private CoboldAction currentAction;

    // References
    private StatScript stats;
    private MovementScript movement;
    private DetectorScript detector;

    // den
    [SerializeField] private Transform den;

    // Combat
    [SerializeField] private float attackDamage = 1f;
    [SerializeField] private float fleeHealth = 0.2f;
    [SerializeField] private float stealDistance = 1f;

    // AI
    [SerializeField] private int searchDepth = 2;

    // target
    private GameObject targetPlayer;

    // loot
    private int goldStolen = 0;
    private List<GameObject> stolenItems = new List<GameObject>();
    private readonly List<GameObject> denLoot = new List<GameObject>();

    private void Awake()
    {
        stats = GetComponent<StatScript>();
        movement = GetComponent<MovementScript>();
        detector = GetComponent<DetectorScript>();
    }


    public void StartTurn()
    {
        Debug.Log("Cobold turn starts!");

        // Find the adventurer
        targetPlayer = detector.GetClosestOpponent();

        if (stats == null || movement == null || detector == null)
        {
            Debug.LogError("Cobold is missing StatScript, MovementScript, or DetectorScript!");
            CombatManager.Instance.EndTurn(gameObject);
            return;
        }

        if (den == null)
        {
            Debug.LogError("Cobold's den is not assigned!");
            CombatManager.Instance.EndTurn(gameObject);
            return;
        }

        targetPlayer = detector.GetClosestOpponent();

        if (targetPlayer == null)
        {
            Debug.LogWarning("Cobold could not find an opponent.");
            CombatManager.Instance.EndTurn(gameObject);
            return;
        }


        // Score possible actions
        currentAction = CoboldDecision();
        
        CombatLog.Instance.Log("Cobold chose action: " + currentAction);

        Debug.Log("Cobold chose action: " + currentAction);

        switch (currentAction)
        {
            case CoboldAction.Steal:
                TryToSteal();
                break;

            case CoboldAction.ReturnToDen:
                ReturnToDen();
                break;

            case CoboldAction.Attack:
                Attack();
                break;

            case CoboldAction.Flee:
                Flee();
                break;
        }

        StartCoroutine(WaitForActionToFinish());
    }

    private IEnumerator WaitForActionToFinish()
    {
        // Wait until movement is done
        while (movement != null && movement.GetCombatMoving())
        {
            yield return null;
        }

        // Give action a frame to finish
        yield return null;

        if (CombatManager.Instance != null)
        {
            EndTurn();
        }
    }




    // DECISION MAKING

    private CoboldAction CoboldDecision()
    {
        CoboldState state = GetCurrentState();

        float bestValue = float.NegativeInfinity;
        CoboldAction bestAction = CoboldAction.ReturnToDen;

        float alpha = float.NegativeInfinity;
        float beta = float.PositiveInfinity;

        foreach (CoboldAction action in GetAvailableActions(state))
        {
            CoboldState result = GetResult(state, action);

            float value = MinValue(result, searchDepth - 1, alpha, beta);

            if (value > bestValue)
            {
                bestValue = value;
                bestAction = action;
            }

            alpha = Mathf.Max(alpha, bestValue);
        }

        return bestAction;
    }

    private float MaxValue(CoboldState state, int depth, float alpha, float beta)
    {
        if (depth <= 0 || IsTerminalState(state))
            return Utility(state);

        float value = float.NegativeInfinity;

        foreach (CoboldAction action in GetAvailableActions(state))
        {
            CoboldState result = GetResult(state, action);

            float actionValue = MinValue(result, depth - 1, alpha, beta);

            value = Mathf.Max(value, actionValue);

            // Alpha-Beta pruning
            if (value >= beta)
                return value;

            alpha = Mathf.Max(alpha, value);
        }

        return value;
    }

    private float MinValue(CoboldState state, int depth, float alpha, float beta)
    {
        if (depth <= 0 || IsTerminalState(state))
            return Utility(state);

        float value = float.PositiveInfinity;

        foreach (AdventurerAction action in GetAdventurerActions(state))
        {
            CoboldState result = GetAdventurerResult(state, action);

            value = Mathf.Min(value, MaxValue(result, depth - 1, alpha, beta));

            // Alpha-Beta pruning
            if (value <= alpha)
                return value;

            beta = Mathf.Min(beta, value);
        }

        return value;
    }








    // UTILITY

    private CoboldState GetCurrentState()
    {
        Vector3Int coboldCell =
            movement.GetTilemap().WorldToCell(transform.position);

        Vector3Int denCell =
            movement.GetTilemap().WorldToCell(den.position);

        bool guildHasGold = false;
        float playerHealth = 0f;
        float playerDistance = 0f;

        if (targetPlayer != null)
        {
            StatScript playerStats =
                targetPlayer.GetComponent<StatScript>();

            if (playerStats != null)
            {
                guildHasGold = GuildManager.Instance.TotalGold > 0;

                playerHealth = playerStats.GetHealth();

                Vector3Int playerCell = movement.GetTilemap().WorldToCell(targetPlayer.transform.position);

                playerDistance = Mathf.Abs(coboldCell.x - playerCell.x) + Mathf.Abs(coboldCell.y - playerCell.y);
            }
        }

        return new CoboldState
        {
            hasGold = stolenItems.Count > 0,
            guildHasGold = guildHasGold,
            atDen = coboldCell == denCell,
            coboldHealth = stats.GetHealth(),
            playerHealth = playerHealth,
            distanceFromDen = Mathf.Abs(coboldCell.x - denCell.x) + Mathf.Abs(coboldCell.y - denCell.y),
            distanceToPlayer = playerDistance
        };
    }

    private float Utility(CoboldState state)
    {
        float utility = 0f;

        if (state.guildHasGold)
            utility += 50f;

        if (state.atDen)
            utility += 50f;

        if (state.guildHasGold)
            utility += 20f;

        utility += state.coboldHealth;
        utility -= state.distanceFromDen;

        return utility;
    }

    private bool IsTerminalState(CoboldState state)
    {
        return state.coboldHealth <= 0 || state.playerHealth <= 0;
    }








    // POSSIBLE ACTIONS

    private CoboldAction[] GetAvailableActions(CoboldState state)
    {
        // Low health makes cobold flee
        if (state.coboldHealth <= stats.GetMaxHealth() * fleeHealth)
            return new[] { CoboldAction.Flee };

        // Carrying loot makes cobold go to den
        if (state.hasGold)
            return new[] { CoboldAction.ReturnToDen };

        List<CoboldAction> actions = new List<CoboldAction>();

        if (state.guildHasGold)
            actions.Add(CoboldAction.Steal);

        actions.Add(CoboldAction.ReturnToDen);

        if (state.distanceToPlayer <= 1f)
            actions.Add(CoboldAction.Attack);

        actions.Add(CoboldAction.Flee);

        return actions.ToArray();
    }

    private AdventurerAction[] GetAdventurerActions(CoboldState state)
    {
        return new[]
        {
            AdventurerAction.Attack,
            AdventurerAction.MoveAway,
            AdventurerAction.Wait
        };
    }

    private CoboldState GetResult(CoboldState state, CoboldAction action)
    {
        CoboldState result = CopyState(state);

        switch (action)
        {
            case CoboldAction.Steal:
                if (result.guildHasGold && result.distanceToPlayer <= stealDistance)
                {
                    result.hasGold = true;
                    result.guildHasGold = false;
                }
                else
                {
                    // Approaching adventurer
                    result.distanceToPlayer = Mathf.Max(0f, result.distanceToPlayer - stats.GetSpeed());
                }
                break;

            case CoboldAction.ReturnToDen:
            case CoboldAction.Flee:
                result.distanceFromDen = Mathf.Max(0f, result.distanceFromDen - stats.GetSpeed());

                result.atDen = result.distanceFromDen == 0f;
                break;

            case CoboldAction.Attack:
                if (result.distanceToPlayer <= 1f)
                    result.playerHealth -= attackDamage;
                break;
        }

        return result;
    }

    private CoboldState GetAdventurerResult(CoboldState state, AdventurerAction action)
    {
        CoboldState result = CopyState(state);

        switch (action)
        {
            case AdventurerAction.Attack:
                if (result.distanceToPlayer <= 1f)
                    result.coboldHealth -= 1f;
                break;

            case AdventurerAction.MoveAway:
                result.distanceToPlayer += 1f;
                break;

            case AdventurerAction.Wait:
                break;
        }

        return result;
    }

    private CoboldState CopyState(CoboldState state)
    {
        return new CoboldState
        {
            hasGold = state.hasGold,
            guildHasGold = state.guildHasGold,
            atDen = state.atDen,
            coboldHealth = state.coboldHealth,
            playerHealth = state.playerHealth,
            distanceFromDen = state.distanceFromDen,
            distanceToPlayer = state.distanceToPlayer
        };
    }











    // ACTUAL ACTIONS

    private void TryToSteal()
    {
        if (GuildManager.Instance.TotalGold <= 0)
        {
            Debug.Log(name + " tried to steal gold, but there is no gold.");
            return;
        }

        if (targetPlayer == null)
        {
            return;
        }

        // Get to player if not close enough
        if (GetTileDistance(transform.position, targetPlayer.transform.position) > stealDistance)
        {
            movement.MoveTowardsCombatTarget(targetPlayer.transform.position);
            return;
        }

        // Steal gold
        GuildManager.Instance.Deposit(-1);

        goldStolen += 1;
    }

    private void ReturnToDen()
    {
        // Store loot in den
        if (IsAtDen())
        {
            StoreLootAtDen();
            return;
        }

        // If adventurer is in the way of den, attack
        if (IsAdjacentToOpponent())
        {
            Attack();
            return;
        }

        movement.MoveTowardsCombatTarget(den.position);
    }

    private void Flee()
    {
        if (IsAtDen())
        {
            StoreLootAtDen();
            return;
        }

        movement.MoveTowardsCombatTarget(den.position);
    }

    private void Attack()
    {
        if (targetPlayer == null)
        {
            targetPlayer = detector.GetClosestOpponent();
        }

        if (targetPlayer == null)
            return;

        if (GetTileDistance(transform.position, targetPlayer.transform.position) <= 1f)
        {
            StatScript playerStats =
                targetPlayer.GetComponent<StatScript>();

            if (playerStats != null)
            {
                playerStats.TakeDamage(attackDamage);
            }
        }
        else
        {
            // If too far go to adventurer
            movement.MoveTowardsCombatTarget(targetPlayer.transform.position);
        }
    }

    private bool IsAdjacentToOpponent()
    {
        if (targetPlayer == null)
            return false;

        return GetTileDistance(transform.position, targetPlayer.transform.position) <= 1f;
    }

    private bool IsAtDen()
    {
        return GetTileDistance(transform.position, den.position) == 0f;
    }

    private float GetTileDistance(Vector3 first, Vector3 second)
    {
        Vector3Int firstCell = movement.GetTilemap().WorldToCell(first);
        Vector3Int secondCell = movement.GetTilemap().WorldToCell(second);

        return Mathf.Abs(firstCell.x - secondCell.x) + Mathf.Abs(firstCell.y - secondCell.y);
    }

    private void StoreLootAtDen()
    {
        if (stolenItems.Count == 0)
        {
            Debug.Log(name + " reached its den.");
            return;
        }

        denLoot.AddRange(stolenItems);
        stolenItems.Clear();
    }





    public void EndTurn()
    {
        CombatManager.Instance.EndTurn(gameObject);
    }
}