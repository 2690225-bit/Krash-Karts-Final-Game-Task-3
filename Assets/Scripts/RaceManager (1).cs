using UnityEngine;

public class RaceManager : MonoBehaviour
{
    [Header("Player Car Prefabs")]
    public GameObject[] carPrefabs;

    [Header("Player Start Position")]
    public Transform playerSpawn;

    [Header("Race Settings")]
    public int totalLaps = 3;

    [Header("Race UI")]
    public Countdown countdown;
    public LapCounterUI lapCounter;
    public RaceTimer raceTimer;

    [Header("Checkpoint Settings")]
    public int totalCheckpoints = 4;

    [HideInInspector]
    public bool raceFinished = false;

    private GameObject playerCar;

    private int currentCheckpoint = -1;
    private int currentLap = 1;

    void Start()
    {
        SpawnPlayer();

        // Freeze the player before countdown
        KartController kart =
            playerCar.GetComponent<KartController>();

        if (kart != null)
        {
            kart.SetFrozen(true);
        }

        // Set initial lap display
        if (lapCounter != null)
        {
            lapCounter.SetLap(
                currentLap,
                totalLaps
            );
        }

        // Start countdown
        if (countdown != null)
        {
            countdown.raceManager = this;
            countdown.StartCountdown();
        }
        else
        {
            Debug.LogError(
                "Countdown is not assigned to RaceManager!"
            );
        }
    }

    void SpawnPlayer()
    {
        int selectedKart =
            PlayerPrefs.GetInt("SelectedKart", 0);

        if (carPrefabs == null ||
            carPrefabs.Length == 0)
        {
            Debug.LogError(
                "No car prefabs assigned to RaceManager!"
            );
            return;
        }

        if (playerSpawn == null)
        {
            Debug.LogError(
                "PlayerSpawnPoint is not assigned!"
            );
            return;
        }

        if (selectedKart < 0 ||
            selectedKart >= carPrefabs.Length)
        {
            selectedKart = 0;
        }

        playerCar = Instantiate(
            carPrefabs[selectedKart],
            playerSpawn.position,
            playerSpawn.rotation
        );

        Debug.Log(
            "Player spawned: " +
            playerCar.name
        );

        // Make sure the spawned car is the Player
        playerCar.tag = "Player";

        // Camera
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
        {
            CameraFollow cameraFollow =
                mainCamera.GetComponent<CameraFollow>();

            if (cameraFollow != null)
            {
                cameraFollow.target =
                    playerCar.transform;
            }
            else
            {
                Debug.LogError(
                    "CameraFollow is missing from Main Camera!"
                );
            }
        }
        else
        {
            Debug.LogError(
                "No Main Camera found!"
            );
        }
    }

    public void StartRace()
    {
        Debug.Log("RACE STARTED!");

        if (playerCar != null)
        {
            KartController kart =
                playerCar.GetComponent<KartController>();

            if (kart != null)
            {
                kart.SetFrozen(false);
            }
        }

        if (raceTimer != null)
        {
            raceTimer.StartTimer();
        }
    }

    public void CheckpointPassed(int checkpointNumber)
    {
        if (raceFinished)
            return;

        // Checkpoints must be passed in order
        if (checkpointNumber != currentCheckpoint + 1)
        {
            return;
        }

        currentCheckpoint = checkpointNumber;

        Debug.Log(
            "Checkpoint passed: " +
            checkpointNumber
        );

        // Last checkpoint reached
        if (currentCheckpoint >= totalCheckpoints - 1)
        {
            CompleteLap();
        }
    }

    void CompleteLap()
    {
        currentCheckpoint = -1;

        if (currentLap >= totalLaps)
        {
            FinishRace();
            return;
        }

        currentLap++;

        Debug.Log(
            "Lap completed! Current lap: " +
            currentLap
        );

        if (lapCounter != null)
        {
            lapCounter.SetLap(
                currentLap,
                totalLaps
            );
        }
    }

    void FinishRace()
    {
        raceFinished = true;

        Debug.Log("RACE FINISHED!");

        if (playerCar != null)
        {
            KartController kart =
                playerCar.GetComponent<KartController>();

            if (kart != null)
            {
                kart.SetFrozen(true);
            }
        }

        if (raceTimer != null)
        {
            raceTimer.StopTimer();
        }
    }
}
