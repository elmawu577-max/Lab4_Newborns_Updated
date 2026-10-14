using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LabGameManager : MonoBehaviour
{
    public static LabGameManager Instance { get; private set; }

    public event Action<int, int> ScoreChanged;
    public event Action<bool> PauseChanged;

    public GameStateData State { get; private set; }
    public bool IsPaused { get; private set; }

    private const string HighScoreKey = "Lab4HighScore";
    private AudioSource musicSource;
    private AudioSource sfxSource;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateManager()
    {
        if (Instance != null)
            return;

        GameObject managerObject = new GameObject("Lab4 Game Manager (Singleton)");
        managerObject.AddComponent<LabGameManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        State = Resources.Load<GameStateData>("GameStateData");
        if (State == null)
        {
            State = ScriptableObject.CreateInstance<GameStateData>();
            Debug.LogWarning("GameStateData asset was not found. Using temporary defaults.");
        }

        State.currentScore = 0;
        State.highScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0.45f;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.volume = 0.85f;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name.StartsWith("World-") &&
            (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)))
        {
            TogglePause();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetPaused(false);
        EnsureMusicIsPlaying();
        RuntimeSceneUI.Build(scene.name);
        ScoreChanged?.Invoke(State.currentScore, State.highScore);
    }

    public void StartNewGame()
    {
        ResetCurrentScore();
        LoadScene("LoadingScene");
    }

    public void EnterWorldOne()
    {
        LoadScene("World-1-1");
    }

    public void GoToNextWorld()
    {
        LoadScene("World-1-2");
    }

    public void GoToMainMenu()
    {
        SetPaused(false);
        State.currentScore = 0;
        LoadScene("MainMenu");
    }

    public void LoadScene(string sceneName)
    {
        SetPaused(false);
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }

    public void AddScore(int amount)
    {
        State.currentScore += amount;

        if (State.currentScore > State.highScore)
        {
            State.highScore = State.currentScore;
            PlayerPrefs.SetInt(HighScoreKey, State.highScore);
            PlayerPrefs.Save();
        }

        ScoreChanged?.Invoke(State.currentScore, State.highScore);
    }

    public void ResetCurrentScore()
    {
        State.currentScore = 0;
        ScoreChanged?.Invoke(State.currentScore, State.highScore);
    }

    public void ResetHighScore()
    {
        State.highScore = 0;
        PlayerPrefs.SetInt(HighScoreKey, 0);
        PlayerPrefs.Save();
        ScoreChanged?.Invoke(State.currentScore, State.highScore);
        RuntimeSceneUI.ShowMessage("HIGH SCORE RESET");
    }

    public void RestartCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        ResetCurrentScore();
        SetPaused(false);
        RestartMusic();
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }

    public void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    private void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        if (paused)
        {
            if (musicSource.isPlaying)
                musicSource.Pause();
            PlayOneShot(State.pauseSound);
        }
        else if (musicSource.clip != null && !musicSource.isPlaying)
        {
            musicSource.UnPause();
        }

        PauseChanged?.Invoke(IsPaused);
    }

    public void EnsureMusicIsPlaying()
    {
        if (State.backgroundMusic == null)
            return;

        if (musicSource.clip != State.backgroundMusic)
            musicSource.clip = State.backgroundMusic;

        if (!musicSource.isPlaying && !IsPaused)
            musicSource.Play();
    }

    public void RestartMusic()
    {
        if (State.backgroundMusic == null)
            return;

        musicSource.Stop();
        musicSource.clip = State.backgroundMusic;
        musicSource.Play();
    }

    public void PlayPowerupSpawn()
    {
        PlayOneShot(State.powerupSpawnSound);
    }

    public void PlayPowerupCollect()
    {
        PlayOneShot(State.powerupCollectSound);
    }

    public void PlayGoombaStomp()
    {
        PlayOneShot(State.goombaStompSound);
    }

    public void PlayGameOver()
    {
        musicSource.Stop();
        PlayOneShot(State.gameOverSound);
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }
}
