using UnityEngine;
using UnityEngine.SceneManagement; // Sahne işlemleri için şart

public class PauseManager : MonoBehaviour
{
    [Header("UI Elementleri")]
    // Inspector'dan sürükleyeceğin Siyah Panel (Menü)
    [SerializeField] private GameObject _PausePanel; 

    // Diğer scriptlerden erişim için (TimerManager buradan okuyacak)
    public static bool GameIsPaused = false; 

    void Update()
    {
        // ESC tuşuna basılınca
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // --- OYUNU DEVAM ETTİR ---
    public void ResumeGame()
    {
        _PausePanel.SetActive(false); // Paneli kapat
        Time.timeScale = 1f;          // Zamanı normal akışına al
        GameIsPaused = false;
        
        // Fareyi kilitle ve gizle (Oyun moduna dön)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // --- OYUNU DURDUR ---
    void PauseGame()
    {
        _PausePanel.SetActive(true);  // Paneli aç
        Time.timeScale = 0f;          // ZAMANI DURDUR (Her şey donar)
        GameIsPaused = true;

        // Fareyi serbest bırak (Menüye tıklayabilmek için)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // --- YENİDEN BAŞLAT ---
    public void RestartLevel()
    {
        // 1. Önce zamanı düzelt, yoksa yeni sahne donuk başlar!
        Time.timeScale = 1f;
        GameIsPaused = false;

        // 2. TimerManager'a ulaşıp süreyi sıfırla (KRİTİK KISIM)
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.ResetTimer();
        }

        TimerManager.isinlanacakKonum = new Vector3(4.96f, -1.29f, 4.35f);

        // 3. Sahneyi Yeniden Yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- OYUNDAN ÇIK ---
    public void QuitGame()
    {
        Debug.Log("Oyundan Çıkılıyor...");
        Application.Quit();
    }

    public void LoadMainMenu()
    {
        // 1. Önce zamanı normale döndür (Çok Önemli!)
        // Bunu yapmazsan ana menüye geçsen bile oyun donuk kalır.
        Time.timeScale = 1f;

        // 2. Oyunun duraklatıldı değişkenini false yap (Hata önlemek için)
        // (Eğer static bir değişken kullanıyorsan, örneğin: GameIsPaused = false;)

        // 3. Ana Menü sahnesini yükle
        // "MainMenu" yerine senin sahnenin adı tam olarak neyse onu yaz.
        SceneManager.LoadScene("MainMenu");
    }
}