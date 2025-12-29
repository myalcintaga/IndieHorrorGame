using System.Collections;

using System.Collections.Generic;

using UnityEngine;



namespace Sample {

public class KidsScript : MonoBehaviour

{

  private Animator _Animator;



  private float _CameraYaw = 0f;



  private float _CameraPitch = 10f; // yukarı-aşağı açı



  private float _CameraDistance = 2.0f;



  private float _CameraRadius = 0.25f;

  [SerializeField] private float _CrouchCenterOffset = 0.2f; // Havada kalmayı düzeltmek için bunu artıracağız



  private float _GravityForce = 10.0f;

private float _OriginalHeight;      // Karakterin ayaktaki boyu
private float _OriginalCenterY;     // Karakterin merkez noktası
private bool _IsCrouching = false;  // Şu an eğiliyor mu?









  private CharacterController _Ctrl;

  private Vector3 _MoveDirection = Vector3.zero;

  private GameObject _View_Camera;

  private Transform _Light;

  private SkinnedMeshRenderer _MeshRenderer;

  // Hash

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



  void Start()

{

    _Animator = this.GetComponent<Animator>();

    _Ctrl = this.GetComponent<CharacterController>();

    _View_Camera = GameObject.Find("Main Camera");

    _Light = GameObject.Find("Directional Light").transform;

    _MeshRenderer = this.transform.Find("Boy0.Humanoid.Body").gameObject.GetComponent<SkinnedMeshRenderer>();



    // --- BURAYI EKLE ---

    Cursor.lockState = CursorLockMode.Locked; // Mouse'u merkeze kilitler

    Cursor.visible = false;                   // Mouse'u görünmez yapar

    _OriginalHeight = _Ctrl.height;    // Başlangıç boyunu kaydet
    _OriginalCenterY = _Ctrl.center.y;

    // -------------------

}



  void Update()

{

    // -----------------------------------------------------------------

    // MOUSE KONTROLÜ (YENİ EKLENEN KISIM)

    // -----------------------------------------------------------------

    // ESC tuşuna basınca mouse'u serbest bırak ve göster

    if (Input.GetKeyDown(KeyCode.Escape))

    {

        Cursor.lockState = CursorLockMode.None;

        Cursor.visible = true;

    }

    // Ekrana tıklayınca mouse'u tekrar kilitle ve gizle

    else if (Input.GetMouseButtonDown(0))

    {

        Cursor.lockState = CursorLockMode.Locked;

        Cursor.visible = false;

    }

    CAMERA();

    DIRECTION_LIGHT();

    GRAVITY();

    STATUS();

    CROUCH();



    if(!_Status.ContainsValue( true ))

    {

        MOVE();

        JUMP();

        DAMAGE();

        FAINT();

    }

    else if(_Status.ContainsValue( true ))

    {

      int status_name = 0;

      foreach(var i in _Status)

      {

        if(i.Value == true)

        {

          status_name = i.Key;

          break;

        }

      }

      if(status_name == Jump)

      {

        MOVE();

        JUMP();

        FAINT();

      }

      else if(status_name == Damage)

      {

        DAMAGE();

      }

      else if(status_name == Faint)

      {

        FAINT();

      }

    }

  }

  //--------------------------------------------------------------------- STATUS

  // Flags to control slime's action

  // It is used by method in Update()

  //---------------------------------------------------------------------

  private const int Jump = 1;

  private const int Damage = 2;

  private const int Faint = 3;

  private Dictionary<int, bool> _Status = new Dictionary<int, bool>

  {

      {Jump, false },

      {Damage, false },

      {Faint, false },

  };

  //------------------------------

  private void STATUS ()

  {

      if(_Animator.GetCurrentAnimatorStateInfo(0).tagHash == JumpTag)

      {

          _Status[Jump] = true;

      }

      else if(_Animator.GetCurrentAnimatorStateInfo(0).tagHash != JumpTag)

      {

          _Status[Jump] = false;

      }



      if(_Animator.GetCurrentAnimatorStateInfo(0).tagHash == DamageTag)

      {

          _Status[Damage] = true;

      }

      else if(_Animator.GetCurrentAnimatorStateInfo(0).tagHash != DamageTag)

      {

          _Status[Damage] = false;

      }



      if(_Animator.GetCurrentAnimatorStateInfo(0).tagHash == FaintTag)

      {

          _Status[Faint] = true;

      }

      else if(_Animator.GetCurrentAnimatorStateInfo(0).tagHash != FaintTag)

      {

          _Status[Faint] = false;

      }

  }

  //--------------------------------------------------------------------- CAMERA

  // camera moving

