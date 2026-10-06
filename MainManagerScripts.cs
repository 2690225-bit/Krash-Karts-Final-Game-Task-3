using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject homePanel;
    public GameObject kartSelectPanel;

    [Header("Racer Name")]
    public TMP_InputField racerNameInput;

    [Header("Kart Display")]
    public GameObject[] karts;

    private int currentKart = 0;

    void Start()
    {
        homePanel.SetActive(true);
        kartSelectPanel.SetActive(false);

        ShowKart();
    }

    public void StartRaceSetup()
    {
        homePanel.SetActive(false);
        kartSelectPanel.SetActive(true);
    }

    public void NextKart()
    {
        currentKart++;

        if (currentKart >= karts.Length)
        {
            currentKart = 0;
        }

        ShowKart();
    }

    public void PreviousKart()
    {
        currentKart--;

        if (currentKart < 0)
        {
            currentKart = karts.Length - 1;
        }

        ShowKart();
    }

    void ShowKart()
    {
        for (int i = 0; i < karts.Length; i++)
        {
            karts[i].SetActive(i == currentKart);
        }
    }

    public void ConfirmSelection()
    {
        string racerName = racerNameInput.text;

        if (string.IsNullOrWhiteSpace(racerName))
        {
            racerName = "Racer";
        }

        PlayerPrefs.SetString("RacerName", racerName);
        PlayerPrefs.SetInt("SelectedKart", currentKart);

        PlayerPrefs.Save();

        SceneManager.LoadScene("RaceScene");
    }
}
