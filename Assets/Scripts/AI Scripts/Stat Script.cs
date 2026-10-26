using UnityEngine;
using System.Collections.Generic;

public class StatScript : MonoBehaviour
{
    private bool adventurer;
    private bool inCombat = false;
    [SerializeField] private float maxHealth = 1f;
    private float health;
    [SerializeField] private int speed = 5; // How many tiles they can move in a turn of combat
    [SerializeField] private float perception = 5f; // How far can they see

    public List<GameObject> inventory = new List<GameObject>();

    // Getters
    public bool GetAdventurer() { return adventurer; }
    public float GetHealth() { return health; }
    public float GetMaxHealth() { return maxHealth; }
    public int GetSpeed() { return speed; }
    public void SetSpeed(int newSpeed)
    {
        speed = newSpeed;
    }
    public float GetPerceptiom() { return perception; }
    public List<GameObject> GetInventory() { return inventory; }

    void Awake()
    {
        health = maxHealth;

        if (gameObject.CompareTag("Adventurer"))
            adventurer = true;
        else if (gameObject.CompareTag("Enemy"))
            adventurer = false;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        Debug.Log(gameObject.name + " took " + damage + " damage. Health: " + health + "/" + maxHealth);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has died!");

        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.RemoveCombatant(gameObject);
        }

        gameObject.SetActive(false);
    }
}
