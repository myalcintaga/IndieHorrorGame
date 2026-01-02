using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenu : MonoBehaviour
{
    // Bu fonksiyon butona basılınca çalışacak
    public void AnaMenuyeDon()
    {
        // Zaman donmuşsa düzelt
        Time.timeScale = 1; 
        
        // "MainMenu" senin ana menü sahnenin adı olmalı. 
        // Eğer farklıysa tırnak içini değiştir.
        SceneManager.LoadScene("MainMenu");
    }
}