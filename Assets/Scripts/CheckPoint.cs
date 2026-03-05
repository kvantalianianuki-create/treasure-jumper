using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    public static Vector2 savedPosition;
    public static bool hasCheckpoint = false;
    public static int savedSceneBuildIndex = -1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        savedPosition = (Vector2)transform.position + Vector2.up * 1f;
        hasCheckpoint = true;
        savedSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;

        Debug.Log("Checkpoint activated in scene " + savedSceneBuildIndex + " at " + savedPosition);
    }

    public static bool IsCheckpointValidForCurrentScene()
    {
        return hasCheckpoint && savedSceneBuildIndex == SceneManager.GetActiveScene().buildIndex;
    }

    public static void Clear()
    {
        hasCheckpoint = false;
        savedSceneBuildIndex = -1;
        savedPosition = Vector2.zero;
    }
}