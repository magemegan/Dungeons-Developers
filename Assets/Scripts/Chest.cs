using UnityEngine;

public class Chest : MonoBehaviour
{
    private LootData lootData;

    void Start()
    {
        lootData = GetComponent<LootData>();
        if (lootData == null)
        {
            Debug.LogError("LootData component is missing!", this);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Adventurer"))
        {
            if (lootData == null) return;
            
            int coinCount = lootData.value;
            float radius = 1.5f;

            for (int i = 0; i < coinCount; i++)
            {
                float angle = i * (360f / coinCount);

                float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
                float y = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;

                Vector3 spawnPosition = transform.position
                                        + new Vector3(x, y, 0);

                Instantiate(
                    lootData.lootObject,
                    spawnPosition,
                    Quaternion.identity
                );
            }
            
            Destroy(gameObject);

        }
    }
}
