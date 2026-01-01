using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // YENİ: UI işlemleri için gerekli
using System.Collections; // YENİ: Zamanlayıcı (Coroutine) için gerekli

public class StalkerAI : MonoBehaviour
{
    [Header("Settings")]
    public Transform player;
    public float moveSpeed = 3.0f;
    public float startDelay = 5.0f; // Oyun başında kaç saniye beklesin?

    [Header("Game Over & VFX")]
    public float catchDistance = 1.0f; // Yakalanma mesafesi
    public Image fadeImage; // Siyah ekran resmi (Canvas'tan sürükleyeceğiz)
    public float fadeDuration = 1.5f; // Kararma kaç saniye sürsün?

    [Header("Vision")]
    [Range(0f, 1f)]
    public float visionAngleThreshold = 0.4f;

    private NavMeshAgent agent;
    private Camera mainCam;
    private Animator anim;
    private bool isGameOver = false; // Oyun bitti mi? (Tekrar tetiklenmeyi önler)

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        mainCam = Camera.main;

        if (player == null && mainCam != null)
        {
            player = mainCam.transform.root;
        }

        agent.speed = moveSpeed;
        agent.acceleration = 200f;

        SetLayerRecursively(gameObject, LayerMask.NameToLayer("Ignore Raycast"));

        // --- SİYAH EKRAN AYARI (GÜNCELLENDİ) ---
        if (fadeImage != null)
        {
            // 1. Önce rengini/alfasını tamamen şeffaf yap (Görünmez olsun)
            // Bu işlemi obje kapalıyken bile yapabiliriz.
            fadeImage.canvasRenderer.SetAlpha(0f);

            // 2. ŞİMDİ objeyi uyandır (SetActive true)
            // Önce şeffaflaştırıp sonra açtığımız için ekranda siyah pırpırlanma olmaz.
            fadeImage.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        // Eğer oyun bittiyse veya player yoksa hiçbir şey yapma
        if (player == null || isGameOver) return;

        // --- 1. BAŞLANGIÇ BEKLEMESİ ---
        // Oyun açılalı 5 saniye olmadıysa hareket etme
        if (Time.timeSinceLevelLoad < startDelay)
        {
            StopEnemy(); // Hareketsiz bekle
            return;
        }

        // --- 2. YAKALAMA KONTROLÜ ---
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer < catchDistance)
        {
            // Yakalandık! Kararma efektini başlat
            StartCoroutine(GameOverSequence());
            return;
        }

        // --- 3. WEEPING ANGEL MANTIĞI ---
        bool isVisible = IsVisibleToPlayer();

        if (isVisible)
        {
            // GÖRÜYORSA DUR
            StopEnemy();
            if (anim != null) anim.speed = 0f;
        }
        else
        {
            // GÖRMÜYORSA SALDIR
            MoveEnemy();
            if (anim != null)
            {
                anim.speed = 1f;
                anim.SetBool("isMoving", true);
            }
        }
    }

    // --- YARDIMCI FONKSİYONLAR ---

    void StopEnemy()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        if (anim != null) anim.SetBool("isMoving", false);
    }

    void MoveEnemy()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    // OYUN BİTİŞ SENARYOSU (COROUTINE)
    IEnumerator GameOverSequence()
    {
        isGameOver = true; // Kodu kilitle
        Debug.Log("YAKALANDIN! Saldırı başlıyor...");

        // 1. Fiziksel hareketi durdur
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // 2. KRİTİK ADIM: Animasyonun oynaması için zamanı normal hıza al!
        // Eğer bunu yapmazsak, saldırı animasyonu donmuş şekilde başlar.
        if (anim != null)
        {
            anim.speed = 1f;
            // 3. Animator'daki "AttackTrigger" tetiğini çek
            anim.SetTrigger("AttackTrigger");
        }

        // 4. Saldırının net görülmesi için çok kısa bir an bekle (İsteğe bağlı)
        // yield return new WaitForSeconds(0.2f); 

        // 5. Ekranı karart (Fade Out)
        if (fadeImage != null)
        {
            // Kararma süresini biraz kısaltabilirsin ki saldırı daha vurucu olsun
            fadeImage.CrossFadeAlpha(1f, fadeDuration, false);
        }

        // 6. Kararma bitene kadar bekle
        yield return new WaitForSeconds(fadeDuration);

        // 7. Sahneyi yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    bool IsVisibleToPlayer()
    {
        // Açı Kontrolü
        Vector3 directionToEnemy = (transform.position - mainCam.transform.position).normalized;
        float dotProduct = Vector3.Dot(mainCam.transform.forward, directionToEnemy);

        if (dotProduct < visionAngleThreshold) return false;

        // Raycast Kontrolü
        Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;
        Vector3 targetPosition = mainCam.transform.position;
        Vector3 directionToCamera = targetPosition - rayOrigin;
        float distance = Vector3.Distance(rayOrigin, targetPosition);

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, directionToCamera, out hit, distance))
        {
            if (hit.transform.root != player && hit.transform.gameObject != mainCam.gameObject)
            {
                return false; // Engel var
            }
        }
        return true; // Görüyor
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (newLayer < 0) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}