using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void OnGameStart()
    {
        SceneManager.LoadScene("Game");
        Debug.Log("Game Scene Loaded");
    }
}