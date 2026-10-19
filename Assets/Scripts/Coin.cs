using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] public int gold = 1;
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Adventurer"))
        {
            Debug.Log(collision.gameObject.name);
            Destroy(gameObject);
            GuildManager.Instance.CurrentGold += gold;
        }
    }
}
