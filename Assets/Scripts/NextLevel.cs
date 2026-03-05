using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    public string nextLevelName;
    public int nextLevelValue;

    private bool loading;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (loading) return;
        if (!other.CompareTag("Player")) return;

        loading = true;
        LoadNextLevel();
    }

    public void LoadNextLevel()
    {
        PlayerPrefs.SetInt("LevelReached", nextLevelValue);
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextLevelName);
    }
}