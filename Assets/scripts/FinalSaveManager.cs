using UnityEngine;
using TMPro; // TextMeshPro UI elementleri için gerekli

public class FinalSaveManager : MonoBehaviour
{
    [Header("UI Bağlantıları")]
    public TMP_InputField nameInputField; // Oyuncunun ismini yazdığı kutu
    public TMP_Text timeDisplay;          // "Süreniz: 12:45" yazacak metin alanı
    public ScoreManager scoreManager;     // Kayıt işlemini yapacak script (Aynı objede olabilir)

    private float finalTime; // O anki süreyi hafızada tutacak değişken

    void Start()
    {
        // 1. TimerManager'dan biriken süreyi güvenli bir şekilde çekiyoruz
        if (TimerManager.Instance != null)
        {
            finalTime = TimerManager.Instance.gecenSure;
        }
        else
        {
            // Eğer TimerManager yoksa (örneğin sadece bu sahneyi test ediyorsan) hata vermesin
            Debug.LogWarning("TimerManager bulunamadı! Süre 0 kabul edildi.");
            finalTime = 0f;
        }

        // 2. Süreyi Dakika:Saniye formatına çevirip ekrana yazdırıyoruz
        float minutes = Mathf.FloorToInt(finalTime / 60);
        float seconds = Mathf.FloorToInt(finalTime % 60);

        if (timeDisplay != null)
        {
            timeDisplay.text = string.Format("Tebrikler!\nKaçış Süreniz: {0:00}:{1:00}", minutes, seconds);
        }

        // 3. Mouse imlecini serbest bırak (İsim kutusuna tıklayabilmek için ŞART)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // --- BU FONKSİYONU 'KAYDET' BUTONUNA ATA ---
    public void SaveButton()
    {
        // Kutudaki yazıyı al
        string playerName = nameInputField.text;

        // Eğer oyuncu isim yazmadan butona basarsa varsayılan isim ata
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Player";
        }

        // ScoreManager scriptindeki kaydetme fonksiyonunu çağır
        // (ScoreManager, kaydettikten sonra otomatik olarak Ana Menüye dönecektir)
        if (scoreManager != null)
        {
            scoreManager.SaveNewScore(playerName, finalTime);
        }
        else
        {
            Debug.LogError("ScoreManager scripti atanmamış! Inspector'dan kontrol et.");
        }
    }
}