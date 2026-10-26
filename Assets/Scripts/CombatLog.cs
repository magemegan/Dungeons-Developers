using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CombatLog : MonoBehaviour
{
    public static CombatLog Instance { get; private set; }

    [SerializeField] private TMP_Text logText;
    [SerializeField] private int maxMessages = 6;

    private readonly List<string> messages = new List<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        UpdateDisplay();
    }

    public void Log(string message)
    {
        messages.Add(message);

        while (messages.Count > maxMessages)
        {
            messages.RemoveAt(0);
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (logText != null)
        {
            logText.text = string.Join("\n", messages);
        }
    }
}