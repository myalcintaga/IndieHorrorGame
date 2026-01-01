using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sample
{
    public class KidsScript : MonoBehaviour
    {
        private Animator _Animator;
        private CharacterController _Ctrl;
        private GameObject _View_Camera;
        private SkinnedMeshRenderer _MeshRenderer;

        [Header("Camera Settings")]
        private float _CameraYaw = 0f;
        private float _CameraPitch = 10f;
        private float _CameraDistance = 2.0f;
        private float _CameraRadius = 0.25f;

        [Header("Movement Settings")]
        [SerializeField] private float _CrouchCenterOffset = 0f;
        private float _GravityForce = 10.0f;

        private float _OriginalHeight;
        private float _OriginalCenterY;
        private bool _IsCrouching = false;
        private Vector3 _MoveDirection = Vector3.zero;

        // Hash ID'leri
        private static readonly int IdleState = Animator.StringToHash("Base Layer.idle");
        private static readonly int MoveState = Animator.StringToHash("Base Layer.move");
        private static readonly int JumpState = Animator.StringToHash("Base Layer.jump");
        private static readonly int DamageState = Animator.StringToHash("Base Layer.damage");
        private static readonly int DownState = Animator.StringToHash("Base Layer.down");
        private static readonly int FaintState = Animator.StringToHash("Base Layer.faint");
        private static readonly int StandUpFaintState = Animator.StringToHash("Base Layer.standup_faint");
        private static readonly int CrouchState = Animator.StringToHash("Base Layer.crouch");

        private static readonly int JumpTag = Animator.StringToHash("Jump");
        private static readonly int DamageTag = Animator.StringToHash("Damage");
        private static readonly int FaintTag = Animator.StringToHash("Faint");

        private static readonly int SpeedParameter = Animator.StringToHash("Speed");
        private static readonly int JumpPoseParameter = Animator.StringToHash("JumpPose");

        // Durum Kontrolü
        private const int Jump = 1;
        private const int Damage = 2;
        private const int Faint = 3;
        private Dictionary<int, bool> _Status = new Dictionary<int, bool>
        {
            {Jump, false },
            {Damage, false },
            {Faint, false },
        };

        public bool InputBlocked = false;

        // --- OYUN BAŞLANGIÇ DEĞİŞKENİ ---
        private bool _IsGameStartFaint = true;

        void Start()
        {
            _Animator = GetComponent<Animator>();
            _Ctrl = GetComponent<CharacterController>();
            _View_Camera = GameObject.Find("Main Camera");

            Transform bodyTransform = transform.Find("Boy0.Humanoid.Body");
            if (bodyTransform != null)
                _MeshRenderer = bodyTransform.gameObject.GetComponent<SkinnedMeshRenderer>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _OriginalHeight = _Ctrl.height;
            _OriginalCenterY = _Ctrl.center.y;

            // --- BAŞLANGIÇ AYARLARI ---
            InputBlocked = true;
            _Animator.Play(FaintState);
        }

        void Update()
        {
            if (PauseManager.GameIsPaused) return;

            // --- DÜZELTME BURADA YAPILDI ---
            // Kamera her zaman çalışmalı, karakter baygın olsa bile.
            // Bu yüzden return'den önceye (en tepeye) aldık.
            CAMERA();

            // --- YENİ BAŞLANGIÇ MANTIĞI ---
            if (_IsGameStartFaint)
            {
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) ||
                    Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
                {
                    _Animator.CrossFade(StandUpFaintState, 0.1f, 0, 0);
                    _IsGameStartFaint = false;
                    StartCoroutine(EnableInputAfterWakeUp());
                }
                // DİKKAT: Buradaki return yüzünden CAMERA aşağıda kalınca çalışmıyordu.
                // Kamerayı yukarı aldığımız için sorun çözüldü.
                return;
            }
            // --------------------------------

            if (InputBlocked) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            // CAMERA() buradan kaldırıldı ve en tepeye taşındı.

            GRAVITY();
            STATUS();
            CROUCH();

            if (!_Status.ContainsValue(true))
            {
                MOVE();
                JUMP();
                DAMAGE();
                FAINT();
            }
            else if (_Status.ContainsValue(true))
            {
                int status_name = 0;
                foreach (var i in _Status)
                {
                    if (i.Value == true)
                    {
                        status_name = i.Key;
                        break;
                    }
                }
                if (status_name == Jump)
                {
                    MOVE();
                    JUMP();
                    FAINT();
                }
                else if (status_name == Damage)
                {
                    DAMAGE();
                }
                else if (status_name == Faint)
                {
                    FAINT();
                }
            }
        }

        IEnumerator EnableInputAfterWakeUp()
        {
            yield return new WaitForSeconds(1.0f);
            InputBlocked = false;
        }

        public void ForceFaint()
        {
            InputBlocked = true;
            _Animator.CrossFade(DownState, 0.1f, 0, 0);
        }

        private void STATUS()
        {
            var stateInfo = _Animator.GetCurrentAnimatorStateInfo(0);
            _Status[Jump] = (stateInfo.tagHash == JumpTag);
            _Status[Damage] = (stateInfo.tagHash == DamageTag);
            _Status[Faint] = (stateInfo.tagHash == FaintTag);
        }

        private void CAMERA()
        {
            _CameraYaw += Input.GetAxis("Mouse X") * 2.5f;
            _CameraPitch -= Input.GetAxis("Mouse Y") * 2.0f;
            _CameraPitch = Mathf.Clamp(_CameraPitch, -20f, 40f);

            Quaternion rot = Quaternion.Euler(_CameraPitch, _CameraYaw, 0);
            Vector3 targetPos = transform.position + Vector3.up * 0.8f;
            Vector3 desiredDir = rot * Vector3.back;
            Vector3 desiredPos = targetPos + desiredDir * _CameraDistance;

            RaycastHit hit;
            if (Physics.SphereCast(targetPos, _CameraRadius, desiredDir, out hit, _CameraDistance))
            {
                _View_Camera.transform.position = targetPos + desiredDir * (hit.distance - 0.05f);
            }
            else
            {
                _View_Camera.transform.position = desiredPos;
            }
            _View_Camera.transform.LookAt(targetPos);
        }

        private void GRAVITY()
        {
            if (CheckGrounded() && _MoveDirection.y <= 0)
            {
                if (_MoveDirection.y < -0.1f)
                {
                    _MoveDirection.y = -0.1f;
                }
            }
            else
            {
                _MoveDirection.y -= _GravityForce * Time.deltaTime;
            }
            _Ctrl.Move(_MoveDirection * Time.deltaTime);
        }

        private bool CheckGrounded()
        {
            if (_Ctrl.isGrounded) return true;
            Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
            float range = 0.2f;
            return Physics.Raycast(ray, range, ~0, QueryTriggerInteraction.Ignore);
        }

        private void MOVE()
        {
            float speed = _Animator.GetFloat(SpeedParameter);

            if (_IsCrouching)
            {
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) ||
                    Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
                {
                    speed = 0.5f;
                }
                else
                {
                    speed = 0f;
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (speed <= 1.5f) speed += 0.01f;
                    else if (speed >= 1.5f) speed = 1.5f;
                }
                else
                {
                    if (speed >= 1) speed -= 0.01f;
                    else if (speed <= 1) speed = 1;
                }
            }
            _Animator.SetFloat(SpeedParameter, speed);

            var currentState = _Animator.GetCurrentAnimatorStateInfo(0);
            if (Input.GetKey(KeyCode.W))
            {
                if (currentState.fullPathHash == MoveState || currentState.tagHash == JumpTag || _IsCrouching)
                {
                    Vector3 camForward = _View_Camera.transform.forward;
                    camForward.y = 0;
                    camForward.Normalize();

                    Quaternion targetRot = Quaternion.LookRotation(camForward);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);

                    Vector3 velocity = camForward * speed;
                    MOVE_XZ(velocity);
                    MOVE_RESET();
                }
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                if (_Animator.GetCurrentAnimatorStateInfo(0).tagHash != JumpTag && !_IsCrouching)
                {
                    _Animator.CrossFade(MoveState, 0.1f, 0, 0);
                }
            }

            if (Input.GetKey(KeyCode.S))
            {
                if (currentState.fullPathHash == MoveState || _IsCrouching)
                {
                    Vector3 camBackward = -_View_Camera.transform.forward;
                    camBackward.y = 0;
                    camBackward.Normalize();

                    Quaternion targetRot = Quaternion.LookRotation(-camBackward);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);

                    Vector3 velocity = camBackward * speed;
                    MOVE_XZ(velocity);
                    MOVE_RESET();
                }
            }

            if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A))
            {
                transform.Rotate(Vector3.up, 1.0f);
            }
            else if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
            {
                transform.Rotate(Vector3.up, -1.0f);
            }

            if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
            {
                if (_Animator.GetCurrentAnimatorStateInfo(0).tagHash != JumpTag && !_IsCrouching)
                {
                    if (Input.GetKeyDown(KeyCode.D) && !Input.GetKey(KeyCode.A))
                        _Animator.CrossFade(MoveState, 0.1f, 0, 0);
                    else if (Input.GetKeyDown(KeyCode.A) && !Input.GetKey(KeyCode.D))
                        _Animator.CrossFade(MoveState, 0.1f, 0, 0);
                    else if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.A))
                        _Animator.CrossFade(IdleState, 0.1f, 0, 0);
                }
            }
            KEY_UP();
        }

        private void KEY_UP()
        {
            if (_IsCrouching) return;

            if (_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash != JumpState && !_Animator.IsInTransition(0))
            {
                if (Input.GetKeyUp(KeyCode.W))
                {
                    if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                        _Animator.CrossFade(IdleState, 0.1f, 0, 0);
                }
                else if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
                {
                    if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.A))
                    {
                        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
                            _Animator.CrossFade(MoveState, 0.1f, 0, 0);
                        else
                            _Animator.CrossFade(IdleState, 0.1f, 0, 0);
                    }
                }
            }
        }

        private void MOVE_XZ(Vector3 velocity)
        {
            _MoveDirection = new Vector3(velocity.x, _MoveDirection.y, velocity.z);
            _Ctrl.Move(_MoveDirection * Time.deltaTime);
        }

        private void MOVE_RESET()
        {
            _MoveDirection.x = 0;
            _MoveDirection.z = 0;
        }

        private void JUMP()
        {
            bool isGrounded = CheckGrounded();
            var stateInfo = _Animator.GetCurrentAnimatorStateInfo(0);

            if (isGrounded)
            {
                if (Input.GetKeyDown(KeyCode.Space)
                    && stateInfo.tagHash != JumpTag
                    && !_Animator.IsInTransition(0))
                {
                    _Animator.CrossFade(JumpState, 0.1f, 0, 0);
                    _MoveDirection.y = 3.0f;
                    _Animator.SetFloat(JumpPoseParameter, _MoveDirection.y);
                }
                else if (stateInfo.fullPathHash == JumpState && !_Animator.IsInTransition(0) && _MoveDirection.y <= 0.1f)
                {
                    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) ||
                        Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
                    {
                        _Animator.CrossFade(MoveState, 0.1f, 0, 0);
                    }
                    else
                    {
                        _Animator.CrossFade(IdleState, 0.1f, 0, 0);
                    }
                }
            }
        }

        private void DAMAGE()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                _Animator.CrossFade(DamageState, 0.1f, 0, 0);
            }
            if (_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1
                && _Animator.GetCurrentAnimatorStateInfo(0).tagHash == DamageTag
                && !_Animator.IsInTransition(0))
            {
                _Animator.CrossFade(IdleState, 0.3f, 0, 0);
            }
        }

        private void FAINT()
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                _Animator.CrossFade(DownState, 0.1f, 0, 0);
            }
            if (_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1
                && _Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == DownState
                && !_Animator.IsInTransition(0))
            {
                _Animator.CrossFade(FaintState, 0.3f, 0, 0);
            }
            if (Input.GetKeyDown(KeyCode.E)
                && _Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == FaintState
                && !_Animator.IsInTransition(0))
            {
                _Animator.CrossFade(StandUpFaintState, 0.1f, 0, 0);
            }
        }

        private void CROUCH()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                _IsCrouching = true;

                float newHeight = _OriginalHeight * 0.5f;
                _Ctrl.height = newHeight;
                _Ctrl.center = new Vector3(0, (newHeight / 2.0f) + _CrouchCenterOffset, 0);

                _Animator.CrossFade(CrouchState, 0.1f, 0, 0);
            }

            if (Input.GetKeyUp(KeyCode.C))
            {
                _IsCrouching = false;
                _Ctrl.height = _OriginalHeight;
                _Ctrl.center = new Vector3(0, _OriginalCenterY, 0);

                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) ||
                    Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
                {
                    _Animator.CrossFade(MoveState, 0.1f, 0, 0);
                }
                else
                {
                    _Animator.CrossFade(IdleState, 0.1f, 0, 0);
                }
            }
        }
    }
}