  //---------------------------------------------------------------------

  private void CAMERA ()

{

    // Mouse input

    _CameraYaw   += Input.GetAxis("Mouse X") * 2.5f;

    _CameraPitch -= Input.GetAxis("Mouse Y") * 2.0f;



    _CameraPitch = Mathf.Clamp(_CameraPitch, -20f, 40f);



    Quaternion rot = Quaternion.Euler(_CameraPitch, _CameraYaw, 0);



    Vector3 targetPos = transform.position + Vector3.up * 0.8f;

    Vector3 desiredDir = rot * Vector3.back;

    Vector3 desiredPos = targetPos + desiredDir * _CameraDistance;



    RaycastHit hit;



    // 🔒 KESİN ÇARPIŞMA KONTROLÜ

    if (Physics.SphereCast(

        targetPos,

        _CameraRadius,

        desiredDir,

        out hit,

        _CameraDistance))

    {

        _View_Camera.transform.position =

            targetPos + desiredDir * (hit.distance - 0.05f);

    }

    else

    {

        _View_Camera.transform.position = desiredPos;

    }



    _View_Camera.transform.LookAt(targetPos);

}







  //--------------------------------------------------------------------- DIRECTION_LIGHT

  // Direction of light

  //---------------------------------------------------------------------

  private void DIRECTION_LIGHT ()

  {

    Vector3 pos = _Light.position - this.transform.position;

    _MeshRenderer.material.SetVector("_LightDir", pos);

  }

  //--------------------------------------------------------------------- GRAVITY

  // gravity for fall of slime

  //---------------------------------------------------------------------

  //--------------------------------------------------------------------- GRAVITY

  // Yerçekimi ve düşüş mantığı

  //---------------------------------------------------------------------

  private void GRAVITY ()

  {

    // DÜZELTME: Sadece aşağı doğru düşüyorsak (_MoveDirection.y <= 0) yer kontrolü yap.

    // Böylece zıplarken yanlışlıkla yere değmiş gibi algılayıp havada donmaz.

    if(CheckGrounded() && _MoveDirection.y <= 0)

    {

      if(_MoveDirection.y < -0.1f){

        _MoveDirection.y = -0.1f;

      }

    }

    else

    {

      // Yerçekimi uygula

      _MoveDirection.y -= _GravityForce * Time.deltaTime;

    }



    _Ctrl.Move(_MoveDirection * Time.deltaTime);

  }



  //--------------------------------------------------------------------- CheckGrounded

  // Zemin kontrolü

  //---------------------------------------------------------------------

  private bool CheckGrounded()

  {

    if (_Ctrl.isGrounded){

      return true;

    }

   

    // Raycast ayarı: Karakterin kendi içinden başlamasın diye 0.1f yukarıdan başlatıyoruz.

    Ray ray = new Ray(this.transform.position + Vector3.up * 0.1f, Vector3.down);

   

    // Mesafe: 0.2f yeterli (Çok uzun yaparsan havada yeri algılar)

    float range = 0.2f;

   

    // QueryTriggerInteraction.Ignore: Triggerlara (görünmez duvarlara) çarpıp yer sanmasın.

    return Physics.Raycast(ray, range, ~0, QueryTriggerInteraction.Ignore);

  }

  //--------------------------------------------------------------------- MOVE

  // for slime moving

  //---------------------------------------------------------------------

  //--------------------------------------------------------------------- MOVE
  // for slime moving
  //---------------------------------------------------------------------
  private void MOVE ()
  {
    float speed = _Animator.GetFloat(SpeedParameter);

    //------------------------------------------------------------ Speed (HIZ KONTROLÜ)
    // --- YENİ DÜZENLEME: EĞİLME KONTROLÜ ---
    if (_IsCrouching)
    {
        // Eğer eğiliyorsak VE yön tuşlarından birine basıyorsak yavaş yürü
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || 
            Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            speed = 0.5f; // Eğilme yürüyüş hızı
        }
        else
        {
            speed = 0f; // Sadece C'ye basıyorsa olduğu yerde dursun
        }
    }
    else
    {
        // --- ESKİ KOD: NORMAL AYAKTA HIZLANMA ---
        // DEĞİŞİKLİK: Z -> LeftShift
        if(Input.GetKey(KeyCode.LeftShift))
        {
          if(speed <= 2){
            speed += 0.01f;
          }
          else if(speed >= 2){
            speed = 2;
          }
        }
        else {
          if(speed >= 1){
            speed -= 0.01f;
          }
          else if(speed <= 1){
            speed = 1;
          }
        }
    }
    // ----------------------------------------
    _Animator.SetFloat(SpeedParameter, speed);

