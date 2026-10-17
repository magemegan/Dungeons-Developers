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
        adventurer = stats.GetAdventurer();
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
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

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
}
