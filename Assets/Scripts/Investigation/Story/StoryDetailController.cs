
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        // -1: giữ nguyên Background hiện tại.
        // 0, 1, 2...: đổi sang ảnh nền tương ứng.
        public int backgroundIndex = -1;
    }

    [Serializable]
    public class StoryData
    {
        public string storyId;
        public DialogueLine[] dialogues;
    }

    [Serializable]
    public class ChapterStoryData
    {
        public StoryData[] stories;
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
    //[SerializeField] private TMP_Text characterNameText;
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
    // START
    // =========================

    private void Start()
    {
        // Mac dinh hien danh sach Story.
        if (sceneStory != null)
            sceneStory.SetActive(true);

        if (storyDetailPanel != null)
            storyDetailPanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (endPanel != null)
            endPanel.SetActive(false);
    }

    public void SetChapterData(ChapterStoryData data)
    {
        chapterData = data;
    }
    

    
    public void OpenStory(
        string storyId,
        Sprite[] storyBackgrounds)
    {
        Debug.Log("DETAIL 1: OpenStory được gọi, ID = " + storyId);
        // Tự tải JSON nếu dữ liệu chưa được tải.
        if (chapterData == null ||
        chapterData.stories == null)
        {
            Debug.LogError(
                "StoryDetailController: Chua nhan du lieu Story tu StoryController."
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(storyId))
        {
            Debug.LogError("Story ID không được để trống.");
            return;
        }

        currentStory = null;

        // Tìm đúng Story theo ID.
        foreach (StoryData story in chapterData.stories)
        {
            if (story != null &&
                story.storyId == storyId)
            {
                currentStory = story;
                break;
            }
        }

        if (currentStory == null)
        {
            Debug.LogError(
                "Không tìm thấy Story có ID: " + storyId
            );
            return;
        }

        if (currentStory.dialogues == null ||
            currentStory.dialogues.Length == 0)
        {
            Debug.LogWarning(
                "Story không có lời thoại: " + storyId
            );
            currentStory = null;
            return;
        }

        // Sử dụng bộ ảnh nền của Story được chọn.
        // Nếu truyền null, giữ nguyên mảng backgrounds
        // đã gán trong Inspector.
        if (storyBackgrounds != null)
            backgrounds = storyBackgrounds;

        dialogueIndex = 0;
        storyFinished = false;

            sceneStory.SetActive(false);
            storyDetailPanel.SetActive(true);
            dialoguePanel.SetActive(true);
            endPanel.SetActive(false);
        
        if (nextDialogueButton != null)
        {
            nextDialogueButton.gameObject.SetActive(true);
            nextDialogueButton.interactable = true;
        }

        ShowDialogue();
    }

    public void OpenStory(
        TextAsset dialogueJson,
        Sprite[] storyBackgrounds)
    {
        if (dialogueJson == null)
        {
            Debug.LogError(
                "StoryDetailController: Chưa gán file JSON!"
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
                "JSON Story riêng không có lời thoại hợp lệ."
            );
            return;
        }

        currentStory = legacyStory;

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

        // Tên người nói giống Intro.
        //if (characterNameText != null) characterNameText.text = line.speaker;

        // Nội dung lời thoại.
        if (dialogueText != null)
            dialogueText.text = line.text;

        // Chỉ đổi Background khi có chỉ định.
        if (line.backgroundIndex >= 0)
        {
            ChangeBackground(line.backgroundIndex);
        }

        // Cập nhật ảnh nhân vật theo người nói.
        Sprite portrait = GetCharacterPortrait(line.speaker);

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
                "Chưa gán Background Image!"
            );
            return;
        }

        if (backgrounds == null ||
            index < 0 ||
            index >= backgrounds.Length)
        {
            Debug.LogWarning(
                "Background Index không hợp lệ: " + index
            );
            return;
        }

        if (backgrounds[index] == null)
        {
            Debug.LogWarning(
                "Chưa gán ảnh nền tại vị trí: " + index
            );
            return;
        }

        backgroundImage.sprite = backgrounds[index];
    }


    public void OnNext  ()
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


    private void FinishStory()
    {
        storyFinished = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (endPanel != null)
            endPanel.SetActive(true);

        if (endText != null)
            endText.text = "KẾT THÚC CÂU CHUYỆN";
    }


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
