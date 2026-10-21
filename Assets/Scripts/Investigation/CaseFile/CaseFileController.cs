using UnityEngine;
using UnityEngine.SceneManagement;

public class CaseFileController : MonoBehaviour
{
    public void OnBack()
    {
        SceneManager.LoadScene("Investigation");
    }
}
