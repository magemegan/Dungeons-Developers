using UnityEngine;
using static GameManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;
    public bool IsInCombat { get; private set; }
    private List<GameObject> combatants = new List<GameObject>();
    private GameObject currentCombatant;
    private bool isStartingTurn;
    [SerializeField] private float delay = 1f;

    public List<GameObject> GetCombatants() { return combatants; }

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

        combatants.Clear();
        combatants.Add(adventurer);
        combatants.Add(enemy);

        StopMovement(adventurer);
        StopMovement(enemy);

        Debug.Log("COMBAT STARTED!");
        Debug.Log(adventurer.name + " is fighting " + enemy.name);

        StartCoroutine(StartTurn(adventurer));
    }

    private IEnumerator StartTurn(GameObject combatant)
    {
        yield return new WaitForSeconds(delay);

        if (!IsInCombat || isStartingTurn)
            yield break;

        if (combatant == null || !IsAlive(combatant))
        {
            StartCoroutine(AdvanceTurn(0));
            yield break;
        }

        AICombatBase combatScript = combatant.GetComponent<AICombatBase>();

        if (combatScript == null)
        {
            Debug.LogError(combatant.name + " does not have an AICombatBase!");

            yield break;
        }

        currentCombatant = combatant;
        isStartingTurn = true;

        Debug.Log(currentCombatant.name + "'s turn starts.");

        combatScript.StartTurn();

        isStartingTurn = false;
    }

    public void EndTurn(GameObject combatant)
    {
        // Ignore if not their turn
        if (!IsInCombat || combatant != currentCombatant)
            return;

        Debug.Log(combatant.name + " ended their turn.");

        int currentIndex = combatants.IndexOf(combatant);

        if (!CheckRemainingCombatants())
        {
            Debug.Log("Only one side is left");
            EndCombat();
            return;
        }

        StartCoroutine(AdvanceTurn(currentIndex));
    }

    private IEnumerator AdvanceTurn(int currentIndex)
    {
        yield return new WaitForSeconds(delay);

        yield return null;

        if (!IsInCombat || combatants.Count == 0)
            yield break;

        // Find the next living combatant.
        for (int i = 1; i <= combatants.Count; i++)
        {
            int nextIndex = (currentIndex + i) % combatants.Count;
            GameObject candidate = combatants[nextIndex];

            if (IsAlive(candidate))
            {
                StartCoroutine(StartTurn(candidate));
                yield break;
            }
        }

        EndCombat();
    }

    private bool IsAlive(GameObject combatant)
    {
        if (combatant == null)
            return false;

        StatScript stats = combatant.GetComponent<StatScript>();

        return stats != null && stats.GetHealth() > 0;
    }

    private bool CheckRemainingCombatants()
    {
        bool adventurersAlive = false;
        bool enemiesAlive = false;
        foreach (GameObject combatant in combatants)
        {
            if(combatant.CompareTag("Enemy"))
                enemiesAlive = true;
            else if(combatant.CompareTag("Adventurer"))
                adventurersAlive = true;

            if (adventurersAlive && enemiesAlive)
                break;
        }

        if (adventurersAlive && enemiesAlive) return true;
        else return false;
    }

    public void EndCombat()
    {
        IsInCombat = false;

        GameManager.Instance.CurrentState = GameState.Exploration;

        Debug.Log("COMBAT ENDED!");
    }



    private void StopMovement(GameObject combatant)
    {
        MovementScript movement = combatant.GetComponent<MovementScript>();

        if (movement != null)
            movement.StopMovement();
    }

    public void RemoveCombatant(GameObject combatant)
    {
        if (combatant == null)
            return;

        // Remove from combatant list
        if (combatants.Contains(combatant))
        {
            combatants.Remove(combatant);

            Debug.Log(combatant.name + " was removed from combat.");
        }

        // If their turn was happening, end it
        if (currentCombatant == combatant)
        {
            currentCombatant = null;

            if (!CheckRemainingCombatants())
            {
                EndCombat();
            }
            else
            {
                // Start next valid turn
                StartCoroutine(AdvanceTurnAfterRemoval());
            }
        }
    }

    private IEnumerator AdvanceTurnAfterRemoval()
    {
        yield return new WaitForSeconds(delay);

        yield return null;

        if (!IsInCombat || combatants.Count == 0)
            yield break;

        // Start next living combatant's turn
        foreach (GameObject combatant in combatants)
        {
            if (IsAlive(combatant))
            {
                StartCoroutine(StartTurn(combatant));
                yield break;
            }
        }

        EndCombat();
    }
}
