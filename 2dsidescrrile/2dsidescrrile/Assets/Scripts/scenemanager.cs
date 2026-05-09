using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class scenemanager : MonoBehaviour
{
    public void Startgame()
    { 
       SceneManager.LoadSceneAsync(2);

    }
    public void Exitgame()
    {
        Application.Quit();
    }
}
