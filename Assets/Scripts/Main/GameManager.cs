
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Timer")]
    [SerializeField] private float defaultTimerSeconds = 600f;

    private const string SavedTimeKey = "DetectivePuzzle_RemainingTime";

    private float remainingTime;
    private bool timerRunning;
    private bool timeUpHandled;

    public float RemainingTime => remainingTime;
    public bool IsTimerRunning => timerRunning;

    public event Action<float> OnTimerUpdated;
    public event Action OnTimeUp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        remainingTime = 0f;
        timerRunning = false;
        timeUpHandled = false;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (!timerRunning)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            timerRunning = false;

            SaveRemainingTime();
            OnTimerUpdated?.Invoke(remainingTime);

            if (!timeUpHandled)
            {
                timeUpHandled = true;
                OnTimeUp?.Invoke();

                if (SceneManager.GetActiveScene().name != "MainStory")
                {
                    SceneManager.LoadScene("MainStory");
                }
            }

            return;
        }

        OnTimerUpdated?.Invoke(remainingTime);
    }

    // Bắt đầu một phiên điều tra mới.
    public void StartNewTimer()
    {
        remainingTime = defaultTimerSeconds;
        timerRunning = true;
        timeUpHandled = false;

        SaveRemainingTime();
        OnTimerUpdated?.Invoke(remainingTime);
    }

    // Tiếp tục đếm từ thời gian đang giữ trong bộ nhớ.
    // Có thể dùng khi bổ sung chức năng tiếp tục phiên chơi.
    public void ResumeTimer()
    {
        if (remainingTime <= 0f || timeUpHandled)
            return;

        timerRunning = true;
        OnTimerUpdated?.Invoke(remainingTime);
    }

    // Dừng Timer và lưu thời gian còn lại.
    public void PauseAndSaveTimer()
    {
        timerRunning = false;
        SaveRemainingTime();
        OnTimerUpdated?.Invoke(remainingTime);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            PauseAndSaveTimer();
        }
    }

    private void SaveRemainingTime()
    {
        PlayerPrefs.SetFloat(SavedTimeKey, remainingTime);
        PlayerPrefs.Save();
    }

    // Đọc thời gian đã lưu để dùng khi triển khai Chơi tiếp.
    public float LoadSavedTime()
    {
        return PlayerPrefs.GetFloat(
            SavedTimeKey,
            defaultTimerSeconds
        );
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            PauseAndSaveTimer();
        }
    }

    private void OnApplicationQuit()
    {
        PauseAndSaveTimer();
    }
    // =====================================================
    // CHƯƠNG HIỆN TẠI
    // =====================================================

    private int selectedChapter = 1;

    public int SelectedChapter => selectedChapter;


    public void SelectChapter(int chapter)
    {
        selectedChapter = chapter;

        Debug.Log("Đã chọn chương: " + selectedChapter);
    }
}
