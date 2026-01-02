using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using Sample;

public class StalkerAI : MonoBehaviour
{
    [Header("Settings")]
    public Transform player;
    public float moveSpeed = 3.0f;
    public float startDelay = 5.0f;

    // OPTİMİZASYON İÇİN YENİ AYAR
    [Tooltip("Oyuncu bu mesafeden uzaksa Freddy durur ve ses keser.")]
    public float sleepDistance = 15.0f;

    [Header("Game Over & VFX")]
    public float catchDistance = 1.0f;
    public Image fadeImage;
    public float fadeDuration = 1.5f;

    [Header("Vision")]
    [Range(0f, 1f)]
    public float visionAngleThreshold = 0.4f;

    [Header("Audio Settings")]
    public AudioClip walkSound;
    private AudioSource audioSource;

    private NavMeshAgent agent;
    private Camera mainCam;
    private Animator anim;
    private bool isGameOver = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        mainCam = Camera.main;
        audioSource = GetComponent<AudioSource>();

        if (player == null && mainCam != null)
        {
            player = mainCam.transform.root;
        }

        agent.speed = moveSpeed;
        agent.acceleration = 200f;

        SetLayerRecursively(gameObject, LayerMask.NameToLayer("Ignore Raycast"));

        if (fadeImage != null)
        {
            fadeImage.canvasRenderer.SetAlpha(0f);
            fadeImage.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (player == null || isGameOver) return;

        // Başlangıç beklemesi
        if (Time.timeSinceLevelLoad < startDelay)
        {
            StopEnemy();
            return;
        }

        // --- OPTİMİZASYON VE SES KESME (YENİ KISIM) ---
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Eğer oyuncu belirlenen mesafeden (sleepDistance) daha uzaksa:
        if (distanceToPlayer > sleepDistance)
        {
            StopEnemy(); // Sesi ve hareketi durdur
            return;      // Aşağıdaki kodları (Görüş hesaplamalarını vs.) hiç çalıştırma
        }
        // ----------------------------------------------

        // Yakalama Kontrolü
        if (distanceToPlayer < catchDistance)
        {
            StartCoroutine(GameOverSequence());
            return;
        }

        // Weeping Angel Mantığı
        bool isVisible = IsVisibleToPlayer();

        if (isVisible)
        {
            StopEnemy();
            if (anim != null) anim.speed = 0f;
        }
        else
        {
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
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (anim != null) anim.SetBool("isMoving", false);

        // SESİ DURDUR (En önemlisi burası)
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    void MoveEnemy()
    {
        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

        if (audioSource != null && !audioSource.isPlaying && walkSound != null)
        {
            audioSource.clip = walkSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    IEnumerator GameOverSequence()
    {
        isGameOver = true;
        Debug.Log("YAKALANDIN! Saldırı başlıyor...");

        if (audioSource != null) audioSource.Stop();

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (anim != null)
        {
            anim.speed = 1f;
            anim.SetTrigger("AttackTrigger");
        }

        yield return new WaitForSeconds(0.5f);

        KidsScript playerScript = player.GetComponent<KidsScript>();
        if (playerScript != null)
        {
            playerScript.ForceFaint();
        }

        if (fadeImage != null)
        {
            fadeImage.CrossFadeAlpha(1f, fadeDuration, false);
        }

        yield return new WaitForSeconds(fadeDuration);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    bool IsVisibleToPlayer()
    {
        Vector3 directionToEnemy = (transform.position - mainCam.transform.position).normalized;
        float dotProduct = Vector3.Dot(mainCam.transform.forward, directionToEnemy);

        if (dotProduct < visionAngleThreshold) return false;

        Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;
        Vector3 targetPosition = mainCam.transform.position;
        Vector3 directionToCamera = targetPosition - rayOrigin;
        float distance = Vector3.Distance(rayOrigin, targetPosition);

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, directionToCamera, out hit, distance))
        {
            if (hit.transform.root != player && hit.transform.gameObject != mainCam.gameObject)
            {
                return false;
            }
        }
        return true;
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