    //------------------------------------------------------------ Forward
    // DEĞİŞİKLİK: UpArrow -> W
    if (Input.GetKey(KeyCode.W))
    {
      // velocity
      // DÜZELTME: Sadece MoveState değil, JumpTag (zıplama) halindeyken de harekete izin ver.
      var currentState = _Animator.GetCurrentAnimatorStateInfo(0);
      
      // --- YENİ EKLEME: || _IsCrouching (Eğilirken de yürüyebilsin) ---
      if (currentState.fullPathHash == MoveState || currentState.tagHash == JumpTag || _IsCrouching)
      {
        Vector3 camForward = _View_Camera.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        // Karakteri kamera yönüne döndür
        Quaternion targetRot = Quaternion.LookRotation(camForward);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * 10f
        );

        Vector3 velocity = camForward * speed;
        MOVE_XZ(velocity);
        MOVE_RESET();
      }
    }
    // DEĞİŞİKLİK: UpArrow -> W
    if (Input.GetKeyDown(KeyCode.W))
    {
      // Eğer zıplamıyorsak VE eğilmiyorsak yürüme animasyonuna geç
      if(_Animator.GetCurrentAnimatorStateInfo(0).tagHash != JumpTag && !_IsCrouching){
        _Animator.CrossFade(MoveState, 0.1f, 0, 0);
      }
    }

