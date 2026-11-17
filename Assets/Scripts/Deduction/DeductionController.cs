using UnityEngine;
using UnityEngine.SceneManagement;

public class DeductionController : MonoBehaviour
{
    [Header("Màn hình thua")]
    [SerializeField] private GameObject losePanel;

    private bool isGameOver;

    private void Start()
    {
        isGameOver = false;
        if (losePanel != null)
            losePanel.SetActive(false);
    }

    // Gắn hàm này vào lựa chọn suy luận sai (hoặc gọi từ hàm kiểm tra đáp án khi đáp án sai).
    // Không trừ thời gian. Người chơi thua ngay và không thể quay lại để thử tiếp.
    public void OnWrongDeductionAnswer()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        if (GameManager.Instance != null)
            GameManager.Instance.LoseImmediately();
        else
            Debug.LogError("[DEDUCTION] Không tìm thấy GameManager để kết thúc ván.");

        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }
        else
        {
            Debug.LogError("[DEDUCTION] Chưa gán Lose Panel trong Inspector. Ván đã được đánh dấu thua, nhưng chưa có panel để hiển thị.");
        }
    }

    public void BackToInvestigation()
    {
        if (isGameOver)
            return;

        SceneManager.LoadScene("Investigation");
    }
}
