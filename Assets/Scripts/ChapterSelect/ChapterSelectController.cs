using UnityEngine;
using UnityEngine.SceneManagement;

public class ChapterSelectController : MonoBehaviour
{
    public void OnBack() { SceneManager.LoadScene("MainMenu"); }
    public void OnSelectChapter1() { StartChapter(1); }
    public void OnSelectChapter2() { StartChapter(2); }

    private void StartChapter(int chapter)
    {
        if (GameManager.Instance == null) { Debug.LogError("Không tìm thấy GameManager."); return; }
        // Chỉ New Game mới xóa Save và dữ liệu phiên cũ.
        GameManager.Instance.BeginNewGame(chapter);
        SceneManager.LoadScene("CaseIntro");
    }
}
