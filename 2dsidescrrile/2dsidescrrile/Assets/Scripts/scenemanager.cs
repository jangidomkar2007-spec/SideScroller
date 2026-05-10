using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class scenemanager : MonoBehaviour
{
    public void Startgame()
    { 
       SceneTransition.instance.LoadScene("SampleScene");

    }
    public void Exitgame()
    {
        Application.Quit();
    }
}
