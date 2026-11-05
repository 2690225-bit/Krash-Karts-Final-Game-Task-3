using UnityEngine;

public class RacerProgress : MonoBehaviour
{
    public string racerName;
    public bool isPlayer;

    [HideInInspector] public int currentLap = 1;
    [HideInInspector] public int nextCheckpoint = 0;
    [HideInInspector] public bool hasFinished;
    [HideInInspector] public int finishingPosition;
    [HideInInspector] public float progressScore;
}