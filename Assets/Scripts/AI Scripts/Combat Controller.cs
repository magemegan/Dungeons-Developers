using UnityEngine;

public class CombatController : MonoBehaviour
{
    [SerializeField] private int movementRange = 5;

    private int remainingMovement;

    public bool HasActed { get; private set; }

    public void StartTurn()
    {
        remainingMovement = movementRange;
        HasActed = false;

        Debug.Log(name + " started their turn.");
    }

    public void EndTurn()
    {
        HasActed = true;

        // Tell combat manager turn has ended
    }
}
