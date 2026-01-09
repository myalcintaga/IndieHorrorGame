using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class BasementSwitch : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject lightSource; // Tavan lambası
    public AudioClip switchSound;
    public int nextSceneIndex = 2;

    [Header("UI Ayarları")]
    public GameObject interactionText; // YENİ: "Press E to turn on light" yazısı

    [Header("White Screen Fade")]
    public Image whiteScreen; // Canvas'taki Beyaz Resim
    public float fadeDuration = 2.0f;

    private bool isActivated = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        if (lightSource != null) lightSource.SetActive(false);

        // Başlangıçta beyaz ekranın görünmez (şeffaf) olduğundan emin olalım
        if (whiteScreen != null)
        {
            whiteScreen.gameObject.SetActive(true);
            whiteScreen.canvasRenderer.SetAlpha(0.0f);
        }

        // Başlangıçta etkileşim yazısı kapalı olsun
        if (interactionText != null)
        {
            interactionText.SetActive(false);
        }
    }

    // YENİ: Oyuncu alana girince yazıyı göster
    void OnTriggerEnter(Collider other)
    {
        if (isActivated) return; // Zaten bastıysa tekrar gösterme

        if (other.CompareTag("Player"))
        {
            if (interactionText != null) interactionText.SetActive(true);
        }
    }

    // YENİ: Oyuncu alandan çıkarsa yazıyı gizle
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (interactionText != null) interactionText.SetActive(false);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (isActivated) return;

        if (other.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Tuşa basıldığı an yazıyı gizle
                if (interactionText != null) interactionText.SetActive(false);

                StartCoroutine(FinishGameSequence());
            }
        }
    }

    IEnumerator FinishGameSequence()
    {
        isActivated = true;

        if (switchSound != null) audioSource.PlayOneShot(switchSound);
        if (lightSource != null) lightSource.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        // --- BEYAZ EKRAN GEÇİŞİ BAŞLASIN ---
        if (whiteScreen != null)
        {
            // 0 (Şeffaf) -> 1 (Tam Beyaz)
            whiteScreen.CrossFadeAlpha(1.0f, fadeDuration, false);
        }

        // Fade süresi kadar bekle
        yield return new WaitForSeconds(fadeDuration);

        // Sahne Yükle
        SceneManager.LoadScene(nextSceneIndex);
    }
}