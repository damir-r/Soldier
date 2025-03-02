using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void OnClick(string scene)
    {
        SceneManager.LoadSceneAsync(scene);
    }
}