    // ------------------------------------------------------------ Backward (S)
    if (Input.GetKey(KeyCode.S))
    {
        // --- YENİ EKLEME: || _IsCrouching ---
        if (_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == MoveState || _IsCrouching)
        {
            Vector3 camBackward = -_View_Camera.transform.forward;
            camBackward.y = 0;
            camBackward.Normalize();

            // karakteri geri yürürken de kamera yönüne hizala
            Quaternion targetRot = Quaternion.LookRotation(-camBackward);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * 10f
            );

            Vector3 velocity = camBackward * speed;
            MOVE_XZ(velocity);
            MOVE_RESET();
        }
    }

    //------------------------------------------------------------ character rotation
    // DEĞİŞİKLİK: RightArrow -> D, LeftArrow -> A
    if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A)){
      this.transform.Rotate(Vector3.up, 1.0f);
    }
    else if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)){
      this.transform.Rotate(Vector3.up, -1.0f);
    }
    
    // DEĞİŞİKLİK: UpArrow -> W, DownArrow -> S
    if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
    {
      if(_Animator.GetCurrentAnimatorStateInfo(0).tagHash != JumpTag)
      {
        // --- EĞİLİYORSAK ANİMASYONU DEĞİŞTİRME (Sadece dön) ---
        if (!_IsCrouching) 
        {
            // DEĞİŞİKLİK: RightArrow -> D, LeftArrow -> A
            if (Input.GetKeyDown(KeyCode.D) && !Input.GetKey(KeyCode.A)){
              _Animator.CrossFade(MoveState, 0.1f, 0, 0);
            }
            else if (Input.GetKeyDown(KeyCode.A) && !Input.GetKey(KeyCode.D)){
              _Animator.CrossFade(MoveState, 0.1f, 0, 0);
            }
        }
      }
      // rotate stop
      // DEĞİŞİKLİK: RightArrow -> D, LeftArrow -> A
      else if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.A))
      {
        if(_Animator.GetCurrentAnimatorStateInfo(0).tagHash != JumpTag && !_IsCrouching){
          _Animator.CrossFade(IdleState, 0.1f, 0, 0);
        }
      }
    }
    KEY_UP();
  }

  //--------------------------------------------------------------------- KEY_UP
  // whether arrow key is key up
  //---------------------------------------------------------------------
  private void KEY_UP ()
  {
    // --- YENİ EKLEME: EĞİLİYORSAK BURADAN ÇIK (Animasyonu Idle yapma) ---
    if (_IsCrouching) return;
    // --------------------------------------------------------------------

    if(_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash != JumpState
        && !_Animator.IsInTransition(0))
    {
      // DEĞİŞİKLİK: UpArrow -> W
      if (Input.GetKeyUp(KeyCode.W))
      {
        // DEĞİŞİKLİK: LeftArrow -> A, RightArrow -> D
        if(!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
          _Animator.CrossFade(IdleState, 0.1f, 0, 0);
        }
      }
      // DEĞİŞİKLİK: UpArrow -> W, DownArrow -> S
      else if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
      {
        // DEĞİŞİKLİK: RightArrow -> D, LeftArrow -> A
        if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.A))
        {
          if(Input.GetKey(KeyCode.A)){
            _Animator.CrossFade(MoveState, 0.1f, 0, 0);
          }
          else if(Input.GetKey(KeyCode.D)){
            _Animator.CrossFade(MoveState, 0.1f, 0, 0);
          }
          else{
            _Animator.CrossFade(IdleState, 0.1f, 0, 0);
          }
        }
      }
    }
  }
  

  //--------------------------------------------------------------------- MOVE_SUB

  // value for moving

  //---------------------------------------------------------------------

  private void MOVE_XZ (Vector3 velocity)

  {

      _MoveDirection = new Vector3 (velocity.x, _MoveDirection.y, velocity.z);

      _Ctrl.Move(_MoveDirection * Time.deltaTime);

  }

  private void MOVE_RESET()

  {

      _MoveDirection.x = 0;

      _MoveDirection.z = 0;

  }

  //--------------------------------------------------------------------- JUMP

  // for jumping

  //---------------------------------------------------------------------

  private void JUMP ()

  {

    if(CheckGrounded())

    {

      // DEĞİŞİKLİK: S -> Space (ZIPLAMA)

      if(Input.GetKeyDown(KeyCode.Space)

          && _Animator.GetCurrentAnimatorStateInfo(0).tagHash != JumpTag

          && !_Animator.IsInTransition(0))

      {

        _Animator.CrossFade(JumpState, 0.1f, 0, 0);

        // jump power

        _MoveDirection.y = 3.0f;

        _Animator.SetFloat(JumpPoseParameter, _MoveDirection.y);

      }

      if (_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == JumpState

          && !_Animator.IsInTransition(0)

          && JumpPoseParameter < 0)

      {

        // DEĞİŞİKLİK: UpArrow -> W, DownArrow -> S, LeftArrow -> A, RightArrow -> D

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)

            || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))

        {

          _Animator.CrossFade(MoveState, 0.3f, 0, 0);

        }

        else{

          _Animator.CrossFade(IdleState, 0.3f, 0, 0);

        }

      }

    }

    else if(!CheckGrounded())

    {

      if (_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == JumpState

          && !_Animator.IsInTransition(0))

        {

          _Animator.SetFloat(JumpPoseParameter, _MoveDirection.y);

        }

      if(_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash != JumpState

            && !_Animator.IsInTransition(0))

        {

          _Animator.CrossFade(JumpState, 0.1f, 0, 0);

        }

    }

  }

  //--------------------------------------------------------------------- DAMAGE

  // play animation of damage

  //---------------------------------------------------------------------

  private void DAMAGE ()

  {

    // DEĞİŞİKLİK: Q aynı kaldı (ama listede vardı, kontrol edildi)

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

  //--------------------------------------------------------------------- FAINT

  // play animation of down and jump of resurrection

  //---------------------------------------------------------------------

  private void FAINT ()

  {

    // DEĞİŞİKLİK: W -> X (BAYILMA)

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



    // DEĞİŞİKLİK: E aynı kaldı (Ayağa kalkma)

    if (Input.GetKeyDown(KeyCode.E)

        && _Animator.GetCurrentAnimatorStateInfo(0).fullPathHash == FaintState

        && !_Animator.IsInTransition(0))

    {

      _Animator.CrossFade(StandUpFaintState, 0.1f, 0, 0);

    }

  }

  //--------------------------------------------------------------------- CROUCH
//--------------------------------------------------------------------- CROUCH
//--------------------------------------------------------------------- CROUCH
private void CROUCH()
{
    // C tuşuna basınca eğilme başlar
    if (Input.GetKeyDown(KeyCode.C))
    {
        _IsCrouching = true;

        // 1. Boyu yarıya indir
        float newHeight = _OriginalHeight * 0.5f;
        _Ctrl.height = newHeight;

        // 2. DÜZELTME: "Havada kalma" sorunu için Offset ekledik.
        // Bu formül kapsülün merkezini yukarı taşır, böylece karakter görsel olarak aşağı iner.
        // Eğer hala havada kalıyorsa Inspector'dan _CrouchCenterOffset değerini 0.1, 0.2 gibi artır.
        _Ctrl.center = new Vector3(0, (newHeight / 2.0f) + _CrouchCenterOffset, 0);

        _Animator.CrossFade(CrouchState, 0.1f, 0, 0);
    }

    // C tuşunu bırakınca ayağa kalkar
    if (Input.GetKeyUp(KeyCode.C))
    {
        _IsCrouching = false;

        // Eski haline getir
        _Ctrl.height = _OriginalHeight;
        _Ctrl.center = new Vector3(0, _OriginalCenterY, 0);

        _Animator.CrossFade(IdleState, 0.1f, 0, 0);
    }
}

}

}