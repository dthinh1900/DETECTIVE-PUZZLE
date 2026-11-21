using UnityEngine;
using UnityEngine.SceneManagement;

public class DeductionController : MonoBehaviour
{
    [Header("Màn hình kết quả")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    private bool isGameOver;

    private void Start()
    {
        isGameOver = false;

        if (winPanel != null)
            winPanel.SetActive(false);
        if (losePanel != null)
            losePanel.SetActive(false);

        // CipherController chuyển sang Scene Deduction khi đáp án mật mã đúng.
        if (GameManager.Instance != null && GameManager.Instance.IsCipherVictoryPending)
        {
            isGameOver = true;
            if (winPanel != null)
                winPanel.SetActive(true);
            else
                Debug.LogError("[DEDUCTION] Chưa gán Win Panel trong Inspector.");
        }
    }

    // Gắn vào lựa chọn suy luận sai.
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
            losePanel.SetActive(true);
        else
            Debug.LogError("[DEDUCTION] Chưa gán Lose Panel trong Inspector.");
    }

    public void BackToInvestigation()
    {
        if (isGameOver)
            return;

        SceneManager.LoadScene("Investigation");
    }
}
