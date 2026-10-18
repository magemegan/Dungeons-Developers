using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static GameManager;

public class DetectorScript : MonoBehaviour
{
    private bool adventurer;
    [SerializeField] private Tilemap tilemap;
    private float combatDistance;

    private StatScript stats;

    private void Awake()
    {
        stats = GetComponent<StatScript>();
        combatDistance = stats.GetPerceptiom();

        if (gameObject.CompareTag("Adventurer"))
            adventurer = true;
        else if (gameObject.CompareTag("Enemy"))
            adventurer = false;
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Exploration)
            return;

        if (adventurer)
        {
            CheckForEnemies();
        } else
        {
            CheckForAdventurers();
        }
    }

    public void CheckForEnemies()
    {
        string tag;

        if (adventurer)
            tag = "Enemy";
        else
            tag = "Adventurer";

        GameObject[] enemies = GameObject.FindGameObjectsWithTag(tag);

        Vector3Int myCell = tilemap.WorldToCell(transform.position);

        foreach (GameObject enemy in enemies)
        {
            Vector3Int enemyCell = tilemap.WorldToCell(enemy.transform.position);

            int distance = Mathf.Abs(myCell.x - enemyCell.x) + Mathf.Abs(myCell.y - enemyCell.y);

            if (distance <= combatDistance)
            {
                CombatManager.Instance.StartCombat(gameObject, enemy);

                return;
            }
        }
    }

    private void CheckForAdventurers()
    {

    }

    public GameObject GetClosestOpponent()
    {
        List<GameObject> combatants = CombatManager.Instance.GetCombatants();

        Vector3Int myCell = tilemap.WorldToCell(transform.position);

        GameObject closestEnemy = null;
        int closestDistance = int.MaxValue;

        StatScript myStats = GetComponent<StatScript>();
        if (myStats == null)
            return null;

        foreach (GameObject combatant in combatants)
        {
            GameObject other = combatant.gameObject;
            StatScript stats = other.GetComponent<StatScript>();
            if (stats == null)
                continue;

            // Adventurers look for enemies
            if (myStats.GetAdventurer() && stats.GetAdventurer())
                continue;

            // Enemies look for adventurers
            if (!myStats.GetAdventurer() && !stats.GetAdventurer())
                continue;

            Vector3Int enemyCell = tilemap.WorldToCell(other.transform.position);

            int distance = Mathf.Abs(myCell.x - enemyCell.x) + Mathf.Abs(myCell.y - enemyCell.y);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = other;
            }
        }

        return closestEnemy;
    }
}
