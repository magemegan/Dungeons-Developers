using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptiveFighter : MonoBehaviour, AICombatBase
{
    private enum FighterAction
    {
        Attack,
        Approach,
        Hold
    }

    [Header("Combat")]
    [SerializeField] private float attackDamage = 3f;

    [Header("Q-Learning")]
    [Range(0f, 1f)]
    [SerializeField] private float learningRate = 0.25f;

    [Range(0f, 1f)]
    [SerializeField] private float discountFactor = 0.90f;

    [Range(0f, 1f)]
    [SerializeField] private float explorationRate = 0.25f;

    [SerializeField] private float minimumExplorationRate = 0.05f;
    [SerializeField] private float explorationDecay = 0.995f;

    [Header("Rewards")]
    [SerializeField] private float damageEnemyReward = 5f;
    [SerializeField] private float defeatEnemyReward = 20f;
    [SerializeField] private float takeDamagePenalty = -5f;
    [SerializeField] private float moveCloserReward = 2f;
    [SerializeField] private float invalidActionPenalty = -3f;
    [SerializeField] private float holdPenalty = -1f;

    private StatScript stats;
    private MovementScript movement;
    private DetectorScript detector;

    // Each state key stores one Q-value for every FighterAction.
    private readonly Dictionary<string, float[]> qTable = new Dictionary<string, float[]>();

    // Saved transition information lets the Fighter learn from damage taken
    // between one turn and the next.
    private string previousState;
    private FighterAction previousAction;
    private float healthAtEndOfLastTurn;
    private bool hasPreviousTurn;

    private GameObject targetEnemy;

    private void Awake()
    {
        stats = GetComponent<StatScript>();
        movement = GetComponent<MovementScript>();
        detector = GetComponent<DetectorScript>();
    }

    public void StartTurn()
    {
        if (stats == null || movement == null || detector == null)
        {
            Debug.LogError(name + " is missing StatScript, MovementScript, or DetectorScript.");
            EndTurn();
            return;
        }

        targetEnemy = detector.GetClosestOpponent();

        if (targetEnemy == null)
        {
            Debug.LogWarning(name + " could not find an enemy.");
            EndTurn();
            return;
        }

        string currentState = GetState(targetEnemy);
        EnsureStateExists(currentState);

        // Learn from any damage received since the Fighter's last turn.
        if (hasPreviousTurn)
        {
            float reward = 0f;
            float healthLost = healthAtEndOfLastTurn - stats.GetHealth();

            if (healthLost > 0f)
            {
                reward += takeDamagePenalty * healthLost;
            }

            UpdateQ(previousState, previousAction, reward, currentState);
        }

        FighterAction action = ChooseAction(currentState);

        Debug.Log(name + " state: " + currentState +
                  " | chose: " + action +
                  " | epsilon: " + explorationRate.ToString("F2"));

        StartCoroutine(PerformAction(currentState, action));
    }

    private IEnumerator PerformAction(string state, FighterAction action)
    {
        float reward = 0f;

        switch (action)
        {
            case FighterAction.Attack:
                reward += AttackTarget();
                break;

            case FighterAction.Approach:
                if (targetEnemy == null || !targetEnemy.activeInHierarchy)
                {
                    reward += invalidActionPenalty;
                    break;
                }

                int distanceBefore = GetDistanceTo(targetEnemy);

                if (distanceBefore <= 1)
                {
                    Debug.Log(name + " is already beside the enemy.");
                    reward += invalidActionPenalty;
                    break;
                }

                movement.MoveTowardsCombatTarget(targetEnemy.transform.position);

                while (movement.GetCombatMoving())
                {
                    yield return null;
                }

                int distanceAfter = GetDistanceTo(targetEnemy);
                reward += distanceAfter < distanceBefore
                    ? moveCloserReward
                    : invalidActionPenalty;
                break;

            case FighterAction.Hold:
                reward += holdPenalty;
                Debug.Log(name + " holds position.");
                break;
        }

        // The target can be disabled immediately after taking lethal damage.
        string nextState = targetEnemy != null && targetEnemy.activeInHierarchy
            ? GetState(targetEnemy)
            : "EnemyDefeated";

        EnsureStateExists(nextState);
        UpdateQ(state, action, reward, nextState);

        previousState = state;
        previousAction = action;
        healthAtEndOfLastTurn = stats.GetHealth();
        hasPreviousTurn = true;

        explorationRate = Mathf.Max(
            minimumExplorationRate,
            explorationRate * explorationDecay
        );

        PrintQValues(state);

        yield return null;
        EndTurn();
    }

    private float AttackTarget()
    {
        if (targetEnemy == null || !targetEnemy.activeInHierarchy)
            return invalidActionPenalty;

        int distance = GetDistanceTo(targetEnemy);

        if (distance > 1)
        {
            Debug.Log(name + " tried to attack, but the enemy was too far away.");
            return invalidActionPenalty;
        }

        StatScript enemyStats = targetEnemy.GetComponent<StatScript>();

        if (enemyStats == null)
            return invalidActionPenalty;

        float healthBefore = enemyStats.GetHealth();

        Debug.Log(name + " attacks " + targetEnemy.name + "!");
        enemyStats.TakeDamage(attackDamage);

        float damageDone = Mathf.Max(0f, healthBefore - enemyStats.GetHealth());
        float reward = damageDone > 0f ? damageEnemyReward : 0f;

        if (!targetEnemy.activeInHierarchy || enemyStats.GetHealth() <= 0f)
        {
            reward += defeatEnemyReward;
        }

        return reward;
    }

    private FighterAction ChooseAction(string state)
    {
        EnsureStateExists(state);

        if (Random.value < explorationRate)
        {
            return (FighterAction)Random.Range(0, System.Enum.GetValues(typeof(FighterAction)).Length);
        }

        float[] values = qTable[state];
        int bestIndex = 0;
        float bestValue = values[0];

        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] > bestValue)
            {
                bestValue = values[i];
                bestIndex = i;
            }
        }

        return (FighterAction)bestIndex;
    }

    private void UpdateQ(string state, FighterAction action, float reward, string nextState)
    {
        if (string.IsNullOrEmpty(state))
            return;

        EnsureStateExists(state);
        EnsureStateExists(nextState);

        float oldQ = qTable[state][(int)action];
        float maxNextQ = GetMaxQ(nextState);

        float newQ = oldQ + learningRate *
            (reward + discountFactor * maxNextQ - oldQ);

        qTable[state][(int)action] = newQ;
    }

    private float GetMaxQ(string state)
    {
        EnsureStateExists(state);

        float max = qTable[state][0];

        for (int i = 1; i < qTable[state].Length; i++)
        {
            if (qTable[state][i] > max)
                max = qTable[state][i];
        }

        return max;
    }

    private void EnsureStateExists(string state)
    {
        if (!qTable.ContainsKey(state))
        {
            qTable[state] = new float[System.Enum.GetValues(typeof(FighterAction)).Length];
        }
    }

    private string GetState(GameObject enemy)
    {
        StatScript enemyStats = enemy.GetComponent<StatScript>();

        string fighterHealth = GetHealthBucket(stats.GetHealth(), stats.GetMaxHealth());
        string enemyHealth = enemyStats != null
            ? GetHealthBucket(enemyStats.GetHealth(), enemyStats.GetMaxHealth())
            : "Unknown";

        int distance = GetDistanceTo(enemy);
        string distanceBucket;

        if (distance <= 1)
            distanceBucket = "Adjacent";
        else if (distance <= stats.GetSpeed() + 1)
            distanceBucket = "Near";
        else
            distanceBucket = "Far";

        return fighterHealth + "|" + enemyHealth + "|" + distanceBucket;
    }

    private string GetHealthBucket(float current, float maximum)
    {
        if (maximum <= 0f)
            return "Low";

        float percent = current / maximum;

        if (percent <= 0.33f)
            return "Low";

        if (percent <= 0.66f)
            return "Medium";

        return "High";
    }

    private int GetDistanceTo(GameObject other)
    {
        Vector3Int myCell = movement.GetTilemap().WorldToCell(transform.position);
        Vector3Int otherCell = movement.GetTilemap().WorldToCell(other.transform.position);

        return Mathf.Abs(myCell.x - otherCell.x) + Mathf.Abs(myCell.y - otherCell.y);
    }

    private void PrintQValues(string state)
    {
        float[] values = qTable[state];

        Debug.Log(
            name + " Q[" + state + "] " +
            "Attack=" + values[(int)FighterAction.Attack].ToString("F2") + ", " +
            "Approach=" + values[(int)FighterAction.Approach].ToString("F2") + ", " +
            "Hold=" + values[(int)FighterAction.Hold].ToString("F2")
        );
    }

    public void EndTurn()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.EndTurn(gameObject);
        }
    }
}
