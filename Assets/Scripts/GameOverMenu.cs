using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    public void ReturnToStart()
    {
        if (GameHandler.Instance != null)
        {
            Destroy(GameHandler.Instance.gameObject);
        }
        SceneManager.LoadScene("Menu");
    }
}
