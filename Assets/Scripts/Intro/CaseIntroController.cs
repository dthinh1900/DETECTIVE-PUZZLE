
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CaseIntroController : MonoBehaviour
{
    [Header("Dialogue Data")]
    [SerializeField] private TextAsset dialogueJson;

    [Header("Background")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite C1_N1_1;
    [SerializeField] private Sprite C1_N1_2;
    [SerializeField] private Sprite C1_N1_3;

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

    private DialogueData dialogueData;
    private int dialogueIndex = 0;

    private void Start()
    {
        LoadDialogueData();

        if (dialogueData == null ||
            dialogueData.dialogues == null ||
            dialogueData.dialogues.Length == 0)
        {
            Debug.LogError("Không có dữ liệu lời thoại!");
            return;
        }

        dialogueIndex = 0;

        dialoguePanel.SetActive(true);
        caseSummaryPanel.SetActive(false);

        // Giữ nguyên ảnh nền ban đầu.
        ChangeBackground(C1_N1_1);

        ShowDialogue();
    }

    // Đọc lời thoại từ JSON.
    private void LoadDialogueData()
    {
        if (dialogueJson == null)
        {
            Debug.LogError("Chưa gán file JSON trong Inspector!");
            return;
        }

        dialogueData =
            JsonUtility.FromJson<DialogueData>(dialogueJson.text);

        if (dialogueData == null ||
            dialogueData.dialogues == null)
        {
            Debug.LogError("Dữ liệu JSON không hợp lệ!");
        }
    }

    // Thay đổi ảnh nền, không chuyển ảnh thành chữ.
    public void ChangeBackground(Sprite newBackground)
    {
        if (backgroundImage == null)
        {
            Debug.LogWarning("Chưa gán Background Image!");
            return;
        }

        if (newBackground == null)
        {
            Debug.LogWarning("Ảnh nền chưa được gán!");
            return;
        }

        backgroundImage.sprite = newBackground;
    }

    // Chuyển sang lời thoại tiếp theo.
    public void OnNextDialogue()
    {
        if (dialogueData == null ||
            dialogueData.dialogues == null)
        {
            return;
        }

        dialogueIndex++;

        if (dialogueIndex >= dialogueData.dialogues.Length)
        {
            ShowCaseSummary();
            return;
        }

        ShowDialogue();
    }

    // Hiển thị lời thoại và ảnh đại diện.
    private void ShowDialogue()
    {
        DialogueLine line =
            dialogueData.dialogues[dialogueIndex];

        // Hiển thị nội dung lấy từ JSON.
        dialogueText.text = line.text;

        // Giữ nguyên cách đổi ảnh nền theo vị trí thoại.
        if (dialogueIndex == 3)
        {
            ChangeBackground(C1_N1_2);
        }
        else if (dialogueIndex == 13)
        {
            ChangeBackground(C1_N1_3);
        }

        // Chọn ảnh đại diện theo tên người nói trong JSON.
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
                portrait = baoPortrait;
                break;

            case "Chi":
                portrait = chiPortrait;
                break;

            case "Dũng":
                portrait = dungPortrait;
                break;

            case "Hải":
                portrait = haiPortrait;
                break;

            case "Người dẫn chuyện":
                portrait = null;
                break;
        }

        // Giữ ảnh nhân vật; chỉ ẩn khi không có ảnh đại diện.
        if (characterImage != null)
        {
            characterImage.sprite = portrait;
            characterImage.gameObject.SetActive(portrait != null);
        }

        if (nextDialogueButton != null)
        {
            nextDialogueButton.gameObject.SetActive(true);
        }
    }

    // Hiển thị tóm tắt vụ án.
    private void ShowCaseSummary()
    {
        dialoguePanel.SetActive(false);
        caseSummaryPanel.SetActive(true);

        caseSummaryText.text =
            "TÓM TẮT VỤ ÁN\n\n" +
            "Nhân vật chính: ChatGPT, một thám tử gặp sự cố xe giữa đường rừng.\n" +
            "Sau khi biết cây cầu gần đó đã sập, ChatGPT được Giang đưa về nhà nghỉ để tránh bão.\n" +
            "Tại đây có Giang, Bảo, Chi, Dũng, Hải và ChatGPT. Cây cầu sập khiến cả nhóm tạm thời không thể ra khỏi khu rừng, nhưng thức ăn trong tủ lạnh vẫn đủ cho một ngày.\n" +
            "Bảo rủ mọi người vào rừng đi săn. Sau chuyến đi săn, cơn mưa ập tới khiến tất cả mọi người quay lại nhà nghỉ. Khi về nhà nghỉ, mọi người nhận được tin nhắn ẩn danh cùng một mật thư xuất hiện ngay trước cửa nhà.\n" +
            "Mọi người cho rằng đó chỉ là trò đùa rồi về phòng tắm rửa, thay đồ.\n" +
            "HỒI 1: BẮT ĐẦU.";
    }

    // Bắt đầu Hồi 1.
    public void OnStartInvestigation()
    {
        Debug.Log("ChatGPT bắt đầu Hồi 1.");

        SceneManager.LoadScene("Investigation");
    }
    // Bỏ qua toàn bộ lời thoại và mở tóm tắt vụ án.
    public void OnSkipDialogue()
    {
        ShowCaseSummary();
    }
}
