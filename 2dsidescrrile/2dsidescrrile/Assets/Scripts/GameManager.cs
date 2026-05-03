using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel; // Assign Pause UI Panel here
    private bool isPaused = false;

    void Start()
    {
        pausePanel.SetActive(false); // Hide at start
    }

    void Update()
    {
        // Press ESC to toggle pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // Resume time
        isPaused = false;
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // Freeze game
        isPaused = true;
    }

    public void MainMenu()
    {
        Time.timeScale = 1f; // Important before loading scene
        SceneManager.LoadScene("Main Menu"); // Change to your menu scene name
    }
}