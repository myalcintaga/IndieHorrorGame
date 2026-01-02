using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Sample; // PauseManager için gerekli

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance;
    public float gecenSure = 0f;

    // --- YENİ: Sayaç çalışıyor mu kontrolü ---
    public bool timerIsActive = true;

    // MonsterAI ışınlanma verisi
    public static Vector3? isinlanacakKonum = null;

    private TextMeshProUGUI sureTexti;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject uiObj = GameObject.Find("TimeText");
        if (uiObj != null) sureTexti = uiObj.GetComponent<TextMeshProUGUI>();

        if (isinlanacakKonum.HasValue)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                player.transform.position = isinlanacakKonum.Value;
                if (cc != null) cc.enabled = true;
            }
        }

        // --- YENİ: Ana Menüye dönüldüyse sayacı ve süreyi sıfırla ---
        // (Build Settings'de MainMenu indexi genelde 0'dır, kontrol et)
        if (scene.buildIndex == 0)
        {
            gecenSure = 0f;
            timerIsActive = true;
        }
    }

    public void ResetTimer()
    {
        gecenSure = 0f;
        timerIsActive = true;
    }

    // --- HATA VEREN FONKSİYON EKLENDİ ---
    // WakeUpManager artık bu fonksiyonu bulabilecek
    public static void SayaciDurdur()
    {
        if (Instance != null)
        {
            Instance.timerIsActive = false;
        }
    }
    // ------------------------------------

    void Update()
    {
        // Oyun duraklatıldıysa VEYA Sayaç durdurulduysa sayma
        if (PauseManager.GameIsPaused || !timerIsActive)
            return;

        gecenSure += Time.deltaTime;

        if (sureTexti != null)
        {
            float minutes = Mathf.FloorToInt(gecenSure / 60);
            float seconds = Mathf.FloorToInt(gecenSure % 60);
            sureTexti.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}