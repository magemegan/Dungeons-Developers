using System;
using TMPro;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text coinsText;


    private void Update()
    {
        // I will change this PLEASE forgive me D:
        coinsText.text = GuildManager.Instance.CurrentGold.ToString();
    }
}
