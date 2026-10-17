using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Exploration,
        Combat
    }

    public static GameManager Instance;

    public GameState CurrentState { get; set; }

    private void Awake()
    {
        Instance = this;
        CurrentState = GameState.Exploration;
    }

    public void StartCombat()
    {
        CurrentState = GameState.Combat;
        Debug.Log("Combat Started!");
    }

    public void EndCombat()
    {
        CurrentState = GameState.Exploration;
        Debug.Log("Combat Ended!");
    }
}
