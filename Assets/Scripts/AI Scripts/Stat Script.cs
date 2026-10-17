using UnityEngine;

public class StatScript : MonoBehaviour
{
    [SerializeField] private bool adventurer;
    [SerializeField] private float health = 1f;
    [SerializeField] private int speed = 5; // How many tiles they can move in a turn of combat
    [SerializeField] private float perception = 5f; // How far can they see

    // Getters
    public bool GetAdventurer() { return adventurer; }
    public float GetHealth() { return health; }
    public int GetSpeed() { return speed; }
    public float GetPerceptiom() { return perception; }

    private void Awake()
    {
        // Get detector and set view distance to perception
        // Also set if adventurer or not in same script
    }
    
}
