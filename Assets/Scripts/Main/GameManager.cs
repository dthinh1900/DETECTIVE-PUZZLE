using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Timer")]
    [SerializeField] private float defaultTimerSeconds = 600f;
    private float remainingTime;
    private bool timerRunning;
    private bool timeUpHandled;
    private int selectedChapter = 1;
    private int storyDialogueIndex;
    private bool finalChancePending;
    private bool gameLost;
    private string lastGameplayScene = "Investigation";

    public float RemainingTime => remainingTime;
    public bool IsTimerRunning => timerRunning;
    public int SelectedChapter => selectedChapter;
    public int StoryDialogueIndex => storyDialogueIndex;
    public bool IsFinalChancePending => finalChancePending;
    public event Action<float> OnTimerUpdated;
    public event Action OnTimeUp;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        remainingTime = defaultTimerSeconds;
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void Update()
    {
        if (!timerRunning) return;
        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f; timerRunning = false; SaveGame(); OnTimerUpdated?.Invoke(remainingTime);
            if (!timeUpHandled)
            {
                timeUpHandled = true; OnTimeUp?.Invoke();
                string activeScene = SceneManager.GetActiveScene().name;
                if (activeScene != "MainStory" && activeScene != "Deduction")
                    SceneManager.LoadScene("MainStory");
            }
            return;
        }
        OnTimerUpdated?.Invoke(remainingTime);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Investigation" || scene.name == "Story" ||
            scene.name == "MainStory" || scene.name == "Deduction")
            lastGameplayScene = scene.name;
        if (CaseSessionData.IsSessionStarted) SaveGame();
    }

    public void SelectChapter(int chapter) { selectedChapter = chapter; }

    public void StartNewTimer()
    {
        remainingTime = defaultTimerSeconds; timerRunning = true; timeUpHandled = false; OnTimerUpdated?.Invoke(remainingTime); SaveGame();
    }

    public float LoadSavedTime() => remainingTime;

    public void ResumeTimer()
    {
        if (remainingTime <= 0f || timeUpHandled) return;
        timerRunning = true; OnTimerUpdated?.Invoke(remainingTime);
    }

    public void PauseAndSaveTimer()
    {
        timerRunning = false; SaveGame(); OnTimerUpdated?.Invoke(remainingTime);
    }

    public void SaveGame(string sceneOverride = null)
    {
        if (gameLost || !CaseSessionData.IsSessionStarted || CaseSessionData.CurrentCaseNumber < 1) return;
        string activeScene = SceneManager.GetActiveScene().name;
        if (activeScene == "Investigation" || activeScene == "Story" ||
            activeScene == "MainStory" || activeScene == "Deduction")
            lastGameplayScene = activeScene;
        string targetScene = sceneOverride;
        if (string.IsNullOrEmpty(targetScene))
            targetScene = (activeScene == "MainMenu" || activeScene == "ChapterSelect" || activeScene == "CaseIntro") ? lastGameplayScene : activeScene;
        if (targetScene != "Investigation" && targetScene != "Story" &&
            targetScene != "MainStory" && targetScene != "Deduction")
            targetScene = "Investigation";

        GameSaveData data = new GameSaveData
        {
            chapterId = selectedChapter,
            selectedBranchId = CaseSessionData.SelectedBranchId,
            currentCaseNumber = CaseSessionData.CurrentCaseNumber,
            viewingCaseNumber = CaseSessionData.ViewingCaseNumber > 0
                ? CaseSessionData.ViewingCaseNumber
                : CaseSessionData.CurrentCaseNumber,
            currentSceneName = targetScene,
            remainingTime = remainingTime,
            storyDialogueIndex = storyDialogueIndex,
            finalChancePending = finalChancePending
        };
        GameSessionData.WriteToSave(data);
        GameSaveManager.Save(data);
    }

    public bool ContinueGame()
    {
        if (gameLost) return false;

        GameSaveData data = GameSaveManager.Load();
        if (data == null) return false;
        ChapterDataLoader loader = GetComponent<ChapterDataLoader>();
        if (loader == null) { Debug.LogError("[CONTINUE] GameManager thiếu ChapterDataLoader."); return false; }
        ChapterData chapter = loader.LoadCase(data.chapterId, data.currentCaseNumber);
        if (chapter == null) return false;

        selectedChapter = data.chapterId;
        if (!CaseSessionData.RestoreSession(chapter, data.selectedBranchId, data.currentCaseNumber))
            return false;

        GameSessionData.RestoreFromSave(data);

        int viewingCase = data.viewingCaseNumber;
        if (viewingCase < 1 || viewingCase > data.currentCaseNumber)
            viewingCase = data.currentCaseNumber;

        ChapterData viewingChapter = viewingCase == data.currentCaseNumber
            ? chapter
            : loader.LoadCase(data.chapterId, viewingCase);

        if (viewingChapter == null || !CaseSessionData.SetViewingCase(viewingChapter, viewingCase))
        {
            Debug.LogWarning("[CONTINUE] Không khôi phục được Case đang xem; quay về Case tiến trình hiện tại.");
            if (!CaseSessionData.SetViewingCase(chapter, data.currentCaseNumber))
                return false;
        }

        remainingTime = Mathf.Max(0f, data.remainingTime);
        timerRunning = remainingTime > 0f;
        timeUpHandled = remainingTime <= 0f;
        storyDialogueIndex = Mathf.Max(0, data.storyDialogueIndex);
        finalChancePending = data.finalChancePending;
        lastGameplayScene = string.IsNullOrEmpty(data.currentSceneName) ? "Investigation" : data.currentSceneName;
        if (lastGameplayScene != "Investigation" && lastGameplayScene != "Story" &&
            lastGameplayScene != "MainStory" && lastGameplayScene != "Deduction")
            lastGameplayScene = "Investigation";
        SceneManager.LoadScene(lastGameplayScene);
        return true;
    }

    // Kết thúc ván ngay khi người chơi đưa ra suy luận sai.
    // Không trừ thêm thời gian; dừng timer và xóa Save để không thể Continue ván đã thua.
    public void LoseImmediately()
    {
        if (gameLost) return;

        gameLost = true;
        timerRunning = false;
        timeUpHandled = true;
        finalChancePending = false;
        storyDialogueIndex = 0;
        GameSaveManager.DeleteSave();
        OnTimerUpdated?.Invoke(remainingTime);
        Debug.Log("[GAME OVER] Người chơi đã thua do suy luận sai. Save đã bị xóa.");
    }

    // Được gọi khi người chơi bấm nút ở EndPanel sau Vụ án 4.
    // Giữ nguyên dữ liệu Case 4, chỉ mở thêm một lượt điều tra cuối cùng.
    public void BeginFinalChanceInvestigation()
    {
        if (!CaseSessionData.IsSessionStarted || CaseSessionData.CurrentCaseNumber != 4)
        {
            Debug.LogError("[FINAL CHANCE] Chỉ có thể bắt đầu sau khi hoàn thành Vụ án 4.");
            return;
        }

        finalChancePending = true;
        ResetStoryDialogueIndex();
        StartNewTimer();
        SaveGame("Investigation");
        Debug.Log("[FINAL CHANCE] Đã mở lượt điều tra cuối cùng.");
    }

    // Được gọi sau khi hội thoại số 5 kết thúc, ngay trước khi vào Deduction.
    public void CompleteFinalChance()
    {
        finalChancePending = false;
        timerRunning = false;
        SaveGame("Deduction");
        OnTimerUpdated?.Invoke(remainingTime);
        Debug.Log("[FINAL CHANCE] Hoàn tất hội thoại cuối; chuyển sang Deduction.");
    }

    public void SetStoryDialogueIndex(int index)
    {
        storyDialogueIndex = Mathf.Max(0, index);
        SaveGame("MainStory");
    }

    public int ConsumeStoryDialogueIndex()
    {
        int value = storyDialogueIndex;
        return value;
    }

    public void ResetStoryDialogueIndex() { storyDialogueIndex = 0; }

    public void BeginNewGame(int chapter)
    {
        gameLost = false;
        selectedChapter = chapter;
        GameSaveManager.DeleteSave();
        GameSessionData.ClearAllSessionData();
        CaseSessionData.ClearSession();
        remainingTime = defaultTimerSeconds;
        timerRunning = false;
        timeUpHandled = false;
        storyDialogueIndex = 0;
        finalChancePending = false;
        lastGameplayScene = "Investigation";
    }

    private void OnApplicationPause(bool paused) { if (paused) SaveGame(); }
    private void OnApplicationQuit() { SaveGame(); }
}
