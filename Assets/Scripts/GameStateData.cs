using UnityEngine;

[CreateAssetMenu(fileName = "GameStateData", menuName = "Lab 4/Game State Data")]
public class GameStateData : ScriptableObject
{
    [Header("Runtime state (shared between scenes)")]
    public int currentScore;
    public int highScore;

    [Header("Power-up tuning")]
    public float mushroomSpeedMultiplier = 1.5f;
    public float mushroomDuration = 6f;

    [Header("Audio")]
    public AudioClip backgroundMusic;
    public AudioClip powerupSpawnSound;
    public AudioClip powerupCollectSound;
    public AudioClip goombaStompSound;
    public AudioClip gameOverSound;
    public AudioClip pauseSound;
}
