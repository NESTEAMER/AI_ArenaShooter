using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public GameObject contractPanel;
    public GameObject creditsPanel;
    private AudioSource audioSource;
    public AudioClip signContractSound;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowContract()
    {
        contractPanel.SetActive(true);
    }

    public void ShowCredits()
    {
        creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        creditsPanel.SetActive(false);
    }

    public void StartGame()
    {
        audioSource.PlayOneShot(signContractSound);
        SceneManager.LoadScene("Prologue");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
