using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoryDetailController : MonoBehaviour
{
    // =========================
    // JSON DATA
    // =========================

    [Serializable]
    public class DialogueLine
    {
        public string speaker;
        public string text;

        // -1: giu Background hien tai.
        // 0, 1, 2...: doi Background.
        public int backgroundIndex = -1;
    }

    [Serializable]
    public class StatementUpdateData
    {
        public string characterId;
        public string question;
        public string answer;
    }

    [Serializable]
    public class StoryData
    {
        public int caseNumber;

        public string storyId;
        public string title;

        // "special" hoac "normal".
        public string storyType;

        // Story dac biet.
        public string requiredEvidenceId;

        // Story thuong: 4 Evidence co the su dung.
        public string[] candidateEvidenceIds;

        // So Evidence can thu thap.
        public int requiredEvidenceCount = 3;

        public StatementUpdateData[] statementUpdates;

        public DialogueLine[] dialogues;
    }
    [Serializable]
    public class BranchStoryData
    {
        public string branchId;
        public StoryData[] stories;
    }

    [Serializable]
    public class ChapterStoryData
    {
        public BranchStoryData[] branches;
    }

    // =========================
    // PANELS
    // =========================

    [Header("Panels")]
    [SerializeField] private GameObject sceneStory;
    [SerializeField] private GameObject storyDetailPanel;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject endPanel;

    private ChapterStoryData chapterData;
    private BranchStoryData selectedBranch;

    private string selectedBranchId;

    // =========================
    // BACKGROUND
    // =========================

    [Header("Background")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite[] backgrounds;

    // =========================
    // DIALOGUE UI
    // =========================

    [Header("Dialogue UI")]
    [SerializeField] private Image characterImage;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button nextDialogueButton;
    [SerializeField] private TMP_Text endText;

    // =========================
    // CHARACTER PORTRAITS
    // =========================

    [Header("Character Portraits")]
    [SerializeField] private Sprite chatGPTPortrait;
    [SerializeField] private Sprite giangPortrait;
    [SerializeField] private Sprite baoPortrait;
    [SerializeField] private Sprite chiPortrait;
    [SerializeField] private Sprite dungPortrait;
    [SerializeField] private Sprite haiPortrait;

    // =========================
    // RUNTIME DATA
    // =========================

    private StoryData currentStory;

    private int dialogueIndex;
    private bool storyFinished;

    // =========================
    // Awake
    // =========================

    private void Start()
    {
        if (sceneStory != null)
            sceneStory.SetActive(true);

        if (storyDetailPanel != null)
            storyDetailPanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (endPanel != null)
            endPanel.SetActive(false);
    }

    // =========================
    // RECEIVE CHAPTER DATA
    // =========================

    public void SetChapterData(ChapterStoryData data)
    {
        SetChapterData(
            data,
            CaseSessionData.SelectedBranchId
        );
    }

    public void SetChapterData(
        ChapterStoryData data,
        string branchId)
    {
        chapterData = data;
        selectedBranchId = branchId;
        selectedBranch = null;

        if (chapterData == null ||
            chapterData.branches == null)
        {
            Debug.LogError(
                "StoryDetailController: Chapter data khong hop le."
            );
            return;
        }

        foreach (BranchStoryData branch in chapterData.branches)
        {
            if (branch != null &&
                branch.branchId == selectedBranchId)
            {
                selectedBranch = branch;
                break;
            }
        }

        if (selectedBranch == null)
        {
            Debug.LogError(
                "StoryDetailController: Khong tim thay Branch " +
                selectedBranchId
            );
        }
    }

    // =========================
    // OPEN STORY BY OBJECT
    // =========================

    public void OpenStory(
        StoryData story,
        Sprite[] storyBackgrounds)
    {
        if (story == null)
        {
            Debug.LogError(
                "StoryDetailController: Story khong hop le."
            );
            return;
        }

        if (story.dialogues == null ||
            story.dialogues.Length == 0)
        {
            Debug.LogWarning(
                "Story khong co loi thoai: " + story.storyId
            );
            return;
        }

        currentStory = story;

        if (storyBackgrounds != null)
            backgrounds = storyBackgrounds;

        dialogueIndex = 0;
        storyFinished = false;

        if (sceneStory != null)
            sceneStory.SetActive(false);

        if (storyDetailPanel != null)
            storyDetailPanel.SetActive(true);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (endPanel != null)
            endPanel.SetActive(false);

        if (nextDialogueButton != null)
        {
            nextDialogueButton.gameObject.SetActive(true);
            nextDialogueButton.interactable = true;
        }

        ShowDialogue();
    }

    // =========================
    // OPEN STORY BY ID
    // =========================

    public void OpenStory(
        string storyId,
        Sprite[] storyBackgrounds)
    {
        if (selectedBranch == null ||
            selectedBranch.stories == null)
        {
            Debug.LogError(
                "StoryDetailController: Chua co Story cua Branch."
            );
            return;
        }

        StoryData foundStory = null;

        foreach (StoryData story in selectedBranch.stories)
        {
            if (story != null &&
                story.storyId == storyId)
            {
                foundStory = story;
                break;
            }
        }

        if (foundStory == null)
        {
            Debug.LogError(
                "Khong tim thay Story: " + storyId
            );
            return;
        }

        OpenStory(foundStory, storyBackgrounds);
    }

    // =========================
    // LEGACY OPEN STORY
    // =========================

    public void OpenStory(
        TextAsset dialogueJson,
        Sprite[] storyBackgrounds)
    {
        if (dialogueJson == null)
        {
            Debug.LogError(
                "StoryDetailController: Chua gan file JSON."
            );
            return;
        }

        StoryData legacyStory =
            JsonUtility.FromJson<StoryData>(
                dialogueJson.text
            );

        if (legacyStory == null ||
            legacyStory.dialogues == null ||
            legacyStory.dialogues.Length == 0)
        {
            Debug.LogError(
                "JSON Story rieng khong co loi thoai hop le."
            );
            return;
        }

        OpenStory(legacyStory, storyBackgrounds);
    }

    // =========================
    // SHOW DIALOGUE
    // =========================

    private void ShowDialogue()
    {
        if (currentStory == null ||
            currentStory.dialogues == null ||
            dialogueIndex < 0 ||
            dialogueIndex >= currentStory.dialogues.Length)
        {
            return;
        }

        DialogueLine line =
            currentStory.dialogues[dialogueIndex];

        if (line == null)
            return;

        if (dialogueText != null)
            dialogueText.text = line.text;

        if (line.backgroundIndex >= 0)
            ChangeBackground(line.backgroundIndex);

        Sprite portrait =
            GetCharacterPortrait(line.speaker);

        if (characterImage != null)
        {
            characterImage.sprite = portrait;

            characterImage.gameObject.SetActive(
                portrait != null
            );
        }

        if (nextDialogueButton != null)
        {
            nextDialogueButton.gameObject.SetActive(true);
            nextDialogueButton.interactable = true;
        }
    }

    private Sprite GetCharacterPortrait(string speaker)
    {
        switch (speaker)
        {
            case "ChatGPT":
                return chatGPTPortrait;

            case "Giang":
                return giangPortrait;

            case "Bảo":
            case "Bao":
                return baoPortrait;

            case "Chi":
                return chiPortrait;

            case "Dũng":
            case "Dung":
                return dungPortrait;

            case "Hải":
            case "Hai":
                return haiPortrait;

            case "Người dẫn chuyện":
                return null;

            default:
                return null;
        }
    }

    private void ChangeBackground(int index)
    {
        if (backgroundImage == null)
        {
            Debug.LogWarning(
                "Chua gan Background Image."
            );
            return;
        }

        if (backgrounds == null ||
            index < 0 ||
            index >= backgrounds.Length)
        {
            Debug.LogWarning(
                "Background Index khong hop le: " + index
            );
            return;
        }

        if (backgrounds[index] == null)
        {
            Debug.LogWarning(
                "Chua gan anh nen tai vi tri: " + index
            );
            return;
        }

        backgroundImage.sprite = backgrounds[index];
    }

    // =========================
    // NEXT DIALOGUE
    // =========================

    public void OnNext()
    {
        if (storyFinished ||
            currentStory == null ||
            currentStory.dialogues == null)
        {
            return;
        }

        dialogueIndex++;

        if (dialogueIndex >= currentStory.dialogues.Length)
        {
            FinishStory();
            return;
        }

        ShowDialogue();
    }

    // =========================
    // FINISH STORY
    // =========================

    private void FinishStory()
    {
        if (storyFinished)
            return;

        storyFinished = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (nextDialogueButton != null)
            nextDialogueButton.gameObject.SetActive(false);

        if (endText != null)
            endText.text = "KẾT THÚC CÂU CHUYỆN";

        // Chỉ cập nhật nếu EndPanel thực sự tồn tại và đã được bật.
        if (endPanel != null)
        {
            endPanel.SetActive(true);
            ApplyStatementUpdates();
        }
        else
        {
            Debug.LogError("StoryDetailController: Chưa gán EndPanel; bỏ qua cập nhật lời khai.");
        }
    }

    private void ApplyStatementUpdates()
    {
        if (currentStory == null)
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "Khong tim thay GameManager, khong the cap nhat Statement."
            );
            return;
        }

        int chapterId =
            GameManager.Instance.SelectedChapter;

        // Tất cả Story đều được đánh dấu hoàn thành để không tự mở lại.
        // Nhưng chỉ Story đặc biệt mới được phép cập nhật lời khai.
        string branchId = selectedBranchId;

        string completionKey =
            chapterId + "|" +
            branchId + "|" +
            currentStory.storyId;

        // Moi Story chi duoc ap dung cap nhat mot lan trong phien.
        if (!GameSessionData.TryMarkStoryCompleted(completionKey))
        {
            Debug.Log(
                "Story da cap nhat Statement truoc do: " +
                currentStory.storyId
            );
            return;
        }

        if (!string.Equals(currentStory.storyType, "special", StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("Story thường hoàn thành; không cập nhật lời khai: " + currentStory.storyId);
            return;
        }

        if (currentStory.statementUpdates == null || currentStory.statementUpdates.Length == 0)
        {
            Debug.LogWarning("Story đặc biệt chưa có statementUpdates: " + currentStory.storyId);
            return;
        }

        foreach (
            StatementUpdateData update
            in currentStory.statementUpdates
        )
        {
            if (update == null)
                continue;

            GameSessionData.UpsertStoryStatement(
                chapterId,
                update.characterId,
                update.question,
                update.answer
            );
        }

        Debug.Log(
            "Da ap dung Statement cua Story: " +
            currentStory.storyId
        );
    }

    // =========================
    // MANUAL EXIT REQUEST
    // =========================

    // Gan ham nay vao BTN_CloseStory trong StoryDetailPanel.
    // Thoat giua chung khong cap nhat Statement.
    public void RequestCloseStory()
    {
        if (currentStory == null)
            return;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (nextDialogueButton != null)
            nextDialogueButton.gameObject.SetActive(false);

        if (endPanel != null)
            endPanel.SetActive(true);

        if (endText != null)
        {
            endText.text = storyFinished
                ? "KẾT THÚC CÂU CHUYỆN"
                : "Bạn đã thoát khi câu chuyện chưa kết thúc. Lời khai chưa được cập nhật.";
        }
    }

    // =========================
    // CLOSE STORY
    // =========================

    // Gan ham nay vao Button nam trong EndPanel.
    public void CloseStory()
    {
        if (storyDetailPanel != null)
            storyDetailPanel.SetActive(false);

        if (endPanel != null)
            endPanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (sceneStory != null)
            sceneStory.SetActive(true);

        currentStory = null;
        dialogueIndex = 0;
        storyFinished = false;
    }
}