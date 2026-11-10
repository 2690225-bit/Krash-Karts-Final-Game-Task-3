using UnityEngine;
using TMPro;

public class RaceTimer : MonoBehaviour
{
    public TMP_Text timerText;

    private float elapsedTime = 0f;
    private bool timerRunning = false;

    public void StartTimer()
    {
        elapsedTime = 0f;
        timerRunning = true;
    }

    public void StopTimer()
    {
        timerRunning = false;
        UpdateTimerText();
    }

    void Update()
    {
        if (!timerRunning)
            return;

        elapsedTime += Time.deltaTime;

        UpdateTimerText();
    }

    void UpdateTimerText()
    {
        int minutes =
            Mathf.FloorToInt(elapsedTime / 60f);

        int seconds =
            Mathf.FloorToInt(elapsedTime % 60f);

        int milliseconds =
            Mathf.FloorToInt(
                (elapsedTime * 100f) % 100f
            );

        if (timerText != null)
        {
            timerText.text = string.Format(
                "{0:00}:{1:00}:{2:00}",
                minutes,
                seconds,
                milliseconds
            );
        }
    }
}
