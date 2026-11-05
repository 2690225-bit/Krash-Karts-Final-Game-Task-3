
using UnityEngine;
using TMPro;

public class RaceTimer : MonoBehaviour
{
    public TMP_Text timerText;

    private float elapsedTime;
    private bool isRunning;

    private void Start()
    {
        UpdateDisplay();
    }

    private void Update()
    {
        if (!isRunning)
            return;

        elapsedTime += Time.deltaTime;
        UpdateDisplay();
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        isRunning = false;
        UpdateDisplay();
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    private void UpdateDisplay()
    {
        if (timerText == null)
            return;

        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds =
            Mathf.FloorToInt((elapsedTime * 100f) % 100f);

        timerText.text =
            $"{minutes:00}:{seconds:00}:{milliseconds:00}";
    }
}
