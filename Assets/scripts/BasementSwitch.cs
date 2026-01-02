using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // UI kütüphanesi eklendi
using System.Collections;

public class BasementSwitch : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject lightSource; // Tavan lambası
    public AudioClip switchSound;
    public int nextSceneIndex = 2;

    [Header("White Screen Fade")]
    public Image whiteScreen; // Canvas'taki Beyaz Resim
    public float fadeDuration = 2.0f; // 2 saniyede beyazlaşsın

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
    }

    void OnTriggerStay(Collider other)
    {
        if (isActivated) return;

        if (other.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
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