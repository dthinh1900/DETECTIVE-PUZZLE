using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject settingPanel;
    public GameObject collectionPanel;
    public GameObject guidePanel;

    public void OnNewGame() { SceneManager.LoadScene("ChapterSelect"); }

    public void OnContinue()
    {
        if (GameManager.Instance == null || !GameSaveManager.HasSave())
        {
            Debug.LogWarning("[CONTINUE] Chưa có dữ liệu để tiếp tục.");
            return;
        }
        if (!GameManager.Instance.ContinueGame()) Debug.LogError("[CONTINUE] Không thể khôi phục Save.");
    }

    public void OnSettings() { if (settingPanel != null) settingPanel.SetActive(true); }
    public void OnHowToPlay() { if (guidePanel != null) guidePanel.SetActive(true); }
    public void OnCollection() { if (collectionPanel != null) collectionPanel.SetActive(true); }
    public void OffPanel()
    {
        if (settingPanel != null) settingPanel.SetActive(false);
        if (guidePanel != null) guidePanel.SetActive(false);
        if (collectionPanel != null) collectionPanel.SetActive(false);
    }
    public void OnExit()
    {
        GameManager.Instance?.SaveGame();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
