
using UnityEngine;
using UnityEngine.SceneManagement;

public class InvestigationController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject panelSettings;

    private void Start()
    {
        if (panelSettings != null)
        {
            panelSettings.SetActive(false);
        }
    }

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

    public void OpenSettings()
    {
        if (panelSettings != null)
        {
            panelSettings.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        if (panelSettings != null)
        {
            panelSettings.SetActive(false);
        }
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
