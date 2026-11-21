using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CipherController : MonoBehaviour
{
    [Header("Danh sách mật mã")]
    [SerializeField] private Transform cipherListContent;

    [Header("Chi tiết mật mã")]
    [SerializeField] private Image cipherPreviewImage;
    [SerializeField] private TMP_Text targetStatusText;

    [Header("Nhập đáp án - chỉ hiện cho mật mã hiện tại")]
    [SerializeField] private GameObject answerArea;
    [SerializeField] private TMP_InputField answerInput;
    [SerializeField] private Button checkAnswerButton;

    [Header("Thông báo khi nhập sai")]
    [SerializeField] private GameObject wrongAnswerPanel;
    [SerializeField] private Button continueAfterWrongButton;

    [Header("Tuỳ chỉnh danh sách")]
    [SerializeField] private float entryHeight = 82f;
    [SerializeField] private float entrySpacing = 8f;

    private List<CipherEntryData> branchCiphers = new List<CipherEntryData>();
    private CipherEntryData selectedCipher;
    private bool checkListenerAdded;
    private bool continueListenerAdded;

    private void Awake()
    {
        if (checkAnswerButton != null && !checkListenerAdded)
        {
            checkAnswerButton.onClick.AddListener(CheckAnswer);
            checkListenerAdded = true;
        }

        if (continueAfterWrongButton != null && !continueListenerAdded)
        {
            continueAfterWrongButton.onClick.AddListener(OnContinueAfterWrongAnswer);
            continueListenerAdded = true;
        }

        if (wrongAnswerPanel != null)
            wrongAnswerPanel.SetActive(false);
    }

    private void OnEnable()
    {
        RefreshPanel();
    }

    private void OnDestroy()
    {
        if (checkAnswerButton != null && checkListenerAdded)
            checkAnswerButton.onClick.RemoveListener(CheckAnswer);

        if (continueAfterWrongButton != null && continueListenerAdded)
            continueAfterWrongButton.onClick.RemoveListener(OnContinueAfterWrongAnswer);
    }

    public void RefreshPanel()
    {
        if (GameManager.Instance == null || GameManager.Instance.SelectedChapter != 1)
        {
            ShowEmptyState("Hệ thống mật mã hiện chỉ được cấu hình cho Chapter 1.");
            return;
        }

        if (!CaseSessionData.IsSessionStarted || string.IsNullOrWhiteSpace(CaseSessionData.SelectedBranchId))
        {
            ShowEmptyState("Chưa có phiên điều tra hoặc chưa xác định Branch.");
            return;
        }

        branchCiphers = CipherDataLoader.GetCiphersForBranch(CaseSessionData.SelectedBranchId);
        List<CipherEntryData> collected = new List<CipherEntryData>();

        foreach (CipherEntryData cipher in branchCiphers)
        {
            if (cipher != null && GameSessionData.IsCipherCollected(cipher.cipherId))
                collected.Add(cipher);
        }

        BuildCipherList(collected);

        if (collected.Count == 0)
        {
            selectedCipher = null;
            ShowEmptyState("Bạn chưa thu thập được mật mã nào.");
            return;
        }

        string preferredId = GameSessionData.ActiveCipherId;
        CipherEntryData preferred = collected.Find(c => c.cipherId == preferredId);
        SelectCipher(preferred != null ? preferred.cipherId : collected[collected.Count - 1].cipherId);
    }

    private void BuildCipherList(List<CipherEntryData> collected)
    {
        if (cipherListContent == null)
        {
            Debug.LogError("[CIPHER UI] Chưa gán Cipher List Content trong Inspector.");
            return;
        }

        for (int i = cipherListContent.childCount - 1; i >= 0; i--)
            Destroy(cipherListContent.GetChild(i).gameObject);

        VerticalLayoutGroup layout = cipherListContent.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
            layout = cipherListContent.gameObject.AddComponent<VerticalLayoutGroup>();

        layout.childAlignment = TextAnchor.UpperCenter;
        layout.spacing = entrySpacing;
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = cipherListContent.GetComponent<ContentSizeFitter>();
        if (fitter == null)
            fitter = cipherListContent.gameObject.AddComponent<ContentSizeFitter>();

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        RectTransform contentRect = cipherListContent as RectTransform;
        if (contentRect != null)
        {
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, contentRect.sizeDelta.y);
        }

        ScrollRect scroll = cipherListContent.GetComponentInParent<ScrollRect>();
        if (scroll != null)
        {
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
        }

        foreach (CipherEntryData cipher in collected)
            CreateCipherEntryButton(cipher);
    }

    private void CreateCipherEntryButton(CipherEntryData cipher)
    {
        GameObject entry = new GameObject("CipherEntry_" + cipher.cipherId,
            typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        entry.transform.SetParent(cipherListContent, false);

        LayoutElement layout = entry.GetComponent<LayoutElement>();
        layout.minHeight = entryHeight;
        layout.preferredHeight = entryHeight;
        layout.flexibleHeight = 0f;

        Image background = entry.GetComponent<Image>();
        bool active = cipher.cipherId == GameSessionData.ActiveCipherId;
        background.color = active
            ? new Color(0.88f, 0.72f, 0.25f, 0.95f)
            : new Color(0.12f, 0.16f, 0.23f, 0.92f);

        Button button = entry.GetComponent<Button>();
        button.targetGraphic = background;
        button.transition = Selectable.Transition.ColorTint;
        button.onClick.AddListener(() => SelectCipher(cipher.cipherId));

        GameObject labelObject = new GameObject("Label",
            typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(entry.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(12f, 5f);
        labelRect.offsetMax = new Vector2(-12f, -5f);

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.fontSize = 20f;
        label.color = active ? new Color(0.08f, 0.08f, 0.08f, 1f) : Color.white;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.raycastTarget = false;
        label.text = BuildEntryLabel(cipher, active);
    }

    private string BuildEntryLabel(CipherEntryData cipher, bool active)
    {
        string label = string.IsNullOrWhiteSpace(cipher.title)
            ? "Mật mã " + (cipher.caseNumber + 1)
            : cipher.title;

        if (GameSessionData.IsCharacterDead(1, cipher.targetCharacterId))
            label += "\nMục tiêu: " + CipherDataLoader.GetCharacterDisplayName(cipher.targetCharacterId) + " - Đã bị sát hại";
        else if (GameSessionData.IsCipherSolved(cipher.cipherId))
            label += "\nĐã giải mã";
        else if (active)
            label += "\nMật mã hiện tại";

        return label;
    }

    public void SelectCipher(string cipherId)
    {
        selectedCipher = branchCiphers.Find(c => c != null && c.cipherId == cipherId);
        if (selectedCipher == null || !GameSessionData.IsCipherCollected(cipherId))
        {
            ShowEmptyState("Không tìm thấy mật mã đã thu thập.");
            return;
        }

        Sprite sprite = CipherDataLoader.LoadCipherImage(selectedCipher);
        if (cipherPreviewImage != null)
        {
            cipherPreviewImage.sprite = sprite;
            cipherPreviewImage.preserveAspect = true;
            cipherPreviewImage.gameObject.SetActive(sprite != null);
        }

        bool targetDead = GameSessionData.IsCharacterDead(1, selectedCipher.targetCharacterId);
        bool solved = GameSessionData.IsCipherSolved(selectedCipher.cipherId);
        bool active = string.Equals(selectedCipher.cipherId, GameSessionData.ActiveCipherId,
            StringComparison.OrdinalIgnoreCase);

        if (targetStatusText != null)
        {
            string targetName = CipherDataLoader.GetCharacterDisplayName(selectedCipher.targetCharacterId);
            if (targetDead)
                targetStatusText.text = targetName + "\nĐÃ BỊ SÁT HẠI";
            else if (solved)
                targetStatusText.text = "Mục tiêu đã được xác định: " + targetName;
            else if (active)
                targetStatusText.text = "Mật mã hiện tại - hãy giải mã để xác định mục tiêu.";
            else
                targetStatusText.text = "Mật mã đã lưu trong hồ sơ.";
        }

        bool canAnswer = active && !solved && !targetDead;
        if (answerArea != null)
            answerArea.SetActive(canAnswer);
        if (answerInput != null)
            answerInput.text = string.Empty;

        RefreshEntryLabels();
    }

    private void RefreshEntryLabels()
    {
        if (cipherListContent == null)
            return;

        foreach (Transform child in cipherListContent)
        {
            TextMeshProUGUI label = child.GetComponentInChildren<TextMeshProUGUI>();
            if (label == null) continue;

            CipherEntryData cipher = branchCiphers.Find(c =>
                c != null && child.name == "CipherEntry_" + c.cipherId);
            if (cipher == null) continue;

            bool active = string.Equals(cipher.cipherId, GameSessionData.ActiveCipherId,
                StringComparison.OrdinalIgnoreCase);
            label.text = BuildEntryLabel(cipher, active);

            Image bg = child.GetComponent<Image>();
            if (bg != null)
                bg.color = active
                    ? new Color(0.88f, 0.72f, 0.25f, 0.95f)
                    : new Color(0.12f, 0.16f, 0.23f, 0.92f);

            label.color = active ? new Color(0.08f, 0.08f, 0.08f, 1f) : Color.white;
        }
    }

    public void CheckAnswer()
    {
        if (selectedCipher == null ||
            !GameSessionData.IsCipherCollected(selectedCipher.cipherId))
        {
            Debug.LogWarning("[CIPHER] Hãy chọn một mật mã đã thu thập.");
            return;
        }

        if (!string.Equals(selectedCipher.cipherId, GameSessionData.ActiveCipherId,
                StringComparison.OrdinalIgnoreCase) ||
            GameSessionData.IsCipherSolved(selectedCipher.cipherId) ||
            GameSessionData.IsCharacterDead(1, selectedCipher.targetCharacterId))
        {
            Debug.LogWarning("[CIPHER] Mật mã này không còn là mật mã hiện tại.");
            RefreshPanel();
            return;
        }

        if (answerInput == null || string.IsNullOrWhiteSpace(answerInput.text))
        {
            Debug.LogWarning("[CIPHER] Bạn chưa nhập đáp án.");
            answerInput?.ActivateInputField();
            return;
        }

        List<string> possibleAnswers = new List<string>();
        if (!string.IsNullOrWhiteSpace(selectedCipher.answer))
            possibleAnswers.Add(selectedCipher.answer);

        if (selectedCipher.acceptedAnswers != null)
        {
            foreach (string answer in selectedCipher.acceptedAnswers)
            {
                if (!string.IsNullOrWhiteSpace(answer))
                    possibleAnswers.Add(answer);
            }
        }

        if (possibleAnswers.Count == 0)
        {
            Debug.LogError("[CIPHER] Chưa cấu hình answer/acceptedAnswers cho " +
                selectedCipher.cipherId + " trong C1_Cipher.json.");
            return;
        }

        string submitted = NormalizeAnswer(answerInput.text);
        bool correct = false;

        foreach (string answer in possibleAnswers)
        {
            if (submitted == NormalizeAnswer(answer))
            {
                correct = true;
                break;
            }
        }

        if (!correct)
        {
            HandleWrongAnswer();
            return;
        }

        if (!GameSessionData.MarkCipherSolved(selectedCipher.cipherId))
        {
            Debug.LogError("[CIPHER] Không thể lưu trạng thái giải mã đúng.");
            return;
        }

        Debug.Log("[CIPHER] Đáp án chính xác. Kết thúc thắng game ngay: " + selectedCipher.cipherId);

        if (GameManager.Instance != null)
            GameManager.Instance.WinImmediatelyFromCipher();
        else
            SceneManager.LoadScene("Deduction");
    }

    private void HandleWrongAnswer()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ApplyCipherWrongAnswerPenaltyAndPause();
        else
            Debug.LogError("[CIPHER] Không tìm thấy GameManager để giảm thời gian.");

        if (answerArea != null)
            answerArea.SetActive(false);

        if (wrongAnswerPanel != null)
        {
            wrongAnswerPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("[CIPHER] Chưa gán Panel_CipherWrong trong Inspector. Đồng hồ đã bị giảm một nửa và tạm dừng.");
        }
    }

    // Gắn tự động vào Button Tiếp tục của Panel_CipherWrong.
    public void OnContinueAfterWrongAnswer()
    {
        if (wrongAnswerPanel != null)
            wrongAnswerPanel.SetActive(false);

        RefreshPanel();

        if (GameManager.Instance != null)
            GameManager.Instance.ResumeTimerAfterCipherWrongAnswer();
    }

    private void ShowEmptyState(string message)
    {
        selectedCipher = null;

        if (cipherPreviewImage != null)
        {
            cipherPreviewImage.sprite = null;
            cipherPreviewImage.gameObject.SetActive(false);
        }

        if (targetStatusText != null)
            targetStatusText.text = message;

        if (answerArea != null)
            answerArea.SetActive(false);

        if (answerInput != null)
            answerInput.text = string.Empty;
    }

    private static string NormalizeAnswer(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        string decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        StringBuilder builder = new StringBuilder(decomposed.Length);

        foreach (char c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(c))
                builder.Append(char.ToUpperInvariant(c));
        }

        return builder.ToString();
    }
}
