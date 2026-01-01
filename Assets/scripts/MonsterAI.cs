using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;
using Sample; // KidsScript'e ulaşmak için bunu ekledik

public class MonsterAI : MonoBehaviour
{
    [Header("Hedef ve Konum")]
    public Transform playerTarget;
    public Vector3 salonKapisiKonumu;

    [Header("Fade (Kararma) Ayarları")]
    public Image fadeImage;
    public float fadeDuration = 1.0f;

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

    private bool isScreaming = false;
    private bool isChasing = false;
    private bool isWaiting = false;
    private bool isGameEnding = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (playerTarget == null) Debug.LogError("Player Target atanmadı!");

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
        if (isGameEnding) return;

        if (anim != null && agent != null && agent.enabled)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }

        if (isScreaming) return;

        if (!isChasing && CanSeePlayer())
        {
            StartCoroutine(ScreamAndChaseSequence());
        }

        if (isChasing)
        {
            agent.isStopped = false;
            agent.SetDestination(playerTarget.position);
        }

        if (!isStationary && !isChasing && !isWaiting)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                StartCoroutine(WaitAndNextPoint());
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isGameEnding) return;

        if (other.CompareTag("Player"))
        {
            StartCoroutine(FadeAndResetSequence());
        }
    }

    IEnumerator FadeAndResetSequence()
    {
        isGameEnding = true;

        // --- OYUNCUYU YERE YIK (YENİ KISIM) ---
        // Oyuncunun üzerindeki KidsScript'i bul ve ForceFaint'i çağır
        KidsScript playerScript = playerTarget.GetComponent<KidsScript>();
        if (playerScript != null)
        {
            playerScript.ForceFaint();
        }

        // Canavarı Durdur
        if (agent != null) agent.isStopped = true;
        if (anim != null) anim.SetFloat("Speed", 0);

        // Ekranı Karart
        if (fadeImage != null)
        {
            fadeImage.CrossFadeAlpha(1f, fadeDuration, false);
        }

        yield return new WaitForSeconds(fadeDuration);

        audioSource.Stop();

        TimerManager.isinlanacakKonum = salonKapisiKonumu;

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    // --- DİĞER FONKSİYONLAR ---
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

        if (!isGameEnding)
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

        Vector3 directionToPlayer = (targetCenter - eyes.position).normalized;
        if (Vector3.Angle(eyes.forward, directionToPlayer) < visionAngle / 2)
        {
            if (distance < visionDistance)
            {
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

    private void OnDrawGizmosSelected()
    {
        if (eyes == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyes.position, visionDistance);
        Gizmos.color = Color.red;
        Vector3 leftBoundary = DirFromAngle(-visionAngle / 2, false);
        Vector3 rightBoundary = DirFromAngle(visionAngle / 2, false);
        Gizmos.DrawLine(eyes.position, eyes.position + leftBoundary * visionDistance);
        Gizmos.DrawLine(eyes.position, eyes.position + rightBoundary * visionDistance);
        if (isChasing || (playerTarget != null && CanSeePlayer()))
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(eyes.position, playerTarget.position + Vector3.up);
        }
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += eyes.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}