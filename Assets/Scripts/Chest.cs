using UnityEngine;

public class Chest : MonoBehaviour
{
    private LootData lootData;

    void Start()
    {
        lootData = GetComponent<LootData>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Adventurer"))
        {
            Destroy(gameObject);

            for (int i = 0; i < lootData.value; i++)
            {
                Instantiate(lootData.gameObject, transform.position, Quaternion.identity);
            }
        }
    }
}
