using UnityEngine;
using UnityEngine.SceneManagement; // Sahne geçişleri için bu kütüphane ŞART.

public class MainMenuManager : MonoBehaviour
{
    // OYUNA BAŞLA BUTONU İÇİN
    public void PlayGame()
    {
        // 1. Hafızadaki konumu Başlangıç Koordinatlarına sıfırla
        
        TimerManager.isinlanacakKonum = new Vector3(4.96f, -1.29f, 4.35f);

        
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.ResetTimer();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    // ÇIKIŞ BUTONU İÇİN
    public void OpenLeaderboard()
    {
        // Tırnak içindeki ismin oluşturduğun sahne adıyla BİREBİR aynı olduğundan emin ol!
        SceneManager.LoadScene("HighScoreScene");
    }
    public void QuitGame()
    {
        Debug.Log("Oyundan çıkılıyor..."); // Editörde çalıştığını görmek için.
        Application.Quit(); // Gerçek oyunda (.exe) uygulamayı kapatır.
    }
}