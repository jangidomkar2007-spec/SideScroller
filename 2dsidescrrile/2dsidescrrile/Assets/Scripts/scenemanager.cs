using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class scenemanager : MonoBehaviour
{
    public void Startgame()
    { 
       SceneManager.LoadSceneAsync(1);

    }
    public void Exitgame()
    {
        Application.Quit();
    }
}
