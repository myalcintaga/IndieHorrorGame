using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance;
    public float gecenSure = 0f;

    // MonsterAI ışınlanma verisi (Aynı kalıyor)
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

        // Işınlanma mantığı (Aynı kalıyor)
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
    }

    // --- YENİ EKLENEN FONKSİYON: SÜREYİ SIFIRLA ---
    public void ResetTimer()
    {
        gecenSure = 0f;
    }

    void Update()
    {
        // EĞER OYUN DURAKLATILDIYSA SAYMAYI KES
        // (PauseManager namespace'i Sample ise, yukarıya using Sample; eklemeyi unutma)
        if (PauseManager.GameIsPaused) 
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