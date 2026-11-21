using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Timer")]
    [SerializeField] private float defaultTimerSeconds = 600f;
    // Giới hạn thời gian tối đa được cấp cho MỖI giai đoạn điều tra.
    // Giá trị này giảm một nửa sau mỗi đáp án mật mã sai và được giữ xuyên các giai đoạn.
    private float maxTimerPerStageSeconds;
    private float remainingTime;
    private bool timerRunning;
    private bool timeUpHandled;
    private int selectedChapter = 1;
    private int storyDialogueIndex;
    private bool finalChancePending;
    private bool gameLost;
    private bool gameWon;
    private bool cipherVictoryPending;
    private bool resumeTimerAfterCipherWrongAnswer;
    // Đang ở màn chọn Chapter sau khi người chơi bấm Chơi mới.
    // Tạm ngăn autosave ghi đè Save cũ trước khi Chapter mới thực sự bắt đầu.
    private bool newGameSelectionPending;
    private string lastGameplayScene = "Investigation";

    public float RemainingTime => remainingTime;
    public float MaxTimerPerStageSeconds => maxTimerPerStageSeconds;
    public bool IsTimerRunning => timerRunning;
    public int SelectedChapter => selectedChapter;
    public int StoryDialogueIndex => storyDialogueIndex;
    public bool IsFinalChancePending => finalChancePending;
    public bool IsCipherVictoryPending => cipherVictoryPending;
    public event Action<float> OnTimerUpdated;
    public event Action OnTimeUp;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        maxTimerPerStageSeconds = Mathf.Max(0f, defaultTimerSeconds);
        remainingTime = maxTimerPerStageSeconds;
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

    // Được gọi ngay khi người chơi nhấn nút Chơi mới ở MainMenu.
    // Reset giới hạn suy luận về mặc định ngay lập tức, trước khi vào ChapterSelect.
    // Không xóa Save cũ tại đây; nếu người chơi quay lại và chọn Continue,
    // ContinueGame sẽ khôi phục lại đúng giới hạn đã lưu của lượt chơi cũ.
    public void PrepareForNewGameSelection()
    {
        newGameSelectionPending = true;
        maxTimerPerStageSeconds = Mathf.Max(0f, defaultTimerSeconds);
        remainingTime = maxTimerPerStageSeconds;
        timerRunning = false;
        timeUpHandled = false;
        resumeTimerAfterCipherWrongAnswer = false;
        OnTimerUpdated?.Invoke(remainingTime);

        Debug.Log("[NEW GAME] Đã reset giới hạn thời gian suy luận về " +
            maxTimerPerStageSeconds.ToString("F1") + " giây khi nhấn Chơi mới.");
    }

    public void StartNewTimer()
    {
        // Mỗi giai đoạn bắt đầu bằng mức tối đa hiện tại.
        // Không quay lại 10 phút nếu người chơi đã từng trả lời sai trong cùng lượt chơi.
        remainingTime = Mathf.Max(0f, maxTimerPerStageSeconds);
        timerRunning = true;
        timeUpHandled = false;
        resumeTimerAfterCipherWrongAnswer = false;
        OnTimerUpdated?.Invoke(remainingTime);
        SaveGame();
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
        // Người chơi đã chọn Chơi mới nhưng chưa xác nhận Chapter.
        // Không ghi trạng thái timer vừa reset đè lên Save của lượt chơi cũ.
        if (newGameSelectionPending) return;
        if (gameLost || gameWon || !CaseSessionData.IsSessionStarted || CaseSessionData.CurrentCaseNumber < 1) return;
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
            selectedKillerCharacterId = CaseSessionData.SelectedKillerCharacterId,
            currentCaseNumber = CaseSessionData.CurrentCaseNumber,
            viewingCaseNumber = CaseSessionData.ViewingCaseNumber > 0
                ? CaseSessionData.ViewingCaseNumber
                : CaseSessionData.CurrentCaseNumber,
            currentSceneName = targetScene,
            remainingTime = remainingTime,
            maxTimerPerStageSeconds = maxTimerPerStageSeconds,
            storyDialogueIndex = storyDialogueIndex,
            finalChancePending = finalChancePending
        };
        GameSessionData.WriteToSave(data);
        GameSaveManager.Save(data);
    }

    public bool ContinueGame()
    {
        if (gameLost || gameWon) return false;

        GameSaveData data = GameSaveManager.Load();
        if (data == null) return false;
        ChapterDataLoader loader = GetComponent<ChapterDataLoader>();
        if (loader == null) { Debug.LogError("[CONTINUE] GameManager thiếu ChapterDataLoader."); return false; }
        ChapterData chapter = loader.LoadCase(data.chapterId, data.currentCaseNumber);
        if (chapter == null) return false;

        selectedChapter = data.chapterId;
        if (!CaseSessionData.RestoreSession(
                chapter,
                data.selectedBranchId,
                data.currentCaseNumber,
                data.selectedKillerCharacterId))
            return false;

        GameSessionData.RestoreFromSave(data);
        newGameSelectionPending = false;

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

        // Save cũ (Version 3/4) chưa có mức thời gian tối đa; dùng lại cấu hình mặc định.
        maxTimerPerStageSeconds = data.maxTimerPerStageSeconds > 0f
            ? data.maxTimerPerStageSeconds
            : Mathf.Max(0f, defaultTimerSeconds);
        remainingTime = Mathf.Clamp(data.remainingTime, 0f, maxTimerPerStageSeconds);
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

    // Đáp án mật mã sai: giảm một nửa GIỚI HẠN TỐI ĐA cho mỗi giai đoạn.
    // Đồng hồ hiện tại không bị chia đôi theo số giây còn lại; nó chỉ bị giới hạn
    // nếu số giây đang còn lớn hơn mức tối đa mới. Các giai đoạn sau sẽ nhận đủ mức mới.
    public void ApplyCipherWrongAnswerPenaltyAndPause()
    {
        if (gameLost || gameWon)
            return;

        resumeTimerAfterCipherWrongAnswer = timerRunning;
        timerRunning = false;

        float previousLimit = maxTimerPerStageSeconds;
        maxTimerPerStageSeconds = Mathf.Max(0f, maxTimerPerStageSeconds * 0.5f);
        remainingTime = Mathf.Min(Mathf.Max(0f, remainingTime), maxTimerPerStageSeconds);

        OnTimerUpdated?.Invoke(remainingTime);
        SaveGame("Investigation");

        Debug.Log(
            "[CIPHER] Đáp án sai. Giới hạn mỗi giai đoạn giảm từ " +
            previousLimit.ToString("F1") + "s xuống " +
            maxTimerPerStageSeconds.ToString("F1") + "s. Thời gian giai đoạn hiện tại: " +
            remainingTime.ToString("F1") + "s. Đồng hồ tạm dừng chờ người chơi xác nhận."
        );
    }

    // Gọi khi người chơi đóng Panel báo sai.
    // Chỉ tiếp tục đồng hồ nếu trước lúc báo sai đồng hồ đang chạy.
    public void ResumeTimerAfterCipherWrongAnswer()
    {
        bool shouldResume = resumeTimerAfterCipherWrongAnswer;
        resumeTimerAfterCipherWrongAnswer = false;

        if (shouldResume && remainingTime > 0f && !timeUpHandled && !gameLost && !gameWon)
            timerRunning = true;

        OnTimerUpdated?.Invoke(remainingTime);
        SaveGame("Investigation");
    }

    // Giải đúng bất kỳ mật mã hiện tại nào sẽ kết thúc game ngay bằng chiến thắng.
    public void WinImmediatelyFromCipher()
    {
        if (gameLost || gameWon)
            return;

        gameWon = true;
        cipherVictoryPending = true;
        timerRunning = false;
        timeUpHandled = true;
        finalChancePending = false;
        storyDialogueIndex = 0;
        resumeTimerAfterCipherWrongAnswer = false;

        OnTimerUpdated?.Invoke(remainingTime);
        GameSaveManager.DeleteSave();

        Debug.Log("[GAME WIN] Người chơi giải đúng mật mã. Chuyển đến Panel_Win.");
        SceneManager.LoadScene("Deduction");
    }

    // Kết thúc ván ngay khi người chơi đưa ra suy luận sai.
    // Không trừ thêm thời gian; dừng timer và xóa Save để không thể Continue ván đã thua.
    public void LoseImmediately()
    {
        if (gameLost) return;

        gameLost = true;
        gameWon = false;
        cipherVictoryPending = false;
        timerRunning = false;
        timeUpHandled = true;
        finalChancePending = false;
        storyDialogueIndex = 0;
        resumeTimerAfterCipherWrongAnswer = false;
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
        newGameSelectionPending = false;
        gameLost = false;
        gameWon = false;
        cipherVictoryPending = false;
        resumeTimerAfterCipherWrongAnswer = false;
        selectedChapter = chapter;
        GameSaveManager.DeleteSave();
        GameSessionData.ClearAllSessionData();
        CaseSessionData.ClearSession();
        // New Game / chơi lại / chuyển sang Chapter mới: khôi phục giới hạn ban đầu.
        maxTimerPerStageSeconds = Mathf.Max(0f, defaultTimerSeconds);
        remainingTime = maxTimerPerStageSeconds;
        timerRunning = false;
        timeUpHandled = false;
        storyDialogueIndex = 0;
        finalChancePending = false;
        lastGameplayScene = "Investigation";
    }

    private void OnApplicationPause(bool paused) { if (paused) SaveGame(); }
    private void OnApplicationQuit() { SaveGame(); }
}
