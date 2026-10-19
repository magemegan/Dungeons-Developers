using UnityEngine;

[System.Serializable]
public class LootData : MonoBehaviour
{
    public GameObject lootObject;
    public Vector3Int gridPosition;
    public int value;
    
    [SerializeField] int minValue = 5;
    [SerializeField] int maxValue = 10;
    
    public int getValue() { return value; }

    void Awake()
    {
        int randomValue = Random.Range(minValue, maxValue);
        value = randomValue;
    }
}
