using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public Transform doorPivot;
    public bool isLocked = false;

    [Header("Puzzle Settings")]
    // YENİ: Oyuncuyu (Boy0) buraya sürükleyeceğiz
    public Transform playerObject;
    public float requiredHeight = 0.5f;

    [Header("Animation")]
    public float openSpeed = 2.0f;
    public float openAngle = 90f;

    private bool isOpen = false;
    private Coroutine currentAnimation;

    public void Interact()
    {
        // 1. Kilit Kontrolü
        if (isLocked)
        {
            FindFirstObjectByType<Interactor>().ShowWarningMessage("Door is Locked!");
            return;
        }

        // 2. Yükseklik Kontrolü (Direkt Oyuncuya Göre)
        if (!isOpen)
        {
            // Eğer oyuncuyu atamayı unuttuysan hata vermesin diye kontrol
            if (playerObject == null)
            {
                Debug.LogError("HATA: Inspector'da Player Object kutusu boş!");
                return;
            }

            float footHeight = playerObject.position.y;

            // Konsola artık DOĞRU (sabit) ayak yüksekliğini yazacak
            Debug.Log("Sabit Ayak Yüksekliği: " + footHeight);

            if (footHeight < requiredHeight)
            {
                FindFirstObjectByType<Interactor>().ShowWarningMessage("You're too short to reach the handle!");
                return;
            }
        }

        // 3. Kapı Açma
        isOpen = !isOpen;

        Quaternion targetRotation;
        if (isOpen) targetRotation = Quaternion.Euler(0, openAngle, 0);
        else targetRotation = Quaternion.Euler(0, 0, 0);

        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(RotateDoor(targetRotation));
    }

    public string GetDescription()
    {
        if (isLocked) return "Locked";
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
}