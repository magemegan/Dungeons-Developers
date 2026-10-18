using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using System.Collections;


public class ExampleAdventurer : MonoBehaviour, AICombatBase
{
    private bool turnIsActive = false;
    private StatScript stats;

    [SerializeField] private float attackDamage = 3f;

    void Awake()
    {
        stats = GetComponent<StatScript>();
    }

    public void StartTurn()
    {
        turnIsActive = true;

        Debug.Log(name + " started their turn");

        DecideAction();
    }

    public void DecideAction()
    {
        Debug.Log("Base Adventurer is deciding their action");

        float health = stats.GetHealth();
        float maxHealth = stats.GetMaxHealth();

        float healthPercent = (float)health / maxHealth;

        // Always just attack rn
        StartCoroutine(Attack());

        EndTurn();
    }

    private IEnumerator Attack()
    {
        Debug.Log(gameObject.name + " attacks!");

        // Attack closest enemy
        DetectorScript detector = GetComponent<DetectorScript>();

        GameObject enemy = detector.GetClosestOpponent();

        if(enemy == null)
        {
            Debug.LogWarning(name + " could not find an enemy to attack.");
            yield break;
        }

        StatScript enemyStats = enemy.GetComponent<StatScript>();

        if (enemyStats != null)
            enemyStats.TakeDamage(attackDamage);
    }

    public void EndTurn()
    {
        CombatManager.Instance.EndTurn(gameObject);
    }
}
