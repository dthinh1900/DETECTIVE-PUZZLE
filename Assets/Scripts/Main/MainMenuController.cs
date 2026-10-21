using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject settingPanel;
    public GameObject collectionPanel;
    public GameObject guidePanel;
    public void OnNewGame()
    {
        SceneManager.LoadScene("ChapterSelect");
    }
    public void OnContinue() {          settingPanel.SetActive(true);}
    public void OnSettings() {          settingPanel.SetActive(true);    }
    public void OnHowToPlay() {         guidePanel.SetActive(true); }
    public void OnCollection() {        collectionPanel.SetActive(true); }

    public void OffPanel() {       
        settingPanel.SetActive(false);
        guidePanel.SetActive(false);
        collectionPanel.SetActive(false);
    }
    public void OnExit()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}