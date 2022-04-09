using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
// 스피드 조정 변수
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;
    private float applySpeed; //상황에 따라 각각의 Speed를 적용

    // 점프 정도
    [SerializeField]
    private float jumpForce;

    // 상태 변수
    private bool isRun = false; public bool GetRun(){return isRun;}
    private bool isGround = true;
    private bool isCrouch = false;
    private bool isWalk = false; 
    private bool pauseCameraRotation = false;
    static public bool isActivated = true;

    // 앉았을 때 얼마나 앉을지 결정하는 변수
    [SerializeField]
    private float crouchPosY;
    private float originPosY;
    private float applyCrouchPosY;

    // 전 프레임의 위치. 현재 프레임의 위치와 비교했을 때 달라지면 움직임이 있었던 것이다.
    private Vector3 lastPos;  
    private Crosshair theCrosshair;

    // 민감도
    [SerializeField]
    private float lookSensitivity;  

    // 카메라 한계
    [SerializeField]
    private float cameraRotationLimit;  
    private float currentCameraRotationX; 

    // 필요한 컴포넌트
    [SerializeField]
    private Camera theCamera; 
    private Rigidbody myRigid;
    private CapsuleCollider capsuleCollider; 
    private GunController theGunController;
    private StatusController theStatusController;

    void Start() 
    {
        // 컴포넌트 할당
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
        theGunController = FindObjectOfType<GunController>();
        theCrosshair = FindObjectOfType<Crosshair>();
        theStatusController = FindObjectOfType<StatusController>();

        // 초기화
        applySpeed = walkSpeed;
        originPosY = theCamera.transform.localPosition.y;
        applyCrouchPosY = originPosY;
    }

    void Update()
    {
        if (isActivated && GameManager.canPlayerMove)
        {
            IsGround();
            TryJump();
            TryRun();
            TryCrouch();
            Move();
            MoveCheck();
            CameraRotation();
            CharacterRotation();
        }
    }

    // 지면 체크
    private void IsGround()
    {
        //현재 위치에서 절대적인 월드 좌표계 기준 아래로 광선을 쐈을 때 충돌이 감지된다면
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
        theCrosshair.JumpingAnimation(!isGround);
    }

    // 점프 시도
    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround && theStatusController.GetCurrentSP() > 0)
        {
            Jump();
        }
    }

    // 점프
    private void Jump()
    {
        if (isCrouch)
            Crouch();

        theStatusController.DecreaseStamina(100);
        myRigid.velocity = transform.up * jumpForce;
    }

    // 달리기 시도 - 왼쪽 쉬프트키
    private void TryRun()
    {
        if (Input.GetKey(KeyCode.LeftShift) && theStatusController.GetCurrentSP() > 0)
        {
            Running();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) || theStatusController.GetCurrentSP() <= 0)
        {
            RunningCancel();
        }
    }

    // 달리기
    private void Running()
    {
        if (isCrouch)
            Crouch();

        theGunController.CancelFineSight(); //뛸 때는 정조준 해제
        isRun = true;
        theCrosshair.RunningAnimation(isRun);
        theStatusController.DecreaseStamina(10);
        applySpeed = runSpeed;
    }

    // 달리기 취소
    private void RunningCancel()
    {
        isRun = false;
        theCrosshair.RunningAnimation(isRun);
        applySpeed = walkSpeed;
    }

    // 앉기 시도
    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }

    // 앉기 동작 - 카메라 위치만 내려준다
    private void Crouch()
    {
        isCrouch = !isCrouch;
        theCrosshair.CrouchingAnimation(isCrouch);
        if (isCrouch)
        {
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY;
        }
        else
        {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }

        StartCoroutine(CrouchCoroutine());
    }

    // 부드러운 앉기 동작
    IEnumerator CrouchCoroutine()
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;

        while(_posY != applyCrouchPosY)
        {
            count++;
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.2f);
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);
            if(count > 15)
                break;
            yield return null;
        }
        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0);
    }

    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal");  
        float _moveDirZ = Input.GetAxisRaw("Vertical");  
        Vector3 _moveHorizontal = transform.right * _moveDirX; 
        Vector3 _moveVertical = transform.forward * _moveDirZ; 

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed; 

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);
    }

    private void CameraRotation()  
    {
        if (!pauseCameraRotation)
        {
            float _xRotation = Input.GetAxisRaw("Mouse Y"); 
            float _cameraRotationX = _xRotation * lookSensitivity;

            // currentCameraRotationX += _cameraRotationX;  마우스 Y 반전
            currentCameraRotationX -= _cameraRotationX;
            //currentCameraRotationX 가 (-cameraRotationLimit ~ cameraRotationLimit) 범위 안에만 있도록
            currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

            theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
        }
    }

    public IEnumerator TreeLookCoroutine(Vector3 _target)
    {
        pauseCameraRotation = true;

        Quaternion direction = Quaternion.LookRotation(_target - theCamera.transform.position);
        Vector3 eulerValue = direction.eulerAngles;
        float destinationX = eulerValue.x;

        while(Mathf.Abs(destinationX - currentCameraRotationX) >= 0.5f)
        {
            eulerValue = Quaternion.Lerp(theCamera.transform.localRotation, direction, 0.3f).eulerAngles;  // 쿼터니언 👉 벡터
            theCamera.transform.localRotation = Quaternion.Euler(eulerValue.x, 0f, 0f); // 벡터 👉 쿼터니언 (X축으로만 회전하면 됨)
            currentCameraRotationX = theCamera.transform.localEulerAngles.x; 
            yield return null;
        }
        pauseCameraRotation = false;
    }

    private void MoveCheck()
    {
        if (!isRun && !isCrouch && isGround)
        {
            if (Vector3.Distance(lastPos, transform.position) >= 0.01f)
                isWalk = true;
            else
                isWalk = false;

            theCrosshair.WalkingAnimation(isWalk);
            lastPos = transform.position;
        }  
    }

    private void CharacterRotation()  // 좌우 캐릭터 회전
    {
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        //두 쿼터니언의 회전량을 더하려면 서로 곱해야한다.
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
        // Debug.Log(myRigid.rotation);  // 쿼터니언
        // Debug.Log(myRigid.rotation.eulerAngles); // 벡터
    }
}