using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoSceneLoader : MonoBehaviour
{
    public string[] sceneNames;

    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Escape))
            Application.Quit();

        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                LoadSceneByIndex(i - 1);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            LoadSceneByIndex(9);
        }
    }

    void LoadSceneByIndex(int index)
    {
        if (index < 0 || index >= sceneNames.Length)
        {
            Debug.LogWarning($"No scene assigned for index {index}");
            return;
        }

        SceneManager.LoadScene(sceneNames[index]);
    }
}
