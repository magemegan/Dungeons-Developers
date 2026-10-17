using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public Grid dungeonGrid;
    public GameObject chestPrefab;

    public List<LootData> lootList = new List<LootData>();

    void Start()
    {
        
    }

    public void SpawnChest(Vector3Int cell, int value)
    {
        Vector3 worldPosition = dungeonGrid.GetCellCenterWorld(cell);
        
        GameObject chest = Instantiate(chestPrefab, worldPosition, Quaternion.identity);
        
        LootData lootData = new LootData();
        lootData.lootObject = chest;
        lootData.gridPosition = cell;
        lootData.value = value;
        
        lootList.Add(lootData);
    }
}
