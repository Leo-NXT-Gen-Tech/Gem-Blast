using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
   
   public void OnGameStart()
    {
        SceneManager.LoadScene("Main Menu");
        Debug.Log("Main Menu Loaded");
    }
}
