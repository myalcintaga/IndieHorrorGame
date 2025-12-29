using UnityEngine;

public class ObjectPusher : MonoBehaviour
{
    [Header("Settings")]
    public float pushPower = 2.0f; // İtme gücü

    // Karakter bir şeye çarptığında bu fonksiyon otomatik çalışır
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Çarptığımız şeyin Rigidbody'si var mı?
        Rigidbody body = hit.collider.attachedRigidbody;

        // Fizik yoksa veya "Kinematic" ise (hareketsizse) itme
        if (body == null || body.isKinematic)
        {
            return;
        }

        // Aşağıya doğru (zemine) itmeye çalışma
        if (hit.moveDirection.y < -0.3)
        {
            return;
        }

        // İtme yönünü hesapla (Karakterin hareket yönü)
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // Gücü uygula
        body.linearVelocity = pushDir * pushPower;
    }
}