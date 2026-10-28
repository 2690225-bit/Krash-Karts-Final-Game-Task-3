
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    [Header("Player Car Prefabs")]
    public GameObject[] carPrefabs;

    [Header("Player Start Position")]
    public Transform playerSpawn;

    [Header("Race Settings")]
    public int totalLaps = 3;
    public int totalCheckpoints = 4;

    [Header("Race UI")]
    public Countdown countdown;
    public LapCounterUI lapCounter;
    public RaceTimer raceTimer;

    [HideInInspector] public bool raceFinished;

    private GameObject playerCar;
    private KartController playerKart;
    private int currentCheckpoint = -1;
    private int currentLap = 1;
    private bool raceStarted;

    private void Start()
    {
        Time.timeScale = 1f;
        raceFinished = false;
        raceStarted = false;
        currentLap = 1;
        currentCheckpoint = -1;

        if (playerSpawn == null)
        {
            Debug.LogError(
                "RaceManager: Player Spawn is not assigned on this RaceManager.",
                this);
            return;
        }

        if (carPrefabs == null || carPrefabs.Length == 0)
        {
            Debug.LogError("RaceManager: No car prefabs assigned.", this);
            return;
        }

        int selected = PlayerPrefs.GetInt("SelectedKart", 0);

        if (selected < 0 || selected >= carPrefabs.Length ||
            carPrefabs[selected] == null)
        {
            selected = -1;

            for (int i = 0; i < carPrefabs.Length; i++)
            {
                if (carPrefabs[i] != null)
                {
                    selected = i;
                    break;
                }
            }
        }

        if (selected == -1)
        {
            Debug.LogError("RaceManager: No valid kart prefab found.", this);
            return;
        }

        GameObject prefab = carPrefabs[selected];

        playerCar = Instantiate(
            prefab, playerSpawn.position, playerSpawn.rotation);

        playerCar.name = prefab.name + "_Player";

        playerKart = playerCar.GetComponentInChildren<KartController>(true);

        if (playerKart == null)
        {
            Debug.LogError(
                "RaceManager: The spawned kart has no KartController.",
                playerCar);
            Destroy(playerCar);
            playerCar = null;
            return;
        }

        playerCar.tag = "Player";
        playerKart.gameObject.tag = "Player";
        playerKart.SetFrozen(true);

        Camera cam = Camera.main;

        if (cam != null)
        {
            CameraFollow follow = cam.GetComponent<CameraFollow>();

            if (follow != null)
                follow.target = playerCar.transform;
        }

        if (lapCounter != null)
            lapCounter.SetLap(currentLap, totalLaps);

        if (raceTimer != null)
            raceTimer.ResetTimer();

        if (countdown == null)
        {
            Debug.LogError("RaceManager: Countdown is not assigned.", this);
            return;
        }

        countdown.raceManager = this;
        countdown.enabled = true;
        countdown.gameObject.SetActive(true);
        countdown.StartCountdown();

        Debug.Log("RaceManager: Kart spawned and countdown requested.", this);
    }

    // Your Countdown script should call this when the countdown finishes.
    public void StartRace()
    {
        if (raceStarted || raceFinished || playerKart == null)
        {
            Debug.LogWarning(
                "RaceManager: Cannot start; kart missing or race already started.",
                this);
            return;
        }

        raceStarted = true;
        playerKart.SetFrozen(false);

        if (raceTimer != null)
            raceTimer.StartTimer();

        Debug.Log("RACE STARTED: Kart unfrozen. WASD should now work.", this);
    }

    public void CheckpointPassed(int checkpointNumber)
    {
        if (!raceStarted || raceFinished)
            return;

        if (checkpointNumber != currentCheckpoint + 1)
            return;

        currentCheckpoint = checkpointNumber;

        if (currentCheckpoint >= totalCheckpoints - 1)
            CompleteLap();
    }

    private void CompleteLap()
    {
        currentCheckpoint = -1;

        if (currentLap >= totalLaps)
        {
            FinishRace();
            return;
        }

        currentLap++;

        if (lapCounter != null)
            lapCounter.SetLap(currentLap, totalLaps);

        Debug.Log("Lap completed: " + currentLap);
    }

    private void FinishRace()
    {
        raceFinished = true;

        if (playerKart != null)
            playerKart.SetFrozen(true);

        if (raceTimer != null)
            raceTimer.StopTimer();

        Debug.Log("RACE FINISHED!");
    }
}
