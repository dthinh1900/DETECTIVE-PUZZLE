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
        GameManager.Instance.SelectChapter(1);

        SceneManager.LoadScene("CaseIntro");
    }

    public void OnSelectChapter2()
    {
        GameManager.Instance.SelectChapter(2);

        SceneManager.LoadScene("CaseIntro");
    }
}