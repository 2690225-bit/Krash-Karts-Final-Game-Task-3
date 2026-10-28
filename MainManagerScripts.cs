
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

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

    [Header("Scene Settings")]
    public string raceSceneName = "RaceScene";

    private int currentKart = 0;
    private bool isLoadingRace = false;

    private TMP_Text namePlaceholder;
    private string originalPlaceholderText;

    private void Start()
    {
        isLoadingRace = false;

        if (startButton != null)
            startButton.SetActive(true);

        if (kartSelectPanel != null)
            kartSelectPanel.SetActive(false);

        if (kartImages == null)
            kartImages = new GameObject[0];

        // Set up the racer-name placeholder.
        SetupRacerNameInput();

        // Restore the last selected kart.
        if (kartImages.Length > 0)
        {
            currentKart = PlayerPrefs.GetInt("SelectedKart", 0);

            if (currentKart < 0 || currentKart >= kartImages.Length)
                currentKart = 0;
        }

        ShowKart();
    }

    private void SetupRacerNameInput()
    {
        if (racerNameInput == null)
            return;

        namePlaceholder = racerNameInput.placeholder as TMP_Text;

        if (namePlaceholder == null)
            return;

        originalPlaceholderText = namePlaceholder.text;

        // Remove previous listeners to prevent duplicates.
        racerNameInput.onSelect.RemoveListener(OnNameInputSelected);
        racerNameInput.onDeselect.RemoveListener(OnNameInputDeselected);

        racerNameInput.onSelect.AddListener(OnNameInputSelected);
        racerNameInput.onDeselect.AddListener(OnNameInputDeselected);

        // Keep the placeholder hidden if a name already exists.
        UpdatePlaceholder();
    }

    private void OnNameInputSelected(string value)
    {
        if (namePlaceholder != null)
            namePlaceholder.text = "";
    }

    private void OnNameInputDeselected(string value)
    {
        UpdatePlaceholder();
    }

    private void UpdatePlaceholder()
    {
        if (racerNameInput == null || namePlaceholder == null)
            return;

        if (racerNameInput.isFocused ||
            !string.IsNullOrEmpty(racerNameInput.text))
        {
            namePlaceholder.text = "";
        }
        else
        {
            namePlaceholder.text = originalPlaceholderText;
        }
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
        if (kartImages == null || kartImages.Length == 0)
            return;

        currentKart = (currentKart + 1) % kartImages.Length;
        ShowKart();
    }

    public void PreviousKart()
    {
        if (kartImages == null || kartImages.Length == 0)
            return;

        currentKart--;

        if (currentKart < 0)
            currentKart = kartImages.Length - 1;

        ShowKart();
    }

    private void ShowKart()
    {
        if (kartImages == null)
            return;

        for (int i = 0; i < kartImages.Length; i++)
        {
            if (kartImages[i] != null)
                kartImages[i].SetActive(i == currentKart);
        }
    }

    public void ConfirmSelection()
    {
        if (isLoadingRace)
            return;

        if (string.IsNullOrWhiteSpace(raceSceneName))
        {
            Debug.LogError(
                "MainMenuManager: Race scene name is empty.",
                this
            );
            return;
        }

        string racerName = "Racer";

        if (racerNameInput != null &&
            !string.IsNullOrWhiteSpace(racerNameInput.text))
        {
            racerName = racerNameInput.text.Trim();
        }

        PlayerPrefs.SetString("RacerName", racerName);
        PlayerPrefs.SetInt("SelectedKart", currentKart);
        PlayerPrefs.Save();

        if (Application.CanStreamedLevelBeLoaded(raceSceneName))
        {
            isLoadingRace = true;
            SceneManager.LoadScene(raceSceneName);
        }
        else
        {
            Debug.LogError(
                "MainMenuManager: Cannot load '" + raceSceneName +
                "'. Check that RaceScene is included in Build Profiles.",
                this
            );
        }
    }

    private void OnDestroy()
    {
        if (racerNameInput != null)
        {
            racerNameInput.onSelect.RemoveListener(OnNameInputSelected);
            racerNameInput.onDeselect.RemoveListener(OnNameInputDeselected);
        }
    }
}
