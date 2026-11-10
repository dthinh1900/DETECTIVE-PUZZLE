
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CaseIntroController : MonoBehaviour
{
   
    [Serializable]    public class IntroDialogueLine
    {
        public string speaker;
        public string text;
        public int backgroundIndex = -1;
    }
    [Serializable]
    public class IntroDialogueData
    {
        public IntroDialogueLine[] dialogues;
    }
    [Serializable]
    public class ChapterSummary
    {
        public string chapterName;
        public string summary;
    }
    [Header("Chapter Intro JSON")]
    [SerializeField] private TextAsset introChapter1;
    [SerializeField] private TextAsset introChapter2;

    [Header("Chapter Summary JSON")]
    [SerializeField] private TextAsset summaryChapter1;
    [SerializeField] private TextAsset summaryChapter2;

    [Header("Background")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite[] backgrounds;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image characterImage;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button nextDialogueButton;

    [Header("Character Portraits")]
    [SerializeField] private Sprite chatGPTPortrait;
    [SerializeField] private Sprite giangPortrait;
    [SerializeField] private Sprite baoPortrait;
    [SerializeField] private Sprite chiPortrait;
    [SerializeField] private Sprite dungPortrait;
    [SerializeField] private Sprite haiPortrait;

    [Header("Case Summary UI")]
    [SerializeField] private GameObject caseSummaryPanel;
    [SerializeField] private TMP_Text caseSummaryText;

    [Header("Dialogue Animation")]
    [SerializeField] private RectTransform dialogueGroup;
    [SerializeField] private CanvasGroup dialogueGroupCanvas;
    [SerializeField] private float animationDuration = 0.4f;
    [SerializeField] private float slideDistance = 80f;


    private IntroDialogueData dialogueData;
    private ChapterSummary summaryData;
    private int dialogueIndex = 0;
    private int selectedChapter = 1;

    private void Start()
    {

        selectedChapter = GameManager.Instance.SelectedChapter;

        Debug.Log(
            "CaseIntro đang chạy Chapter: "
            + selectedChapter
        );


        LoadIntroData();

        if (dialogueData == null ||
            dialogueData.dialogues == null ||
            dialogueData.dialogues.Length == 0)
        {
            Debug.LogError(
                "Không có dữ liệu Intro cho Chapter "
                + selectedChapter
            );

            return;
        }


        LoadSummaryData();


        dialogueIndex = 0;


        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (caseSummaryPanel != null)
        {
            caseSummaryPanel.SetActive(false);
        }

        if (dialogueGroup != null)
        {
            dialogueGroupOriginalPosition =
                dialogueGroup.anchoredPosition;
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        ShowDialogue();
    }
    
    private void LoadIntroData()
    {
        TextAsset selectedIntroJson = null;


        // Chapter 1
        if (selectedChapter == 1)
        {
            selectedIntroJson = introChapter1;
        }

        // Chapter 2
        else if (selectedChapter == 2)
        {
            selectedIntroJson = introChapter2;
        }

        if (selectedIntroJson == null)
        {
            Debug.LogError(
                "Chưa gán Intro JSON cho Chapter "
                + selectedChapter
                + " trong Inspector!"
            );

            return;
        }

        dialogueData =
            JsonUtility.FromJson<IntroDialogueData>(
                selectedIntroJson.text
            );


        if (dialogueData == null ||
            dialogueData.dialogues == null)
        {
            Debug.LogError(
                "Intro JSON của Chapter "
                + selectedChapter
                + " không hợp lệ!"
            );

            return;
        }


        Debug.Log(
            "Đã load Intro JSON Chapter "
            + selectedChapter
        );
    }

    private void LoadSummaryData()
    {
        TextAsset selectedSummaryJson = null;


        // Chapter 1
        if (selectedChapter == 1)
        {
            selectedSummaryJson = summaryChapter1;
        }

        // Chapter 2
        else if (selectedChapter == 2)
        {
            selectedSummaryJson = summaryChapter2;
        }

        if (selectedSummaryJson == null)
        {
            Debug.LogError(
                "Chưa gán Summary JSON cho Chapter "
                + selectedChapter
                + " trong Inspector!"
            );

            return;
        }

        summaryData =
            JsonUtility.FromJson<ChapterSummary>(
                selectedSummaryJson.text
            );


        if (summaryData == null)
        {
            Debug.LogError(
                "Summary JSON của Chapter "
                + selectedChapter
                + " không hợp lệ!"
            );

            return;
        }


        Debug.Log(
            "Đã load Summary JSON Chapter "
            + selectedChapter
        );
    }

    public void ChangeBackground(int index)
    {
        if (backgroundImage == null)
        {
            Debug.LogWarning(
                "Chưa gán Background Image!"
            );

            return;
        }

        if (index < 0)
        {
            return;
        }

        if (backgrounds == null ||
            backgrounds.Length == 0)
        {
            Debug.LogWarning(
                "Chưa gán Backgrounds!"
            );

            return;
        }

        if (index >= backgrounds.Length)
        {
            Debug.LogWarning(
                "Background Index "
                + index
                + " không tồn tại. "
                + "Hiện chỉ có "
                + backgrounds.Length
                + " Background."
            );

            return;
        }

        if (backgrounds[index] == null)
        {
            Debug.LogWarning(
                "Background tại Element "
                + index
                + " chưa được gán!"
            );

            return;
        }

        backgroundImage.sprite =
            backgrounds[index];
    }

    public void OnNextDialogue()
    {
        if (dialogueData == null ||
            dialogueData.dialogues == null)
        {
            return;
        }


        dialogueIndex++;


        if (dialogueIndex >=
            dialogueData.dialogues.Length)
        {
            ShowCaseSummary();

            return;
        }

        ShowDialogue();
    }

    private void ShowDialogue()
    {
        if (dialogueData == null ||
            dialogueData.dialogues == null)
        {
            return;
        }


        if (dialogueIndex < 0 ||
            dialogueIndex >=
            dialogueData.dialogues.Length)
        {
            return;
        }


        IntroDialogueLine line =
            dialogueData.dialogues[dialogueIndex];



        if (dialogueText != null)
        {
            dialogueText.text = line.text;
        }

        if (line.backgroundIndex >= 0)
        {
            ChangeBackground(
                line.backgroundIndex
            );
            PlayDialogueGroupAnimation();
        }

        Sprite portrait = null;


        switch (line.speaker)
        {
            case "ChatGPT":

                portrait = chatGPTPortrait;

                break;


            case "Giang":

                portrait = giangPortrait;

                break;


            case "Bảo":

            case "Bao":

                portrait = baoPortrait;

                break;


            case "Chi":

                portrait = chiPortrait;

                break;


            case "Dũng":

            case "Dung":

                portrait = dungPortrait;

                break;


            case "Hải":

            case "Hai":

                portrait = haiPortrait;

                break;


            case "Người dẫn chuyện":

                portrait = null;

                break;


            default:

                Debug.LogWarning(
                    "Không tìm thấy Portrait cho speaker: "
                    + line.speaker
                );

                portrait = null;

                break;
        }

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
        PlayDialogueGroupAnimation();
    }

    private void ShowCaseSummary()
    {
        
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (caseSummaryPanel != null)
        {
            caseSummaryPanel.SetActive(true);
        }

        if (summaryData == null)
        {
            Debug.LogError(
                "Không có dữ liệu tóm tắt cho Chapter "
                + selectedChapter
            );

            return;
        }

        if (caseSummaryText != null)
        {
            caseSummaryText.text =
                summaryData.chapterName
                + "\n\n"
                + summaryData.summary;
        }
    }

    public void OnSkipDialogue()
    {
        ShowCaseSummary();
    }

    public void OnStartInvestigation()
    {
        GameManager.Instance.StartNewTimer();

        SceneManager.LoadScene("Investigation");
    }

    private Coroutine dialogueAnimationCoroutine;
    private Vector2 dialogueGroupOriginalPosition;
    private void PlayDialogueGroupAnimation()
    {
        if (dialogueGroup == null || dialogueGroupCanvas == null)
            return;

        if (dialogueAnimationCoroutine != null)
        {
            StopCoroutine(dialogueAnimationCoroutine);
        }

        dialogueAnimationCoroutine =
            StartCoroutine(AnimateDialogueGroup());
    }

    private IEnumerator AnimateDialogueGroup()
    {
        Vector2 endPosition = dialogueGroupOriginalPosition;

        Vector2 startPosition =
            endPosition + Vector2.down * slideDistance;

        dialogueGroup.anchoredPosition = startPosition;
        dialogueGroupCanvas.alpha = 0f;

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / animationDuration;

            t = Mathf.SmoothStep(0f, 1f, t);

            dialogueGroup.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    endPosition,
                    t
                );

            dialogueGroupCanvas.alpha = t;

            yield return null;
        }

        // Đảm bảo cuối animation luôn về đúng vị trí gốc
        dialogueGroup.anchoredPosition =
            dialogueGroupOriginalPosition;

        dialogueGroupCanvas.alpha = 1f;

        dialogueAnimationCoroutine = null;
    }

}
