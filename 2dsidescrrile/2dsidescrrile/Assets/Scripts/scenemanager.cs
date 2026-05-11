using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class scenemanager : MonoBehaviour
{
    public void Startgame()
    { 
       SceneManager.LoadScene(2);

    }
    public void Exitgame()
    {
        Application.Quit();
    }
}
