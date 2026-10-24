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
        Panic
    }

    private HenchmanState state;

    // References
    private StatScript stats;
    private MovementScript movement;
    private DetectorScript detector;

    [SerializeField] private Tilemap tilemap;

    [SerializeField] private GameObject targetEnemy;
    [SerializeField] private float findDistance = 12f;
    [SerializeField] private float protectDistance = 2f;
    [SerializeField] private float attackDamage = 1f;

    //private int panicField = 2;
    // ^ panicking henchman will pick a cell and go to it as long as it is not within panicField spaces of the adventurer

    private GameObject targetPlayer;

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
            - if with enemy: have to continuously follow the enemy's movement
        - if adventurer is within protectDistance, start combat
        - find all enemies in room, find enemy with highest health
        - find closest enemy
        - PANIC
        - check if adjacent to target enemy
            - if not -- move to adjacent position to enemy
     */

    // i'm going to steal detector.CheckForEnemies -- henchman checks for adventurers, if adventurer within protectDistance, start combat

    private GameObject FindStrongestEnemy()
    {
        GameObject strongestEnemy = null;
        float strongestHealth = 0;
        List<GameObject> equalStrongest = new List<GameObject>();

        List<GameObject> enemies = FindEnemies();
        //Debug.Log(enemies.Count + " enemy, it is " + enemies[0].name);

        if (enemies == null || enemies.Count == 0)
        {
            stats.SetSpeed(3);
            state = HenchmanState.Panic;
            targetEnemy = null;
            //Panic();
            Debug.Log("There are no enemies. state: " + state);
            return null;
        }

        stats.SetSpeed(5);

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
        movement.MoveTowardsTarget(strongestEnemy.transform.position);
        return strongestEnemy;
    }

    private List<GameObject> FindEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(tag);

        if(enemies.Length == 0)
        {
            return null;
        }

        Vector3Int myCell = tilemap.WorldToCell(transform.position);

        List<GameObject> enemiesInRange = new List<GameObject>();

        foreach (GameObject enemy in enemies)
        {
            Vector3Int enemyCell = tilemap.WorldToCell(enemy.transform.position);

            int distance = Mathf.Abs(myCell.x - enemyCell.x) + Mathf.Abs(myCell.y - enemyCell.y);
            if(enemy.activeInHierarchy)
            {
                if(enemy.GetComponent<HenchmanScript>())
                {
                    continue;
                }
                if(distance <= findDistance)
                {
                    enemiesInRange.Add(enemy);
                }
            }
            
        }

        return enemiesInRange;
    }



    //private void Panic()
    //{
    //    Debug.Log("I'M FREAKING OUT!!!!!!");
    //    // "move in direction that increases distance between itself and adventurers, with 20% chance of moving in a suboptimal direction"
    //}

    private bool IsAdjacent(GameObject target)
    {
        Vector3Int myCell = tilemap.WorldToCell(transform.position);
        Vector3Int targetCell = tilemap.WorldToCell(target.transform.position);

        int distance = movement.GetDistance(myCell, targetCell);
        if(distance == 1)
        {
            return true;
        } else
        {
            return false;
        }
    }

    public void StartTurn()
    {
        Debug.Log("Henchman turn starts! state: " + state);
        if(state != HenchmanState.Panic)
        {
            if(targetEnemy.GetComponent<StatScript>().GetHealth() < targetEnemy.GetComponent<StatScript>().GetMaxHealth())
            {
                //check to make sure this is still the strongest enemy -- if not, jump ship to the new strongest
                FindStrongestEnemy();
                if(targetEnemy != null)
                {
                    if (!IsAdjacent(targetEnemy))
                    {
                        movement.MoveTowardsCombatTarget(targetEnemy.transform.position);
                        state = HenchmanState.FindingEnemy;
                    }
                    else
                    {
                        state = HenchmanState.FollowingEnemy;
                    }
                }
            }

            if(state == HenchmanState.FollowingEnemy)
            {
                targetPlayer = targetEnemy.GetComponent<DetectorScript>().GetClosestOpponent();
                Attack();
            }

            EndTurn();
        } else if(state == HenchmanState.Panic)
        {
            //i'm freaking out!!!!!!
            targetPlayer = detector.GetClosestOpponent();
            movement.Panic(targetPlayer.transform.position, stats.GetSpeed());
            EndTurn();
        }
        
    }

    private void Attack()
    {
        Debug.Log("Henchman is trying to attack! state: " + state);
        if (targetPlayer == null)
        {
            targetPlayer = targetEnemy.GetComponent<DetectorScript>().GetClosestOpponent();
        }

        if (targetPlayer == null)
        {
            return;
        }

        if (IsAdjacent(targetPlayer))
        {
            StatScript playerStats =
                targetPlayer.GetComponent<StatScript>();

            if (playerStats != null)
            {
                playerStats.TakeDamage(attackDamage);
            }
        }
        else
        {
            // If too far go to adventurer
            movement.MoveTowardsCombatTarget(targetPlayer.transform.position);
        }
    }

    public void EndTurn()
    {
        CombatManager.Instance.EndTurn(gameObject);
    }
}
