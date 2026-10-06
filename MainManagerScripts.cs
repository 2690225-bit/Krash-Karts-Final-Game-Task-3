using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject startButton;

    [Header("Kart Selection")]
    public GameObject kartSelectPanel;

    [Header("Racer Name")]
    public TMP_InputField racerNameInput;

    [Header("Kart Images")]
    public GameObject[] kartImages;

    private int currentKart = 0;

    void Start()
    {
        if (startButton != null)
            startButton.SetActive(true);

        if (kartSelectPanel != null)
            kartSelectPanel.SetActive(false);

        ShowKart();
    }

    public void StartRaceSetup()
    {
        if (startButton != null)
            startButton.SetActive(false);

        if (kartSelectPanel != null)
            kartSelectPanel.SetActive(true);

        ShowKart();
    }

    public void NextKart()
    {
        if (kartImages.Length == 0)
            return;

        currentKart++;

        if (currentKart >= kartImages.Length)
            currentKart = 0;

        ShowKart();
    }

    public void PreviousKart()
    {
        if (kartImages.Length == 0)
            return;

        currentKart--;

        if (currentKart < 0)
            currentKart = kartImages.Length - 1;

        ShowKart();
    }

    private void ShowKart()
    {
        for (int i = 0; i < kartImages.Length; i++)
        {
            if (kartImages[i] != null)
                kartImages[i].SetActive(i == currentKart);
        }
    }

    public void ConfirmSelection()
    {
        string racerName = "Racer";

        if (racerNameInput != null &&
            !string.IsNullOrWhiteSpace(racerNameInput.text))
        {
            racerName = racerNameInput.text;
        }

        PlayerPrefs.SetString("RacerName", racerName);
        PlayerPrefs.SetInt("SelectedKart", currentKart);
        PlayerPrefs.Save();

        SceneManager.LoadScene("RaceScene");
    }
}
