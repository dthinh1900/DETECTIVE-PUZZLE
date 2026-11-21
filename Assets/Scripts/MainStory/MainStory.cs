using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainStory : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private GameObject endPanel;

    [Header("JSON Loader")]
    [SerializeField] private MainStoryLoader storyLoader;

    [Header("TMP - Nội dung")]
    [SerializeField] private TMP_Text caseTitleText;
    [SerializeField] private TMP_Text caseSummaryText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Cipher Reward UI - thêm Image vào EndPanel và gán tại đây")]
    [SerializeField] private Image rewardCipherImage;

    private MainStoryCaseData currentCaseData;
    private int dialogueIndex;
    private bool isTransitioning;
    private bool isFinalChanceDialogue;

    private void Start()
    {
        isTransitioning = false;
        isFinalChanceDialogue = GameManager.Instance != null &&
                                GameManager.Instance.IsFinalChancePending;
        dialogueIndex = GameManager.Instance != null
            ? GameManager.Instance.ConsumeStoryDialogueIndex()
            : 0;

        if (!CaseSessionData.IsSessionStarted)
        {
            Debug.LogError("[MAIN STORY] Không có phiên chơi hợp lệ.");
            ShowEndPanel();
            return;
        }

        int currentCase = isFinalChanceDialogue
            ? 5
            : CaseSessionData.CurrentCaseNumber;
        string selectedBranch = CaseSessionData.SelectedBranchId;

        Debug.Log("[MAIN STORY] Nội dung số " + currentCase +
                  " | Branch: " + selectedBranch +
                  " | Cơ hội cuối: " + isFinalChanceDialogue);

        if (storyLoader == null)
            storyLoader = GetComponent<MainStoryLoader>();

        if (storyLoader == null)
        {
            Debug.LogError("[MAIN STORY] Chưa gắn MainStoryLoader.");
            return;
        }

        if (!storyLoader.LoadJson())
            return;

        currentCaseData = storyLoader.GetCase(currentCase, selectedBranch);

        if (currentCaseData == null)
        {
            Debug.LogError("[MAIN STORY] Không tìm thấy nội dung số " + currentCase);
            return;
        }

        if (currentCaseData.dialogues == null || currentCaseData.dialogues.Count == 0)
        {
            Debug.LogError("[MAIN STORY] Nội dung không có lời thoại.");
            return;
        }

        if (storyPanel != null)
            storyPanel.SetActive(true);

        if (endPanel != null)
            endPanel.SetActive(false);

        if (caseTitleText != null)
            caseTitleText.text = currentCaseData.title;

        if (caseSummaryText != null)
            caseSummaryText.text = currentCaseData.summary;

        if (dialogueIndex < 0 || dialogueIndex >= currentCaseData.dialogues.Count)
        {
            dialogueIndex = 0;
            GameManager.Instance?.SetStoryDialogueIndex(0);
        }

        ShowDialogue();
    }

    // Hiển thị lời thoại hiện tại.
    private void ShowDialogue()
    {
        if (currentCaseData == null || currentCaseData.dialogues == null)
            return;

        if (dialogueIndex < 0 || dialogueIndex >= currentCaseData.dialogues.Count)
            return;

        MainStoryDialogueData dialogue = currentCaseData.dialogues[dialogueIndex];
        if (dialogueText != null)
        {
            dialogueText.text = "<b>" + ResolveSpeakerName(dialogue.speaker) + "</b>\n\n" +
                                dialogue.text;
        }
    }

    private string ResolveSpeakerName(string speaker)
    {
        if (speaker != "@KILLER")
            return speaker;

        switch (CaseSessionData.SelectedKillerCharacterId)
        {
            case "BAO": return "Bảo";
            case "CHI": return "Chi";
            case "DUNG": return "Dũng";
            default: return "Hung thủ";
        }
    }

    // Gắn vào nút Next Dialogue.
    public void OnNextDialogue()
    {
        if (isTransitioning)
            return;

        if (currentCaseData == null || currentCaseData.dialogues == null ||
            currentCaseData.dialogues.Count == 0)
        {
            Debug.LogError("[MAIN STORY] Không có dữ liệu lời thoại.");
            return;
        }

        int totalDialogues = currentCaseData.dialogues.Count;
        if (dialogueIndex < totalDialogues - 1)
        {
            dialogueIndex++;
            GameManager.Instance?.SetStoryDialogueIndex(dialogueIndex);
            ShowDialogue();

            Debug.Log("[MAIN STORY] Đang hiển thị lời thoại " +
                      (dialogueIndex + 1) + "/" + totalDialogues);
            return;
        }

        Debug.Log("[MAIN STORY] Đã hoàn thành toàn bộ lời thoại.");
        FinishMainStory();
    }

    // Gắn vào nút Skip.
    public void OnSkipStory()
    {
        if (isTransitioning)
            return;

        GameManager.Instance?.SetStoryDialogueIndex(dialogueIndex);
        FinishMainStory();
    }

    private void FinishMainStory()
    {
        if (currentCaseData == null || isTransitioning)
            return;

        isTransitioning = true;

        if (isFinalChanceDialogue)
        {
            Debug.Log("[MAIN STORY] Hoàn thành hội thoại cơ hội cuối. Chuyển sang Deduction.");
            if (GameManager.Instance != null)
                GameManager.Instance.CompleteFinalChance();

            SceneManager.LoadScene("Deduction");
            return;
        }

        Debug.Log("[MAIN STORY] Hoàn thành Vụ án " + CaseSessionData.CurrentCaseNumber);
        ShowEndPanel();
    }

    // Chuyển sang hiện trường của vụ án kế tiếp (Case 1 -> 2 -> 3 -> 4).
    public void StartNextInvestigation()
    {
        if (isTransitioning)
            return;

        if (!CaseSessionData.IsSessionStarted)
        {
            Debug.LogError("[MAIN STORY] Không có phiên chơi hợp lệ.");
            return;
        }

        int currentCase = CaseSessionData.CurrentCaseNumber;
        if (currentCase >= 4)
        {
            ShowEndPanel();
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("[MAIN STORY] Không tìm thấy GameManager.");
            return;
        }

        ChapterDataLoader loader = GameManager.Instance.GetComponent<ChapterDataLoader>();
        if (loader == null)
        {
            Debug.LogError("[MAIN STORY] GameManager chưa có ChapterDataLoader.");
            return;
        }

        int chapterNumber = GameManager.Instance.SelectedChapter;
        int nextCaseNumber = currentCase + 1;
        ChapterData nextChapter = loader.LoadCase(chapterNumber, nextCaseNumber);

        if (nextChapter == null)
        {
            Debug.LogError("[MAIN STORY] Không tải được Vụ án " + nextCaseNumber);
            return;
        }

        if (!CaseSessionData.LoadNextCase(nextChapter, nextCaseNumber))
        {
            Debug.LogError("[MAIN STORY] Không thể chuyển dữ liệu vụ án.");
            return;
        }

        isTransitioning = true;
        GameManager.Instance.StartNewTimer();
        GameManager.Instance.ResetStoryDialogueIndex();
        GameManager.Instance.SaveGame("Investigation");

        Debug.Log("[MAIN STORY] Bắt đầu Vụ án " + nextCaseNumber +
                  " | Branch: " + CaseSessionData.SelectedBranchId);
        SceneManager.LoadScene("Investigation");
    }

    // PUBLIC: gắn hàm này vào Button trong EndPanel để quay về Investigation.
    // Case 1-3: mở Case kế tiếp. Case 4: mở lượt điều tra cuối cùng, vẫn dùng dữ liệu Case 4.
    public void ReturnToInvestigationFromEndPanel()
    {
        if (isTransitioning == false && currentCaseData == null)
            return;

        if (!CaseSessionData.IsSessionStarted)
        {
            Debug.LogError("[MAIN STORY] Không có phiên chơi hợp lệ.");
            return;
        }

        int currentCase = CaseSessionData.CurrentCaseNumber;
        Debug.Log("[MAIN STORY] Quay lại Investigation sau Case " + currentCase);

        if (currentCase < 4)
        {
            isTransitioning = false;
            StartNextInvestigation();
            return;
        }

        if (currentCase == 4)
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("[MAIN STORY] Không tìm thấy GameManager.");
                return;
            }

            // Đánh dấu lượt điều tra cuối và chạy lại đồng hồ.
            // Không tăng CurrentCaseNumber và không xóa chứng cứ đã lưu của Case 4.
            GameManager.Instance.BeginFinalChanceInvestigation();
            isTransitioning = true;
            SceneManager.LoadScene("Investigation");
            return;
        }

        Debug.LogWarning("[MAIN STORY] Tiến trình Case không hợp lệ: " + currentCase);
    }

    // Giữ tên hàm cũ để những Button đã gán OnStartInvestigationFromEndPanel không bị mất liên kết.
    public void OnStartInvestigationFromEndPanel()
    {
        ReturnToInvestigationFromEndPanel();
    }

    public void ShowEndPanel()
    {
        if (storyPanel != null)
            storyPanel.SetActive(false);

        if (endPanel == null)
        {
            Debug.LogError("[MAIN STORY] Chưa gắn EndPanel trong Inspector; chưa ghi nhận nhân vật chết.");
            return;
        }

        endPanel.SetActive(true);

        // Chỉ ghi nhận cái chết khi EndPanel của Case 1-4 thực sự được bật.
        // Trong hội thoại, nhân vật vẫn được xem là còn sống.
        RegisterCharacterDeathForCompletedCase();
        ShowCipherRewardForCompletedCase();
    }

    private void ShowCipherRewardForCompletedCase()
    {
        if (isFinalChanceDialogue || GameManager.Instance == null ||
            GameManager.Instance.SelectedChapter != 1 ||
            !CaseSessionData.IsSessionStarted)
            return;

        int completedCase = CaseSessionData.CurrentCaseNumber;

        // Chỉ có 4 mật mã trong mỗi lượt: Intro, sau Case 1, sau Case 2, sau Case 3.
        // Sau MainStory Case 4 không phát thêm mật mã vì mục tiêu cuối cùng đã là nạn nhân thứ tư.
        if (completedCase == 4)
        {
            if (rewardCipherImage != null)
                rewardCipherImage.gameObject.SetActive(false);

            if (caseSummaryText != null)
                caseSummaryText.text = "VỤ ÁN 04 ĐÃ KHÉP LẠI.\n\nHãy tiếp tục để bước vào lượt điều tra cuối cùng.";

            return;
        }

        if (completedCase < 1 || completedCase > 3)
            return;

        CipherEntryData cipher = CipherDataLoader.GetStoryCipher(
            CaseSessionData.SelectedBranchId, completedCase);

        if (cipher == null)
        {
            Debug.LogError("[MAIN STORY CIPHER] Không tìm thấy mật mã cho Branch " +
                           CaseSessionData.SelectedBranchId + " sau Case " + completedCase);
            if (caseSummaryText != null)
                caseSummaryText.text = "Không thể tải dữ liệu mật mã. Vui lòng kiểm tra C1_Cipher.json.";
            if (rewardCipherImage != null)
                rewardCipherImage.gameObject.SetActive(false);
            return;
        }

        bool newlyCollected = GameSessionData.CollectCipher(cipher.cipherId);
        Sprite sprite = CipherDataLoader.LoadCipherImage(cipher);

        if (rewardCipherImage != null)
        {
            rewardCipherImage.sprite = sprite;
            rewardCipherImage.preserveAspect = true;
            rewardCipherImage.gameObject.SetActive(sprite != null);
        }

        if (caseSummaryText != null)
        {
            string description = string.IsNullOrWhiteSpace(cipher.description)
                ? ""
                : "\n\n" + cipher.description;

            caseSummaryText.text =
                (newlyCollected ? "ĐÃ NHẬN ĐƯỢC MẬT MÃ MỚI!\n\n" : "MẬT MÃ ĐÃ ĐƯỢC LƯU\n\n") +
                cipher.title + description;
        }

        Debug.Log("[MAIN STORY CIPHER] " + (newlyCollected ? "Đã nhận " : "Đã có sẵn ") +
                  cipher.cipherId + " | Mục tiêu: " + cipher.targetCharacterId);
    }

    private void RegisterCharacterDeathForCompletedCase()
    {
        if (!CaseSessionData.IsSessionStarted ||
            isFinalChanceDialogue ||
            GameManager.Instance == null ||
            GameManager.Instance.SelectedChapter != 1)
            return;

        int completedCase = CaseSessionData.CurrentCaseNumber;
        string deadCharacterId = CaseSessionData.GetDeadCharacterAfterCase(completedCase);

        if (string.IsNullOrWhiteSpace(deadCharacterId))
        {
            if (completedCase >= 1 && completedCase <= 4)
                Debug.LogWarning("[MAIN STORY] Không tìm thấy nhân vật cần ghi nhận đã chết sau Case " + completedCase);
            return;
        }

        // Chỉ ghi nhận sau khi EndPanel xuất hiện; nhân vật vẫn sống trong hội thoại.
        bool newlyMarked = GameSessionData.MarkCharacterDead(1, deadCharacterId);
        if (newlyMarked)
        {
            Debug.Log("[MAIN STORY] EndPanel xuất hiện sau Case " +
                      completedCase + " | Nhân vật đã chết: " + deadCharacterId);
        }
    }
}
