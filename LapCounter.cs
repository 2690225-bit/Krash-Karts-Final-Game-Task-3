
using UnityEngine;
using TMPro;

public class LapCounterUI : MonoBehaviour
{
    public TMP_Text lapText;

    public void SetLap(int currentLap, int totalLaps)
    {
        if (lapText == null)
        {
            Debug.LogError(
                "LapCounterUI: LapText is not assigned!"
            );
            return;
        }

        lapText.text =
            "LAP " + currentLap + "/" + totalLaps;
    }
}