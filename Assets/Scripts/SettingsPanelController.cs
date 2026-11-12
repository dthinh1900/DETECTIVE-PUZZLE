
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsPanelController : MonoBehaviour
{
    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;

    private void Start()
    {
        // Mặc định đóng bảng Settings
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    // Mở Settings
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    // Đóng Settings và tiếp tục chơi
    public void ResumeGame()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    // Quay về MainMenu
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
