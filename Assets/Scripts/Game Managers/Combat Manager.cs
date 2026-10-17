using UnityEngine;
using static GameManager;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    public bool IsInCombat { get; set; }

    private void Awake()
    {
        Instance = this;
        IsInCombat = false;
    }

    public void StartCombat(GameObject adventurer, GameObject enemy)
    {
        if (IsInCombat)
            return;

        IsInCombat = true;
        GameManager.Instance.CurrentState = GameState.Combat;

        MovementScript adventurerMovement = adventurer.GetComponent<MovementScript>();
        MovementScript enemyMovement = enemy.GetComponent<MovementScript>();

        if (adventurerMovement != null)
            adventurerMovement.StopMovement();

        if (enemyMovement != null)
            enemyMovement.StopMovement();

        Debug.Log("COMBAT STARTED!");
        Debug.Log(adventurer.name + " is fighting " + enemy.name);
    }

    public void EndCombat()
    {
        IsInCombat = false;

        GameManager.Instance.CurrentState = GameState.Exploration;

        Debug.Log("COMBAT ENDED!");
    }
}
