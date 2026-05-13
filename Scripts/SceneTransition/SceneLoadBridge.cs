using UnityEngine;

public class SceneLoadBridge : MonoBehaviour
{
    public void LoadLoginScene()
    {
        SceneTransitionManager.Instance?.LoadScene("LoginScene");
    }
}