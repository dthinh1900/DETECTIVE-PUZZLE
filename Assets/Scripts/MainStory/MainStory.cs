using UnityEngine;
using UnityEngine.SceneManagement;

public class MainStory : MonoBehaviour
{
    [Header("Main Story")]
    public GameObject storyPanel;
    public GameObject endPanel;

    private void Start()
    {
        if (storyPanel != null)
            storyPanel.SetActive(true);

        if (endPanel != null)
            endPanel.SetActive(false);
    }

    // Gọi khi người chơi đã xem xong MainStory
    public void StartNextInvestigation()
    {
        GameManager.Instance.StartNewTimer();

        SceneManager.LoadScene("Investigation");
    }
}