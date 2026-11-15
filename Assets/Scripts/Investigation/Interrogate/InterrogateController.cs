
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class InterrogateController : MonoBehaviour
{
    [Serializable]
    private class CharacterPortrait
    {
        public int chapterId;
        public string characterId;
        public Sprite portrait;
    }

    [Header("Các hệ thống")]
    [SerializeField] private ListCharacter listCharacter;
    [SerializeField] private PanelQues panelQues;

    [Header("RectTransform hai panel")]
    [SerializeField] private RectTransform listCharacterPanel;
    [SerializeField] private RectTransform questionPanel;

    [Header("Thông tin nhân vật")]
    [SerializeField] private TMP_Text characterText;
    [SerializeField] private Image characterImage;

    [Header("Các nút điều khiển")]
    [SerializeField] private Button btnOpenLC;
    [SerializeField] private Button btnCloseLC;
    [SerializeField] private Button btnOpenQues;
    [SerializeField] private Button btnCloseQues;

    [Header("Ảnh nhân vật theo chương")]
    [SerializeField]
    private List<CharacterPortrait> characterPortraits =
        new List<CharacterPortrait>();

    [Header("Nội dung mặc định")]
    [SerializeField]
    private string defaultAnswer =
        "Bạn có gì muốn hỏi tôi?";

    [SerializeField]
    private string deadCharacterAnswer =
        "Bạn chỉ có thể xem lại các câu đã từng hỏi trước khi nhân vật này bị sát hại";

    [Header("Hiệu ứng trượt")]
    [SerializeField] private float slideDuration = 0.3f;

    private Vector2 listOpenPosition;
    private Vector2 questionOpenPosition;

    private bool isListOpen = true;
    private bool isQuestionOpen = false;

    private Coroutine listSlideCoroutine;
    private Coroutine questionSlideCoroutine;

    private int currentChapterId;

    private void Start()
    {
        if (!CheckReferences())
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "[INTERROGATE] Không tìm thấy GameManager!"
            );
            return;
        }

        currentChapterId = GameManager.Instance.SelectedChapter;

        Canvas.ForceUpdateCanvases();

        listOpenPosition = listCharacterPanel.anchoredPosition;
        questionOpenPosition = questionPanel.anchoredPosition;

        listCharacterPanel.gameObject.SetActive(true);
        questionPanel.gameObject.SetActive(true);

        isListOpen = true;
        isQuestionOpen = false;

        questionPanel.anchoredPosition =
            GetQuestionClosedPosition();

        UpdateCharacterListButtons();

        // Đăng ký sự kiện trước khi tạo danh sách
        listCharacter.OnCharacterSelected += HandleCharacterSelected;

        // Khởi tạo dữ liệu câu hỏi
        panelQues.Initialize();

        // Initialize sẽ tự chọn nhân vật đầu tiên
        listCharacter.Initialize(currentChapterId);

        // Đăng ký nút
        btnOpenLC.onClick.AddListener(OpenCharacterList);
        btnCloseLC.onClick.AddListener(CloseCharacterList);
        btnOpenQues.onClick.AddListener(OpenQues);
        btnCloseQues.onClick.AddListener(CloseQues);

        Debug.Log(
            "[INTERROGATE] Khởi tạo hoàn tất. Chương: " +
            currentChapterId
        );
    }

    // Kiểm tra các đối tượng đã gán trong Inspector
    private bool CheckReferences()
    {
        if (listCharacter == null ||
            panelQues == null ||
            listCharacterPanel == null ||
            questionPanel == null ||
            characterText == null ||
            characterImage == null ||
            btnOpenLC == null ||
            btnCloseLC == null ||
            btnOpenQues == null ||
            btnCloseQues == null)
        {
            Debug.LogError(
                "[INTERROGATE] Có trường trong Inspector chưa được gán!"
            );
            return false;
        }

        return true;
    }

    // Xử lý khi chọn nhân vật
    private void HandleCharacterSelected(CaseCharacterInfo character)
    {
        if (character == null)
            return;

        // Hiển thị thông tin nhân vật
        if (character.isDead)
        {
            characterText.text = "Đã bị sát hại";

            // Gọi PanelQues cập nhật AnsewText
            panelQues.SetAnswerText(deadCharacterAnswer);
        }
        else
        {
            characterText.text = character.characterDescription;

            // Gọi PanelQues cập nhật AnsewText
            panelQues.SetAnswerText(defaultAnswer);
        }

        // Cập nhật ảnh nhân vật
        UpdateCharacterPortrait(
            character.characterId,
            character.isDead
        );

        // Cập nhật danh sách câu hỏi
        panelQues.SelectCharacter(
            character.characterId,
            character.isDead
        );
    }

    // Tìm ảnh theo chương và mã nhân vật
    private void UpdateCharacterPortrait(
        string characterId,
        bool isDead)
    {
        Sprite selectedPortrait = null;

        foreach (CharacterPortrait portrait in characterPortraits)
        {
            if (portrait == null)
                continue;

            if (portrait.chapterId == currentChapterId &&
                string.Equals(
                    portrait.characterId,
                    characterId,
                    StringComparison.OrdinalIgnoreCase))
            {
                selectedPortrait = portrait.portrait;
                break;
            }
        }

        characterImage.sprite = selectedPortrait;
        characterImage.preserveAspect = true;

        // Ảnh nhân vật đã mất chuyển sang màu tối
        characterImage.color = isDead
            ? new Color(0.35f, 0.35f, 0.35f, 1f)
            : Color.white;

        if (selectedPortrait == null)
        {
            Debug.LogWarning(
                "[INTERROGATE] Chưa gán ảnh cho nhân vật " +
                characterId +
                " ở chương " +
                currentChapterId
            );
        }
    }

    // Mở danh sách nhân vật
    public void OpenCharacterList()
    {
        if (isListOpen)
            return;

        isListOpen = true;
        UpdateCharacterListButtons();

        StopSlide(ref listSlideCoroutine);

        listSlideCoroutine = StartCoroutine(
            SlidePanel(listCharacterPanel, listOpenPosition)
        );
    }

    // Đóng danh sách nhân vật
    public void CloseCharacterList()
    {
        if (!isListOpen)
            return;

        isListOpen = false;
        UpdateCharacterListButtons();

        StopSlide(ref listSlideCoroutine);

        listSlideCoroutine = StartCoroutine(
            SlidePanel(
                listCharacterPanel,
                GetListClosedPosition()
            )
        );
    }

    // Cập nhật trạng thái hai nút danh sách
    private void UpdateCharacterListButtons()
    {
        btnOpenLC.gameObject.SetActive(!isListOpen);
        btnCloseLC.gameObject.SetActive(isListOpen);
    }

    // Mở bảng câu hỏi
    public void OpenQues()
    {
        if (isQuestionOpen)
            return;

        if (!panelQues.HasSelectedCharacter)
        {
            Debug.LogWarning(
                "[INTERROGATE] Hãy chọn nhân vật trước khi mở câu hỏi!"
            );
            return;
        }

        isQuestionOpen = true;

        StopSlide(ref questionSlideCoroutine);

        questionSlideCoroutine = StartCoroutine(
            SlidePanel(questionPanel, questionOpenPosition)
        );
    }

    // Đóng bảng câu hỏi
    public void CloseQues()
    {
        if (!isQuestionOpen)
            return;

        isQuestionOpen = false;

        StopSlide(ref questionSlideCoroutine);

        questionSlideCoroutine = StartCoroutine(
            SlidePanel(
                questionPanel,
                GetQuestionClosedPosition()
            )
        );
    }

    // Vị trí đóng của danh sách: dịch sang trái
    private Vector2 GetListClosedPosition()
    {
        return listOpenPosition +
               Vector2.left * listCharacterPanel.rect.width;
    }

    // Vị trí đóng của bảng câu hỏi: dịch sang phải
    private Vector2 GetQuestionClosedPosition()
    {
        return questionOpenPosition +
               Vector2.right * questionPanel.rect.width;
    }

    // Dừng hiệu ứng trượt cũ
    private void StopSlide(ref Coroutine routine)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    // Hiệu ứng trượt dùng chung
    private IEnumerator SlidePanel(
        RectTransform panel,
        Vector2 targetPosition)
    {
        if (panel == null)
            yield break;

        Vector2 startPosition = panel.anchoredPosition;

        float elapsedTime = 0f;
        float duration = Mathf.Max(0.01f, slideDuration);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsedTime / duration);

            float smoothProgress =
                Mathf.SmoothStep(0f, 1f, progress);

            panel.anchoredPosition = Vector2.Lerp(
                startPosition,
                targetPosition,
                smoothProgress
            );

            yield return null;
        }

        panel.anchoredPosition = targetPosition;

        if (panel == listCharacterPanel)
            listSlideCoroutine = null;
        else if (panel == questionPanel)
            questionSlideCoroutine = null;
    }

    // Quay lại màn hình điều tra
    public void OnBack()
    {
        SceneManager.LoadScene("Investigation");
    }

    private void OnDestroy()
    {
        if (listCharacter != null)
            listCharacter.OnCharacterSelected -= HandleCharacterSelected;

        if (btnOpenLC != null)
            btnOpenLC.onClick.RemoveListener(OpenCharacterList);

        if (btnCloseLC != null)
            btnCloseLC.onClick.RemoveListener(CloseCharacterList);

        if (btnOpenQues != null)
            btnOpenQues.onClick.RemoveListener(OpenQues);

        if (btnCloseQues != null)
            btnCloseQues.onClick.RemoveListener(CloseQues);
    }
}
