using UnityEngine;
using UnityEngine.SceneManagement;

public class ExploreController : MonoBehaviour
{
    
    public void OnBack()
    {
        SceneManager.LoadScene("Investigation");
    }
}
