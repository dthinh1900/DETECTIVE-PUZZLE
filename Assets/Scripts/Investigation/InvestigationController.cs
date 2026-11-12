
using UnityEngine;
using UnityEngine.SceneManagement;

public class InvestigationController : MonoBehaviour
{
    public void ShowExplore()
    {
        SceneManager.LoadScene("Explore");
    }

    public void ShowInterrogate()
    {
        SceneManager.LoadScene("Interrogate");
    }

    public void ShowStory()
    {
        SceneManager.LoadScene("Story");
    }

    public void ShowCaseFile()
    {
        SceneManager.LoadScene("CaseFile");
    }
}
