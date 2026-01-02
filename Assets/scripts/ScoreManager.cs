using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq; // Sıralama yapmak için gerekli

public class ScoreManager : MonoBehaviour
{
    [Header("BU KISIM SENİN İÇİN (Rekor Sahnesi)")]
    public TMP_Text scoreListDisplay;   // Tahtadaki yazı alanı

    // --- BU FONKSİYONU ARKADAŞIN KULLANACAK ---
    // Arkadaşın "Kaydet" butonuna bastığında bu fonksiyonu çağıracak.
    // O sana sadece ismi ve süreyi gönderecek, gerisini bu kod halledecek.
    public void SaveNewScore(string playerName, float timeVal)
    {
        // İsim boşsa "Bilinmeyen" yap
        if (string.IsNullOrEmpty(playerName)) playerName = "Bilinmeyen";

        // Veriyi hazırla: "Ahmet|120" formatında
        string newEntry = playerName + "|" + timeVal;

        // Eski kayıtları hafızadan çek
        string currentSave = PlayerPrefs.GetString("HighScores", "");
        
        // Yeni kaydı eskilerin yanına ekle (Araya # koyarak)
        if(currentSave.Length > 0)
            currentSave += "#" + newEntry;
        else
            currentSave = newEntry;

        // Hepsini tekrar kaydet
        PlayerPrefs.SetString("HighScores", currentSave);
        PlayerPrefs.Save();

        // Kayıt bitince otomatik olarak Ana Menüye dön
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu"); 
    }

    // --- BU KISIM SENİN İÇİN (LİSTELEME) ---
    void Start()
    {
        // Eğer bu script Rekor Sahnesindeyse (Kutu bağlıysa) çalışır
        if (scoreListDisplay != null)
        {
            ShowHighScores();
        }
    }

    void ShowHighScores()
    {
        string saveData = PlayerPrefs.GetString("HighScores", "");
        
        // Hiç kayıt yoksa
        if (string.IsNullOrEmpty(saveData))
        {
            scoreListDisplay.text = "Henüz kaçabilen olmadı...";
            return;
        }

        // 1. Veriyi parçala (# işaretlerinden ayır)
        string[] entries = saveData.Split('#');
        List<ScoreEntry> scoreList = new List<ScoreEntry>();

        // 2. Her parçayı İsim ve Süre olarak ayır (| işaretinden)
        foreach (string entry in entries)
        {
            string[] data = entry.Split('|');
            if (data.Length == 2)
            {
                if (float.TryParse(data[1], out float timeVal))
                {
                     scoreList.Add(new ScoreEntry { name = data[0], time = timeVal });
                }
            }
        }

        // 3. SÜREYE GÖRE SIRALA (En kısa süre en üstte)
        scoreList = scoreList.OrderBy(x => x.time).ToList();

        // 4. Ekrana Yazdır (İlk 10 Kişi)
        scoreListDisplay.text = "";
        for (int i = 0; i < scoreList.Count && i < 6; i++)
        {
            float m = Mathf.FloorToInt(scoreList[i].time / 60);
            float s = Mathf.FloorToInt(scoreList[i].time % 60);
            
            // Format: "1. AHMET - 04:32"
            scoreListDisplay.text += (i + 1) + ". " + scoreList[i].name.ToUpper() + " - " + string.Format("{0:00}:{1:00}", m, s) + "\n";
        }
    }

    // Basit veri tutucu sınıf
    class ScoreEntry { public string name; public float time; }
    
    // Arkadaşının Ana Menüye dönmek için kullanabileceği basit fonksiyon
    public void GoToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
}