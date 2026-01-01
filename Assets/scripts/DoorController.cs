using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class DoorController : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public Transform doorPivot;
    // isLocked değişkeni kaldırıldı

    [Header("Puzzle Settings")]
    public Transform playerObject;
    public float requiredHeight = 0.5f;

    [Header("Animation")]
    public float openSpeed = 2.0f;
    public float openAngle = 90f;

    [Header("Audio Settings")]
    public AudioClip openSound;   // Açılma sesi
    public AudioClip closeSound;  // Kapanma sesi
    // lockedSound değişkeni kaldırıldı

    private AudioSource audioSource;
    private bool isOpen = false;
    private Coroutine currentAnimation;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Interact()
    {
        // Kilit kontrol bloğu tamamen çıkarıldı.

        // 1. Yükseklik Kontrolü (Sadece kapı kapalıyken açmaya çalışırsan bakar)
        if (!isOpen)
        {
            if (playerObject == null)
            {
                Debug.LogError("HATA: Inspector'da Player Object kutusu boş!");
                return;
            }

            float footHeight = playerObject.position.y;

            if (footHeight < requiredHeight)
            {
                FindFirstObjectByType<Interactor>().ShowWarningMessage("You're too short to reach the handle!");
                return;
            }
        }

        // 2. Kapı Açma / Kapama İşlemi
        isOpen = !isOpen;

        // --- SES ÇALMA ---
        if (isOpen)
        {
            PlaySound(openSound);
        }
        else
        {
            PlaySound(closeSound);
        }

        // 3. Animasyon Başlatma
        Quaternion targetRotation;
        if (isOpen) targetRotation = Quaternion.Euler(0, openAngle, 0);
        else targetRotation = Quaternion.Euler(0, 0, 0);

        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(RotateDoor(targetRotation));
    }

    public string GetDescription()
    {
        // Kilitli yazısı kaldırıldı
        return isOpen ? "Close [E]" : "Open [E]";
    }

    IEnumerator RotateDoor(Quaternion target)
    {
        while (Quaternion.Angle(doorPivot.localRotation, target) > 0.1f)
        {
            doorPivot.localRotation = Quaternion.Slerp(doorPivot.localRotation, target, Time.deltaTime * openSpeed);
            yield return null;
        }
        doorPivot.localRotation = target;
        currentAnimation = null;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f); // Hafif ton farkı ekler, doğal duyulur
            audioSource.PlayOneShot(clip);
        }
    }
}