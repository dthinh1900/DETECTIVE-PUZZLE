
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CaseIntroController : MonoBehaviour
{

    [Serializable]
    public class IntroDialogueLine
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

    [Header("Intro Cipher Reward UI - xuất hiện sau hội thoại Intro")]
    [SerializeField] private GameObject introCipherRewardPanel;
    [SerializeField] private Image introCipherRewardImage;
    [SerializeField] private TMP_Text introCipherRewardTitleText;
    [SerializeField] private TMP_Text introCipherRewardDescriptionText;

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
        if (GameManager.Instance == null)
        {
            Debug.LogError("[INTRO] Không tìm thấy GameManager.");
            return;
        }

        selectedChapter = GameManager.Instance.SelectedChapter;
        Debug.Log("CaseIntro đang chạy Chapter: " + selectedChapter);

        LoadIntroData();
        if (dialogueData == null || dialogueData.dialogues == null || dialogueData.dialogues.Length == 0)
        {
            Debug.LogError("Không có dữ liệu Intro cho Chapter " + selectedChapter);
            return;
        }

        LoadSummaryData();
        dialogueIndex = 0;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (caseSummaryPanel != null) caseSummaryPanel.SetActive(false);
        if (introCipherRewardPanel != null) introCipherRewardPanel.SetActive(false);

        if (dialogueGroup != null)
            dialogueGroupOriginalPosition = dialogueGroup.anchoredPosition;

        // Chapter 1: tự random Branch ngay khi mở Intro.
        // Branch và hung thủ được lưu trước hội thoại để mật mã Intro lấy đúng dữ liệu.
        // Nếu phiên chơi đã được khởi tạo (ví dụ quay lại/reload Intro), giữ nguyên Branch cũ.
        if (selectedChapter == 1 && !CaseSessionData.IsSessionStarted)
        {
            ChapterDataLoader loader = GameManager.Instance.GetComponent<ChapterDataLoader>();
            if (loader == null)
            {
                Debug.LogError("[INTRO] GameManager chưa có ChapterDataLoader.");
                return;
            }

            ChapterData chapter = loader.LoadCase(1, 1);
            if (chapter == null)
            {
                Debug.LogError("[INTRO] Không tải được dữ liệu Chapter 1 - Case 1 để random Branch.");
                return;
            }

            CaseSessionData.StartNewSession(chapter, 1);
            if (!CaseSessionData.IsSessionStarted ||
                string.IsNullOrWhiteSpace(CaseSessionData.SelectedBranchId))
            {
                Debug.LogError("[INTRO] Không thể random và khởi tạo Branch cho Chapter 1.");
                return;
            }

            Debug.Log("[INTRO] Đã random Branch khi mở Intro: " +
                CaseSessionData.SelectedBranchId +
                " | Hung thủ: " + CaseSessionData.SelectedKillerCharacterId);
        }

        if (selectedChapter == 1 &&
            (!CaseSessionData.IsSessionStarted ||
             CaseSessionData.CurrentCaseNumber != 1 ||
             string.IsNullOrWhiteSpace(CaseSessionData.SelectedBranchId)))
        {
            Debug.LogError("[INTRO] Trạng thái Branch Chapter 1 không hợp lệ.");
            return;
        }

        // Sau khi random (hoặc khôi phục) Branch, bắt đầu hội thoại Intro ngay.
        // Các Chapter khác tiếp tục dùng luồng Intro hiện tại.
        BeginIntroDialogue();
    }

    private void BeginIntroDialogue()
    {
        dialogueIndex = 0;
        if (caseSummaryPanel != null)
            caseSummaryPanel.SetActive(false);
        if (introCipherRewardPanel != null)
            introCipherRewardPanel.SetActive(false);
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

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
            ShowIntroCipherRewardPanel();

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

    private void ShowIntroCipherRewardPanel()
    {
        // Chapter 2 chưa có cấu hình mật mã; giữ nguyên luồng Intro hiện tại.
        if (selectedChapter != 1)
        {
            ShowCaseSummary();
            return;
        }

        if (!CaseSessionData.IsSessionStarted || string.IsNullOrWhiteSpace(CaseSessionData.SelectedBranchId))
        {
            Debug.LogError("[CIPHER INTRO] Branch chưa được random/lưu khi mở Intro.");
            return;
        }

        CipherEntryData cipher = CipherDataLoader.GetIntroCipher(CaseSessionData.SelectedBranchId);

        if (cipher == null)
        {
            Debug.LogError("[CIPHER INTRO] Không tải được mật mã đầu tiên cho Branch " + CaseSessionData.SelectedBranchId + ".");
            ShowCaseSummary();
            return;
        }

        // Ghi nhận ngay khi Panel phần thưởng xuất hiện, không chờ nút Tiếp tục.
        GameSessionData.CollectCipher(cipher.cipherId);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (caseSummaryPanel != null)
            caseSummaryPanel.SetActive(false);

        Sprite sprite = CipherDataLoader.LoadCipherImage(cipher);
        if (introCipherRewardImage != null)
        {
            introCipherRewardImage.sprite = sprite;
            introCipherRewardImage.preserveAspect = true;
            introCipherRewardImage.gameObject.SetActive(sprite != null);
        }

        if (introCipherRewardTitleText != null)
            introCipherRewardTitleText.text = "ĐÃ NHẶT ĐƯỢC MẬT MÃ ĐẦU TIÊN!";

        if (introCipherRewardDescriptionText != null)
            introCipherRewardDescriptionText.text = cipher.description;

        if (introCipherRewardPanel == null)
        {
            Debug.LogError("[CIPHER INTRO] Chưa gán Intro Cipher Reward Panel trong Inspector.");
            ShowCaseSummary();
            return;
        }

        introCipherRewardPanel.SetActive(true);
        Debug.Log("[CIPHER INTRO] Đã nhận mật mã đầu tiên: " + cipher.cipherId);
    }

    // Gắn vào nút TIẾP TỤC trên Intro Cipher Reward Panel.
    public void OnContinueFromIntroCipherReward()
    {
        if (introCipherRewardPanel != null)
            introCipherRewardPanel.SetActive(false);
        ShowCaseSummary();
    }

    private void ShowCaseSummary()
    {
        if (introCipherRewardPanel != null)
            introCipherRewardPanel.SetActive(false);

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
        // Skip chỉ bỏ qua lời thoại; vẫn phải nhận mật mã đầu tiên như luồng bình thường.
        ShowIntroCipherRewardPanel();
    }




    public void OnStartInvestigation()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("Không tìm thấy GameManager.");
            return;
        }

        int chapterNumber = GameManager.Instance.SelectedChapter;
        int caseNumber = 1;

        ChapterDataLoader loader = GameManager.Instance.GetComponent<ChapterDataLoader>();
        if (loader == null)
        {
            Debug.LogError("GameManager chưa có ChapterDataLoader.");
            return;
        }

        ChapterData chapter = loader.LoadCase(chapterNumber, caseNumber);
        if (chapter == null)
        {
            Debug.LogError("Không thể tải dữ liệu Vụ án 1.");
            return;
        }

        if (chapterNumber == 1)
        {
            // Chapter 1 đã random và lưu Branch ngay khi mở Intro.
            // Tuyệt đối không random lại khi bấm nút bắt đầu điều tra.
            if (!CaseSessionData.IsSessionStarted ||
                CaseSessionData.CurrentCaseNumber != 1 ||
                string.IsNullOrWhiteSpace(CaseSessionData.SelectedBranchId))
            {
                Debug.LogError("[INTRO] Không tìm thấy Branch đã random khi mở Intro Chapter 1.");
                return;
            }
        }
        else
        {
            // Chapter khác giữ nguyên cơ chế hiện có.
            CaseSessionData.StartNewSession(chapter, caseNumber);
            if (!CaseSessionData.IsSessionStarted)
            {
                Debug.LogError("Không thể khởi tạo phiên chơi.");
                return;
            }
        }

        GameManager.Instance.StartNewTimer();

        Debug.Log(
            $"Bắt đầu Chapter {chapterNumber} - Vụ án {CaseSessionData.CurrentCaseNumber} - " +
            $"Branch {CaseSessionData.SelectedBranchId} - Hung thủ {CaseSessionData.SelectedKillerCharacterId}"
        );

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
