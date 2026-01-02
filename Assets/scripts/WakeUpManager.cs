using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Bunu eklemeyi unutma
using System.Collections;

public class WakeUpManager : MonoBehaviour
{
    [Header("UI Ayarları")]
    public Image whiteScreen;
    public float fadeDuration = 2.0f;

    [Header("Ses Ayarları")]
    public AudioClip wakeUpVoice;
    private AudioSource audioSource;

    [Header("Kamera Animasyonu")]
    public Animator cameraAnim;

    // Geçilecek sahnenin adı (İsim yazma ekranı)
    public string nameInputSceneName = "NameOnBoard";

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        // Timer'ı durdur (Oyun bitti, artık süre işlemesin)
        TimerManager.SayaciDurdur();

        if (whiteScreen != null)
        {
            whiteScreen.gameObject.SetActive(true);
            whiteScreen.canvasRenderer.SetAlpha(1.0f);
        }

        StartCoroutine(WakeUpSequence());
    }

    IEnumerator WakeUpSequence()
    {
        yield return new WaitForSeconds(1.0f);
        if (whiteScreen != null)
        {
            whiteScreen.CrossFadeAlpha(0.0f, fadeDuration, false);
        }

        if (wakeUpVoice != null)
        {
            audioSource.clip = wakeUpVoice;
            audioSource.Play();
        }


        //yield return new WaitForSeconds(1.5f);

        if (cameraAnim != null)
        {
            cameraAnim.SetTrigger("WakeUp");
        }

        yield return new WaitForSeconds(fadeDuration);

        // Mouse'u açıyoruz ki isim yazabilsin
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        yield return new WaitForSeconds(4.5f);
        // --- DEĞİŞİKLİK BURADA ---
        // Animasyon bitti, şimdi İsim Girme Sahnesine gidiyoruz
        SceneManager.LoadScene(nameInputSceneName);
    }
}