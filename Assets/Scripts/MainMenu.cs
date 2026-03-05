using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject startMainMenu;
    public GameObject levelSelect;

    public void StartGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void GoToLevelSelect()
    {
        if (startMainMenu != null) startMainMenu.SetActive(false);
        if (levelSelect != null) levelSelect.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}