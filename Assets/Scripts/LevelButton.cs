using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public int level;

    void Start()
    {
        Button btn = GetComponent<Button>();

        int levelReached = PlayerPrefs.GetInt("LevelReached", 1);

        if (level > levelReached)
        {
            btn.interactable = false;
        }
    }
}