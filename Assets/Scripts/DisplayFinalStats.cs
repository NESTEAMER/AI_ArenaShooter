using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class DisplayFinalStats : MonoBehaviour
{
    public TextMeshProUGUI finalStatsTextElement;
    private AudioSource audioSource;
    public AudioClip gunShot;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(gunShot);
        StringBuilder statsBuilder = new StringBuilder();
        statsBuilder.AppendLine("--- FINAL SCORE ---");
        statsBuilder.AppendLine($"Total Money Earned: ${GameHandler.finalRunTotalMoney:N0}");
        statsBuilder.AppendLine($"Reached Day: {GameHandler.finalDayReached}");
        statsBuilder.AppendLine($"Reached Round: {GameHandler.finalRoundReached}");

        if (GameHandler.finalActivePowerUpNames.Count > 0)
        {
            statsBuilder.AppendLine("\nChosen Power-ups:");
            foreach (string powerUpName in GameHandler.finalActivePowerUpNames)
            {
                statsBuilder.AppendLine($"- {powerUpName}");
            }
        }
        else
        {
            statsBuilder.AppendLine("\nNo power-ups acquired.");
        }

        finalStatsTextElement.text = statsBuilder.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
