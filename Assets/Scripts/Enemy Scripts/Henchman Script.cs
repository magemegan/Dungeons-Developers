using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HenchmanScript : MonoBehaviour, AICombatBase
{
    // i don't know for sure that i need this but it's here
    private enum HenchmanState
    {
        FindingEnemy,
        FollowingEnemy,
        Protecting,
        Panic
    }

    // References
    private StatScript stats;
    private MovementScript movement;
    private DetectorScript detector;

    [SerializeField] private Tilemap tilemap;

    [SerializeField] private GameObject targetEnemy;
    [SerializeField] private float findDistance = 12f;
    [SerializeField] private float protectDistance = 2f;

    private void Awake()
    {
        stats = GetComponent<StatScript>();
        movement = GetComponent<MovementScript>();
        detector = GetComponent<DetectorScript>();
    }

    void Start()
    {
        FindStrongestEnemy();
    }

    void Update()
    {
        
    }

    /* 
     NEED:
        - henchman actions enum
        - henchman states enum? alone, with enemy?
            - if with enemy: have to continuously follow the enemy's movement
        - if adventurer is within protectDistance, start combat
        - find all enemies in room, find enemy with highest health
        - find closest enemy
        - PANIC
     */

    // i'm going to steal detector.CheckForEnemies -- henchman checks for adventurers, if adventurer within protectDistance, start combat

    private GameObject FindStrongestEnemy()
    {
        GameObject strongestEnemy = null;
        float strongestHealth = 0;
        List<GameObject> equalStrongest = new List<GameObject>();

        List<GameObject> enemies = FindEnemies();

        foreach(GameObject enemy in enemies)
        {
            float health = enemy.GetComponent<StatScript>().GetHealth();
            if(enemy.GetComponent<HenchmanScript>())
            {
                continue;
            }
            if(health > strongestHealth)
            {
                strongestHealth = health;
                strongestEnemy = enemy;
                equalStrongest.Clear();
                equalStrongest.Add(enemy);
            } else if(health == strongestHealth)
            {
                equalStrongest.Add(enemy);
            }
        }

        if(equalStrongest.Count > 1)
        {
            //if multiple enemies have the same (highest) health, go to the closest one

            int ind = -1;
            int shortest = 15;
            Vector3Int myCell = tilemap.WorldToCell(transform.position);

            //for loop returns the index of the shortest distance
            for (int i = 0; i < equalStrongest.Count; i++)
            {
                Vector3Int enemyCell = tilemap.WorldToCell(equalStrongest[i].transform.position);

                int distance = Mathf.Abs(myCell.x - enemyCell.x) + Mathf.Abs(myCell.y - enemyCell.y);

                if(distance < shortest)
                {
                    ind = i;
                    shortest = distance;
                }
            }

            strongestEnemy = equalStrongest[ind];
        }

        movement.SetTarget(strongestEnemy.transform.position);
        targetEnemy = strongestEnemy;
        Debug.Log("strongest enemy: " + strongestEnemy.name);
        //movement.MoveTowardsCombatTarget(strongestEnemy.transform.position);
        return strongestEnemy;
    }

    private List<GameObject> FindEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(tag);

        Vector3Int myCell = tilemap.WorldToCell(transform.position);

        List<GameObject> enemiesInRange = new List<GameObject>();

        foreach (GameObject enemy in enemies)
        {
            Vector3Int enemyCell = tilemap.WorldToCell(enemy.transform.position);

            int distance = Mathf.Abs(myCell.x - enemyCell.x) + Mathf.Abs(myCell.y - enemyCell.y);

            if(distance <= findDistance)
            {
                enemiesInRange.Add(enemy);
            }
        }

        if(enemiesInRange.Count == 0)
        {
            // PANIC !!!!
        }

        return enemiesInRange;
    }



    private void Panic()
    {
        // "move in direction that increases distance between itself and adventurers, with 20% chance of moving in a suboptimal direction"
    }

    public void StartTurn()
    {
        Debug.Log("Henchman turn starts!");

        if(targetEnemy.GetComponent<StatScript>().GetHealth() < targetEnemy.GetComponent<StatScript>().GetMaxHealth())
        {
            //check to make sure this is still the strongest enemy -- if not, jump ship to the new strongest
            FindStrongestEnemy();
            movement.MoveTowardsCombatTarget(targetEnemy.transform.position);
        }
    }

    public void EndTurn()
    {
        CombatManager.Instance.EndTurn(gameObject);
    }
}
