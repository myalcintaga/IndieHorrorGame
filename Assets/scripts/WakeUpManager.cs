using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WakeUpManager : MonoBehaviour
{
    [Header("UI Ayarları")]
    public Image whiteScreen; // Canvas'taki Beyaz Resim
    public float fadeDuration = 2.0f; // 2 saniyede açılsın

    [Header("Ses Ayarları")]
    public AudioClip wakeUpVoice;
    private AudioSource audioSource;

    [Header("Kamera Animasyonu")]
    public Animator cameraAnim;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        // Ekran başta TAM BEYAZ olsun (Göz kamaşması etkisi)
        if (whiteScreen != null)
        {
            whiteScreen.gameObject.SetActive(true);
            whiteScreen.canvasRenderer.SetAlpha(1.0f); // 1 = Tam Opak
        }

        StartCoroutine(WakeUpSequence());
    }

    IEnumerator WakeUpSequence()
    {
        // Sahne yüklenir yüklenmez ses başlasın
        if (wakeUpVoice != null)
        {
            audioSource.clip = wakeUpVoice;
            audioSource.Play();
        }

        // --- BEYAZLIK SİLİNMEYE BAŞLASIN ---
        if (whiteScreen != null)
        {
            // 1 (Tam Beyaz) -> 0 (Şeffaf)
            whiteScreen.CrossFadeAlpha(0.0f, fadeDuration, false);
        }
        yield return new WaitForSeconds(1.5f);
        // Eğer kamera animasyonu varsa başlat
        if (cameraAnim != null)
        {
            cameraAnim.SetTrigger("WakeUp");
        }

        // Fade süresi bitene kadar bekle (Opsiyonel, sonra mouse açılır)
        yield return new WaitForSeconds(fadeDuration);

        // Mouse'u serbest bırak
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}