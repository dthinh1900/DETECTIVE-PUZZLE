
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CaseFileController : MonoBehaviour
{
    [Header("Các Panel nội dung")]
    public GameObject panelEvidence;
    public GameObject panelStatements;
    public GameObject panelCipher;
    public GameObject panelTimeline;

    [Header("Các nút Tab")]
    public Button btnEvidence;
    public Button btnStatements;
    public Button btnCipher;
    public Button btnTimeline;

    private void Start()
    {
        // Gắn sự kiện cho các nút Tab
        btnEvidence.onClick.AddListener(ShowEvidence);
        btnStatements.onClick.AddListener(ShowStatements);
        btnCipher.onClick.AddListener(ShowCipher);
        btnTimeline.onClick.AddListener(ShowTimeline);

        // Mặc định mở Tab Vật chứng
        ShowEvidence();
    }

    // Hiển thị Tab Vật chứng
    public void ShowEvidence()
    {
        ShowPanel(panelEvidence);
    }

    // Hiển thị Tab Lời khai
    public void ShowStatements()
    {
        ShowPanel(panelStatements);
    }

    // Hiển thị Tab Mật mã
    public void ShowCipher()
    {
        ShowPanel(panelCipher);
    }

    // Hiển thị Tab Timeline
    public void ShowTimeline()
    {
        ShowPanel(panelTimeline);
    }

    // Chỉ hiển thị một Panel tại một thời điểm
    private void ShowPanel(GameObject selectedPanel)
    {
        panelEvidence.SetActive(selectedPanel == panelEvidence);
        panelStatements.SetActive(selectedPanel == panelStatements);
        panelCipher.SetActive(selectedPanel == panelCipher);
        panelTimeline.SetActive(selectedPanel == panelTimeline);
    }

    // Quay lại màn hình điều tra
    public void OnBack()
    {
        SceneManager.LoadScene("Investigation");
    }
}
