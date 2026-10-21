
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryDetailController : MonoBehaviour
{
    [Serializable]
    public class DialogueData
    {
        public DialogueLine[] dialogues;
    }

    [Serializable]
    public class DialogueLine
    {
        public string speaker;
        public string text;

        // -1: giu nguyen Background hien tai.
        // 0, 1, 2...: doi sang anh nen tuong ung.
        public int backgroundIndex = -1;
    }

    [Header("Panels")]
    [SerializeField] private GameObject sceneStory;
    [SerializeField] private GameObject storyDetailPanel;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject endPanel;

    [Header("Background")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite[] backgrounds;

    [Header("Dialogue UI")]
    [SerializeField] private Image characterImage;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button nextDialogueButton;
    [SerializeField] private TMP_Text endText;

    [Header("Character Portraits")]
    [SerializeField] private Sprite chatGPTPortrait;
    [SerializeField] private Sprite giangPortrait;
    [SerializeField] private Sprite baoPortrait;
    [SerializeField] private Sprite chiPortrait;
    [SerializeField] private Sprite dungPortrait;
    [SerializeField] private Sprite haiPortrait;

    private DialogueData dialogueData;
    private int dialogueIndex;
    private bool storyFinished;

    private void Start()
    {
        // Mac dinh hien danh sach, an chi tiet.
        if (sceneStory != null)
            sceneStory.SetActive(true);

        if (storyDetailPanel != null)
            storyDetailPanel.SetActive(false);

        if (endPanel != null)
            endPanel.SetActive(false);
    }

    // Goi ham nay khi nguoi choi chon mot Story.
    public void OpenStory(
        TextAsset dialogueJson,
        Sprite[] storyBackgrounds)
    {
        if (dialogueJson == null)
        {
            Debug.LogError(
                "StoryDetailController: Chua gan file JSON!"
            );
            return;
        }

        // Doc du lieu cua Story duoc chon.
        dialogueData =
            JsonUtility.FromJson<DialogueData>(
                dialogueJson.text
            );

        if (dialogueData == null ||
            dialogueData.dialogues == null ||
            dialogueData.dialogues.Length == 0)
        {
            Debug.LogError(
                "StoryDetailController: JSON khong co loi thoai hop le!"
            );
            return;
        }

        // Nhan bo anh nen rieng cua Story nay.
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
            nextDialogueButton.gameObject.SetActive(true);

        ShowDialogue();
    }

    // Hien thi loi thoai hien tai.
    private void ShowDialogue()
    {
        if (dialogueData == null ||
            dialogueData.dialogues == null ||
            dialogueIndex < 0 ||
            dialogueIndex >= dialogueData.dialogues.Length)
        {
            return;
        }

        DialogueLine line =
            dialogueData.dialogues[dialogueIndex];

        if (dialogueText != null)
            dialogueText.text = line.text;

        // Chi doi Background khi co chi dinh hop le.
        if (line.backgroundIndex >= 0)
        {
            ChangeBackground(line.backgroundIndex);
        }

        // Chon anh nhan vat theo speaker.
        Sprite portrait = GetCharacterPortrait(line.speaker);

        if (characterImage != null)
        {
            characterImage.sprite = portrait;
            characterImage.gameObject.SetActive(
                portrait != null
            );
        }
    }

    // Lay anh dai dien theo ten nguoi noi.
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

    // Thay doi anh nen theo index.
    private void ChangeBackground(int index)
    {
        if (backgroundImage == null)
        {
            Debug.LogWarning(
                "Chua gan Background Image!"
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

    // Goi tu Button Next.
    public void OnNextDialogue()
    {
        if (storyFinished ||
            dialogueData == null ||
            dialogueData.dialogues == null)
        {
            return;
        }

        dialogueIndex++;

        if (dialogueIndex >= dialogueData.dialogues.Length)
        {
            FinishStory();
            return;
        }

        ShowDialogue();
    }

    // Ket thuc Story: toi panel va hien chu ket thuc.
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

    // Goi tu nut CloseStory.
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

        dialogueData = null;
        dialogueIndex = 0;
        storyFinished = false;
    }
}
