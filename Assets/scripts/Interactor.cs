using UnityEngine;
using TMPro;

public class Interactor : MonoBehaviour
{
    [Header("Settings")]
    public float interactionRange = 4f;
    public LayerMask ignoreLayers;
    public TextMeshProUGUI interactionText;

    private Camera cam;
    private bool isShowingWarning = false; // YENİ: Uyarı var mı kontrolü

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        // Eğer şu an ekranda kırmızı uyarı varsa, aşağıdaki işlemleri yapma (Yazıyı ezme)
        if (isShowingWarning) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * interactionRange, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, ~ignoreLayers))
        {
            IInteractable interactObj = hit.collider.GetComponentInParent<IInteractable>();

            if (interactObj != null)
            {
                // Normal durum: Ekrana objenin adını yaz (Örn: Open [E])
                if (interactionText) interactionText.text = interactObj.GetDescription();

                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactObj.Interact();
                }
            }
            else
            {
                if (interactionText) interactionText.text = ""; // Etkileşim yoksa boşalt
            }
        }
        else
        {
            if (interactionText) interactionText.text = ""; // Boşluğa bakıyorsak boşalt
        }
    }

    // --- UYARI SİSTEMİ ---
    public void ShowWarningMessage(string message)
    {
        if (interactionText != null)
        {
            // Eğer zaten bir uyarı varsa eskisini durdur
            StopAllCoroutines();
            StartCoroutine(ShowTempMessage(message));
        }
    }

    private System.Collections.IEnumerator ShowTempMessage(string msg)
    {
        isShowingWarning = true; // KİLİTLE: Update artık yazıyı değiştiremez

        interactionText.text = msg;
        interactionText.color = Color.red;

        yield return new WaitForSeconds(2.0f); // 2 saniye bekle

        interactionText.text = "";
        interactionText.color = Color.white;

        isShowingWarning = false; // KİLİDİ AÇ: Normal çalışmaya dön
    }
}