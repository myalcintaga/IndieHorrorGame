using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance;
    public float gecenSure = 0f;

    // MonsterAI burayı dolduracak. Static olduğu için sahne değişse de silinmez.
    public static Vector3? isinlanacakKonum = null;

    private TextMeshProUGUI sureTexti;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Bu obje asla yok olmaz
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

    // Sahne Yüklendiğinde Otomatik Çalışır
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // UI'yı tekrar bul
        GameObject uiObj = GameObject.Find("TimeText");
        if (uiObj != null) sureTexti = uiObj.GetComponent<TextMeshProUGUI>();

        // EĞER MONSTERAI BİR KONUM BELİRLEDİYSE IŞINLA
        if (isinlanacakKonum.HasValue)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // CharacterController çakışma yapmasın diye kapatıp açıyoruz
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                player.transform.position = isinlanacakKonum.Value;

                if (cc != null) cc.enabled = true;

                // İstersen ışınlanma bilgisini silebilirsin, böylece normal resetlerde başlangıçta doğar
                // isinlanacakKonum = null; 
            }
        }
    }

    void Update()
    {
        gecenSure += Time.deltaTime;

        if (sureTexti != null)
        {
            float minutes = Mathf.FloorToInt(gecenSure / 60);
            float seconds = Mathf.FloorToInt(gecenSure % 60);
            sureTexti.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}