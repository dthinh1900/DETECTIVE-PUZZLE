using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void OnNewGame()
    {
        SceneManager.LoadScene("ChapterSelect");
    }
    public void OnContinue() { /* làm ở Phase Save/Load */ }
    public void OnSettings() { /* mở panel Cài đặt, làm sau */ }
    public void OnHowToPlay() { /* mở panel Hướng dẫn, làm sau */ }
    public void OnCollection() { /* mở panel Sưu tầm, làm ở Phase 11 */ }

    public void OnExit()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}