using UnityEngine;

namespace Sample
{
    public class ObjectPusher : MonoBehaviour
    {
        [Header("Ayarlar")]
        [SerializeField] private float _PushPower = 2.0f;       // İtme gücü
        [SerializeField] private string _PushableTag = "Pushable"; // İtilecek objenin etiketi

        private Animator _Animator;
        private CharacterController _Ctrl;

        // ANIMATOR STATE İSİMLERİ
        // Animator penceresindeki "Push Layer" içindeki kutucuk isimleriyle BİREBİR aynı olmalı.
        private static readonly int PushState = Animator.StringToHash("Push");
        private static readonly int EmptyState = Animator.StringToHash("Empty");

        // Animator Katman İndeksi (Base Layer = 0, Push Layer = 1)
        private const int PushLayerIndex = 1;

        void Start()
        {
            _Animator = GetComponent<Animator>();
            _Ctrl = GetComponent<CharacterController>();
        }

        void Update()
        {
            CheckPushAnimation();
        }

        //---------------------------------------------------------------------
        // FİZİKSEL İTME MANTIĞI (Otomatik Çalışır)
        //---------------------------------------------------------------------
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // 1. Çarptığımız objenin Rigidbody'si var mı?
            Rigidbody body = hit.collider.attachedRigidbody;

            // Yoksa veya "Kinematic" ise (hareketsizse) işlem yapma
            if (body == null || body.isKinematic)
                return;

            // 2. Tag kontrolü (Sadece "Pushable" etiketlileri it)
            if (!hit.collider.CompareTag(_PushableTag))
                return;

            // 3. Aşağı doğru (yerçekimi yönünde) itmeyi engelle
            if (hit.moveDirection.y < -0.3f)
                return;

            // 4. İtme yönünü hesapla ve güç uygula
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
            body.linearVelocity = pushDir * _PushPower;
        }

        //---------------------------------------------------------------------
        // ANİMASYON KONTROLÜ (Layer 1 - Push Layer)
        //---------------------------------------------------------------------
        private void CheckPushAnimation()
        {
            bool isPushing = false;
            RaycastHit hit;

            // Karakterin göbeğinden (0.5f yukarı) ileri doğru 1 metre ışın atıyoruz
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out hit, 1.0f))
            {
                // Eğer çarptığımız şey "Pushable" ise VE ileri (W) tuşuna basıyorsak
                if (hit.collider.CompareTag(_PushableTag) && Input.GetKey(KeyCode.W))
                {
                    isPushing = true;
                }
            }

            // --- KATMAN GEÇİŞLERİ ---
            if (isPushing)
            {
                // Eğer itiyorsak ve şu an Push animasyonunda değilsek -> Push'a geç
                // Not: Sadece PushLayer (Index 1) durumunu kontrol ediyoruz.
                if (_Animator.GetCurrentAnimatorStateInfo(PushLayerIndex).tagHash != PushState)
                {
                    _Animator.CrossFade(PushState, 0.1f, PushLayerIndex, 0);
                }
            }
            else
            {
                // İtmiyorsak ve şu an Empty (Boş) state'inde değilsek -> Empty'e geç (Kolları serbest bırak)
                if (_Animator.GetCurrentAnimatorStateInfo(PushLayerIndex).tagHash != EmptyState)
                {
                    _Animator.CrossFade(EmptyState, 0.1f, PushLayerIndex, 0);
                }
            }
        }
    }
}