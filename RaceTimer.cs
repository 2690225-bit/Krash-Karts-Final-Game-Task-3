using UnityEngine;
using TMPro;

public class RaceTimer : MonoBehaviour
{
    public TMP_Text timerText;

    private float elapsedTime = 0f;
    private bool timerRunning = true;

    private RaceManager raceManager;

    void Start()
    {
        raceManager = FindAnyObjectByType<RaceManager>();
    }

    void Update()
    {
        // Stops the timer when the race is finished
        if (raceManager != null && raceManager.raceFinished)
        {
            StopTimer();
            return;
        }

        // Means Don't update the timer once it has stopped
        if (!timerRunning)
            return;

        elapsedTime += Time.deltaTime;

        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 100f) % 100f);

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

    public void StopTimer()
    {
        if (!timerRunning)
            return;

        timerRunning = false;

        // Displays the final time
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 100f) % 100f);

        if (timerText != null)
        {
            timerText.text = string.Format(
                "{0:00}:{1:00}:{2:00}",
                minutes,
                seconds,
                milliseconds
            );
        }

        Debug.Log("TIMER STOPPED!");
    }
}