using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelProgression : MonoBehaviour
{
    [Header("Scene Names")]
    public string level1Scene = "Level1";
    public string level2Scene = "Level2";

    [Header("Result Message")]
    public TMP_Text resultText;
    public float messageDuration = 4f;

    private bool progressionStarted;

    public void FinishRace(int playerPosition)
    {
        if (progressionStarted)
            return;

        progressionStarted = true;

        if (playerPosition >= 1 && playerPosition <= 3)
        {
            PlayerPrefs.SetInt("Level2Unlocked", 1);
            PlayerPrefs.Save();

            if (resultText != null)
                resultText.text = "YOU MADE THE TOP 3! Level 2 unlocked!";

            StartCoroutine(LoadLevel2());
        }
        else
        {
            if (resultText != null)
            {
                resultText.text =
                    "Oopsie! You didn't make the top 3. " +
                    "You need to finish in the top 3 to unlock the next level. " +
                    "Better luck next time!";
            }

            StartCoroutine(RestartLevel1());
        }
    }

    private IEnumerator LoadLevel2()
    {
        yield return new WaitForSecondsRealtime(messageDuration);
        SceneManager.LoadScene(level2Scene);
    }

    private IEnumerator RestartLevel1()
    {
        yield return new WaitForSecondsRealtime(messageDuration);
        SceneManager.LoadScene(level1Scene);
    }
}