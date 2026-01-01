using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI; // UI işlemleri için gerekli
using System.Collections;

public class MonsterAI : MonoBehaviour
{
    [Header("Hedef ve Konum")]
    public Transform playerTarget;
    public Vector3 salonKapisiKonumu; // Oyuncunun doğacağı yer

    [Header("Fade (Kararma) Ayarları")]
    public Image fadeImage; // Editörden Siyah Resmi buraya sürükle
    public float fadeDuration = 1.0f; // Ekranın kararma süresi

    [Header("Ses Ayarları")]
    public AudioClip idleGrowlSFX;
    public AudioClip screamSFX;
    public AudioClip chaseMusic;
    private AudioSource audioSource;

    [Header("Davranış Ayarları")]
    public bool isStationary = false;

    [Header("Hız Ayarları")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 4.5f;

    [Header("Devriye Ayarları")]
    public Transform[] waypoints;
    public float waitAtPoint = 2.0f;

    [Header("Görüş Ayarları")]
    public float visionDistance = 10f;
    public float visionAngle = 60f;
    public Transform eyes;
    public float screamAnimDuration = 2.0f;

    private NavMeshAgent agent;
    private Animator anim;
    private int currentWaypointIndex = 0;

    // Durum Değişkenleri
    private bool isScreaming = false;
    private bool isChasing = false;
    private bool isWaiting = false;
    private bool isGameEnding = false; // Oyun bitiş kilidi

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (playerTarget == null) Debug.LogError("Player Target atanmadı!");

        // Ekranın başında siyahlığı kaldır (Şeffaf yap)
        if (fadeImage != null)
        {
            fadeImage.canvasRenderer.SetAlpha(0.0f);
        }

        PlayIdleSound();

        agent.speed = walkSpeed;

        if (isStationary) agent.isStopped = true;
        else MoveToNextWaypoint();
    }

    void Update()
    {
        if (playerTarget == null) return;

        // Oyun bitiyorsa (Fade başladıysa) hiçbir şey yapma
        if (isGameEnding) return;

        // 1. ANİMASYON HIZI
        if (anim != null && agent != null && agent.enabled)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }

        if (isScreaming) return;

        // 2. GÖRÜŞ
        if (!isChasing && CanSeePlayer())
        {
            StartCoroutine(ScreamAndChaseSequence());
        }

        // 3. KOVALAMA
        if (isChasing)
        {
            agent.isStopped = false;
            agent.SetDestination(playerTarget.position);
        }

        // 4. DEVRİYE
        if (!isStationary && !isChasing && !isWaiting)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                StartCoroutine(WaitAndNextPoint());
            }
        }
    }

    // --- TEMAS HALİNDE FADE VE RESET ---
    void OnTriggerEnter(Collider other)
    {
        // Eğer zaten oyun bitiyorsa tekrar tetiklenme
        if (isGameEnding) return;

        if (other.CompareTag("Player"))
        {
            StartCoroutine(FadeAndResetSequence());
        }
    }

    IEnumerator FadeAndResetSequence()
    {
        isGameEnding = true; // Kilidi kapat

        // Canavarı ve Müziği Durdur
        if (agent != null) agent.isStopped = true;
        if (anim != null) anim.SetFloat("Speed", 0);
        audioSource.Stop();

        // Ekranı Karart (Fade Out)
        if (fadeImage != null)
        {
            fadeImage.CrossFadeAlpha(1f, fadeDuration, false);
        }

        // Kararma süresi kadar bekle
        yield return new WaitForSeconds(fadeDuration);

        // 1. TimerManager'a ışınlanma emrini ver
        TimerManager.isinlanacakKonum = salonKapisiKonumu;

        // 2. Sahneyi Yenile
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    // --- DİĞER FONKSİYONLAR (AYNI KALDI) ---

    IEnumerator ScreamAndChaseSequence()
    {
        isScreaming = true;
        isWaiting = false;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();
        transform.LookAt(new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z));

        anim.SetTrigger("foundPlayer");
        audioSource.Stop();
        if (screamSFX != null) audioSource.PlayOneShot(screamSFX);

        yield return new WaitForSeconds(screamAnimDuration);

        if (!isGameEnding) // Fade girdiyse müziği başlatma
        {
            if (chaseMusic != null)
            {
                audioSource.clip = chaseMusic;
                audioSource.loop = true;
                audioSource.spatialBlend = 0.0f;
                audioSource.Play();
            }

            isScreaming = false;
            isChasing = true;
            agent.speed = runSpeed;
            agent.isStopped = false;
            anim.SetBool("isChasing", true);
        }
    }

    IEnumerator WaitAndNextPoint()
    {
        isWaiting = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(waitAtPoint);

        if (isChasing || isScreaming || isGameEnding) yield break;

        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        MoveToNextWaypoint();
        isWaiting = false;
    }

    void MoveToNextWaypoint()
    {
        if (waypoints.Length == 0) return;
        agent.isStopped = false;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    bool CanSeePlayer()
    {
        if (playerTarget == null) return false;

        Vector3 targetCenter = playerTarget.position + Vector3.up * 1.0f;
        float distance = Vector3.Distance(eyes.position, targetCenter);

        if (distance < 4f) return true;
        // -------------------------------------------

        Vector3 directionToPlayer = (targetCenter - eyes.position).normalized;

        // Açı kontrolü
        if (Vector3.Angle(eyes.forward, directionToPlayer) < visionAngle / 2)
        {
            // Mesafe kontrolü
            if (distance < visionDistance)
            {
                // Engel kontrolü
                if (Physics.Raycast(eyes.position, directionToPlayer, out RaycastHit hit, visionDistance))
                {
                    if (hit.transform.root == playerTarget.root) return true;
                }
            }
        }
        return false;
    }

    void PlayIdleSound()
    {
        if (audioSource == null) return;
        audioSource.Stop();
        audioSource.clip = idleGrowlSFX;
        audioSource.loop = true;
        audioSource.spatialBlend = 1.0f;
        audioSource.Play();
    }
    // --- GÖRSEL DEBUG (GIZMOS) ---
    // Bu kod oyunun çalışmasına etki etmez, sadece Scene ekranında çizim yapar.
    private void OnDrawGizmosSelected()
    {
        if (eyes == null) return;

        // 1. Görüş Mesafesi (Sarı Çember)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyes.position, visionDistance);

        // 2. Görüş Açısı (Kırmızı Çizgiler)
        Gizmos.color = Color.red;

        // Açının sağ ve sol sınırlarını hesapla
        Vector3 leftBoundary = DirFromAngle(-visionAngle / 2, false);
        Vector3 rightBoundary = DirFromAngle(visionAngle / 2, false);

        // Çizgileri çiz
        Gizmos.DrawLine(eyes.position, eyes.position + leftBoundary * visionDistance);
        Gizmos.DrawLine(eyes.position, eyes.position + rightBoundary * visionDistance);

        // 3. Eğer oyuncu görüş alanındaysa Mavi çizgi çek
        if (isChasing || (playerTarget != null && CanSeePlayer()))
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(eyes.position, playerTarget.position + Vector3.up);
        }
    }

    // Açıyı (Derece) Vektöre çeviren matematiksel yardımcı fonksiyon
    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            // Canavarın kendi dönüş açısına (Y ekseni) göre hesapla
            angleInDegrees += eyes.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}