using UnityEngine;

public class GuildManager : MonoBehaviour
{
    public static GuildManager Instance{ get; private set; }

    // after the dungeon is cleared 
    public int TotalGold { get; set; }
    
    // during the dungeon adventures
    public int CurrentGold { get; set; }    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Deposit(int amount)
    {
        if (amount <= 0)
            return;

        CurrentGold += amount;

        Debug.Log("Gold collected this dungeon: " + CurrentGold);
    }

    public void ClearDungeon()
    {
        TotalGold += CurrentGold;
        CurrentGold = 0;

        Debug.Log("Total guild gold: " + TotalGold);
    }
}
