using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
       
    /// <summary>
    /// Button callback to start a new game. Loads the NewGame scene
    /// </summary>
    public void StartNewGame()
    {
        SceneManager.LoadScene("NewGame");
    }

    /// <summary>
    /// Button callback to quit game to desktop
    /// </summary>
    public void QuitToDesktop()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

}
