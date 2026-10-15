
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChapterSelectController : MonoBehaviour
{
    public void OnBack()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnSelectChapter1()
    {
        Debug.Log("Đã chọn Chương 1");
        // Chuyển sang Intro vụ án ở Ngày 16.
    }

    public void OnSelectChapter2()
    {
        Debug.Log("Đã chọn Chương 2");
        // Chưa xử lý mở khóa và tiến độ ở bước này.
    }
}
