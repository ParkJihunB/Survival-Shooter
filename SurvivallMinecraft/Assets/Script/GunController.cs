using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public static bool isActivate = false;  // 활성화 여부

    private bool isReload = false;  // 재장전 중인지. alse 일 때만 발사
    private bool isFineSightMode = false; // 정조준 중인지.

    [SerializeField]
    private Gun currentGun; // 현재 들고 있는 총의 Gun.cs 가 할당.
    //단발로 여러번 쏠 때, 그 한발 한발간의 텀이 길 수록 연사 속도가 낮은 것
    //한번 발사한 후 이 시간만큼 발사할 수 없다
    private float currentFireRate; //초기값은 연사 속도인 Gun.cs의 fireRate 

    [SerializeField]
    private Vector3 originPos;  // 원래 총의 위치(정조준 해제하면 나중에 돌아와야 함)
    [SerializeField]
    private LayerMask layerMask;

    private RaycastHit hitInfo;  // 총알의 충돌 정보
    [SerializeField]
    private GameObject hitEffectPrefab;
    [SerializeField]
    private Camera theCam;  // 카메라 시점에서 정 중앙에 Raycast 발사에 필요

    private Crosshair theCrosshair;
    private AudioSource audioSource;  // 발사 소리 재생기

    public Gun GetGun()
    {
        return currentGun;
    }

    void Start()
    {
        originPos = Vector3.zero;
        audioSource = GetComponent<AudioSource>();
        theCrosshair = FindObjectOfType<Crosshair>();

        WeaponManager.currentWeapon = currentGun.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentGun.anim;
    }

    void Update()
    {
        if(isActivate) //무기가 활성화 되었을때만 실행
        {
            GunFireRateCalc();
            if (!Inventory.invectoryActivated)
            {
                TryFire();
                TryReload();
                TryFineSight(); //마우스 우클릭
            }
 
        }
    }

    //currentFireRate를 시간만큼 감소시킨다
    private void GunFireRateCalc()
    {
        if (currentFireRate > 0)
            currentFireRate -= Time.deltaTime;  //1 초에 1 씩 감소
    }

    private void TryFire()  // 발사 입력(마우스 좌클릭)을 받고 발사 가능한지 확인
    {
        if(Input.GetButton("Fire1") && currentFireRate <= 0)
        {
            Fire();
        }
    }

    private void Fire()  // 발사를 위한 과정
    {
        if (!isReload)
        {
            if (currentGun.currentBulletCount > 0)
                Shoot();
            else //총을 발사하려는데 현재 탄창에 총알이 하나도 없는경우
            {
                CancelFineSight();
                StartCoroutine(ReloadCoroutine()); //자동 재장전
            }
        }
    }

    private void Shoot()  // 실제 발사 되는 과정
    {
        theCrosshair.FireAnimaion();
        //발사 처리
        currentGun.currentBulletCount--;
        currentFireRate = currentGun.fireRate;  // 연사 속도 재계산
        PlaySE(currentGun.fire_Sound);
        currentGun.muzzleFlash.Play();
        
        // 피격 처리
        Hit();

        // 총기 반동 코루틴 실행
        StopAllCoroutines();
        StartCoroutine(RetroActionCoroutine());

        Debug.Log("총알 발사");
    }

    public bool GetFineSightMode()
    {
        return isFineSightMode;
    }

    private void Hit()
    {
        // 카메라 월드 좌표 (localPosition이 아님)를 기준의 위치로부터 Raycast를 쏜다
        // range안에 충돌되는 것이 있다면 그 정보를 hitInfo에 담는다
        Vector3 randomShot =  new Vector3(Random.Range(
                                            -theCrosshair.GetAccuracy() - currentGun.accuracy, 
                                            -theCrosshair.GetAccuracy() + currentGun.accuracy),
                                        Random.Range(
                                            -theCrosshair.GetAccuracy() - currentGun.accuracy, 
                                            -theCrosshair.GetAccuracy() + currentGun.accuracy),
                                            0);
        if (Physics.Raycast(theCam.transform.position, theCam.transform.forward+randomShot, out hitInfo, currentGun.range,
            layerMask))
        {
            GameObject clone = Instantiate(hitEffectPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            Destroy(clone, 2f);
        }
    }

    //R키 입력 + isReload=false + 총알 개수
    //고려해서 재장전 처리를 한다
    private void TryReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReload && currentGun.currentBulletCount < currentGun.reloadBulletCount)
        {
            CancelFineSight(); //재장전 중일때는 정조준/정조준 해제가 이루어지면 안된다
            //이루어진다면 재장전 애니메이션 재생 코루틴 함수가 도중에 중단되므로
            //재장전이 완벽하게 끝나지 않아 isReload가 False가 되지 못한다
            //이 상태에서는 발사를 할 수 없어 버그가 난다
            StartCoroutine(ReloadCoroutine());
        }
    }

    private void TryFineSight()
    {
        if(Input.GetButtonDown("Fire2"))
        {
            FineSight();
        }
    }

    public void CancelFineSight()
    {
        if (isFineSightMode)
            FineSight();
    }

    private void FineSight()
    {
        //정조준중이었다면 정조준해제
        //정조준 아니었다면 정조준 모드로
        isFineSightMode = !isFineSightMode;
        currentGun.anim.SetBool("FineSightMode", isFineSightMode); //현재 상태에 따른 애니메이션 재생
        theCrosshair.FineSightAnimation(isFineSightMode);

        if(isFineSightMode)
        {
            //다른 코루틴 애니메이션 함수가 진행중일 경우 (대부분 Lerp 함수를 통해 보간하며 부드럽게 변화)
            //보간은 딱 떨어지지 않는 경우가 많기 때문에 변화를 마치고 정확한 위치가 될 떄 까지 반복한다면
            //무한 루프가 일어나 두 코루틴 함수가 계속 끝나지 않고 총의 위치가 계속해서 변할 수 있다.
            StopAllCoroutines(); //현재 실행중인 코루틴 함수 모두 중단
            StartCoroutine(FineSightActivateCoroutine()); //정중앙 위치로 총 설정
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(FineSightDeActivateCoroutine()); //정조준 해제를 위해 원래 위치로 총 설정
        }
    }

    public void CancelReload()
    {
        if(isReload)
        {
            StopAllCoroutines();
            isReload = false;
        }
    }

    public void GunChange(Gun _gun)
    {
        if (WeaponManager.currentWeapon != null) //현재 들고있는 무기 있다면 비활성화
            WeaponManager.currentWeapon.gameObject.SetActive(false);

        currentGun = _gun; //자신의 컴포넌트로 WeaponManager 업데이트
        WeaponManager.currentWeapon = currentGun.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentGun.anim;

        currentGun.transform.localPosition = Vector3.zero; //무기 교체후 원점으로 초기화
        currentGun.gameObject.SetActive(true); //현재 총 오브젝트 활성화

        isActivate = true;
    }
    
    IEnumerator FineSightActivateCoroutine()
    {
        while(currentGun.transform.localPosition != currentGun.fineSightOriginPos)
        {//현재 위치를 업데이트 하고 한 프레임 쉬기를 계속 반복
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.2f);
            yield return null;
        }
    }

    IEnumerator FineSightDeActivateCoroutine()
    {
        while (currentGun.transform.localPosition != originPos)
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.2f);
            yield return null;
        }
    }


    //반동 애니메이션
    IEnumerator RetroActionCoroutine()
    {
        Vector3 recoilBack = new Vector3(currentGun.retroActionForce, originPos.y, originPos.z);// 정조준 안 했을 때의 최대 반동
        Vector3 retroActionRecoilBack = new Vector3(currentGun.retroActionFineSightForce, currentGun.fineSightOriginPos.y, currentGun.fineSightOriginPos.z);  // 정조준 했을 때의 최대 반동

        if(!isFineSightMode)  // 정조준이 아닌 상태
        {
            //반동이 이루어지는 상태에서 또 반동이 들어오면(발사가 들어오면) 중복 처리이므로
            //일단 시작하기 전에 원 위치로 되돌려놓는다
            currentGun.transform.localPosition = originPos;

            // 반동 시작
            while(currentGun.transform.localPosition.x <= currentGun.retroActionForce - 0.02f)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, recoilBack, 0.4f); //현재 위치가 recoilBack 위치로 될 때 까지 40퍼센트 비율로 보정
                yield return null;
            }

            // 원위치 - 한번 recoilBack 근처까지 갔다면 다시 원래 위치로 돌아간다
            while (currentGun.transform.localPosition != originPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.1f);//10퍼센트 비율로 보정 - 반동때보다는 좀 더 느리게 돌아옴
                yield return null;
            }
        }
        else  // 정조준 상태
        {
            currentGun.transform.localPosition = currentGun.fineSightOriginPos;

            // 반동 시작
            while(currentGun.transform.localPosition.x <= currentGun.retroActionFineSightForce - 0.02f)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, retroActionRecoilBack, 0.4f);
                yield return null;
            }

            // 원위치
            while (currentGun.transform.localPosition != currentGun.fineSightOriginPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.1f);
                yield return null;
            }
        }
    }

    //재장전 애니메이션이 다 재생될 때 까지 대기해야하므로 코루틴
    IEnumerator ReloadCoroutine()
    {
        if(currentGun.carryBulletCount > 0)
        {
            isReload = true; //이미 재 장전 중일때 다시 재장전 하지 않도록
            currentGun.anim.SetTrigger("Reload");

            //현재 소유한 총알의 개수에 현재 탄창의 총알 더한다
            currentGun.carryBulletCount += currentGun.currentBulletCount;
            //탄창에 총알이 있는 상태에서 재장전 할 경우 대비해서
            currentGun.currentBulletCount = 0; //현채 탄창은 비운다 

            yield return new WaitForSeconds(currentGun.reloadTime);  // 재장전 애니메이션이 다 재생될 동안 대기

            if (currentGun.carryBulletCount >= currentGun.reloadBulletCount)
            {
                currentGun.currentBulletCount = currentGun.reloadBulletCount;
                currentGun.carryBulletCount -= currentGun.reloadBulletCount;
            }
            else
            {
                currentGun.currentBulletCount = currentGun.carryBulletCount;
                currentGun.carryBulletCount = 0;
            }

            isReload = false; //재장전 처리 끝
        }
        else
        {
            Debug.Log("소유한 총알이 없습니다.");
        }
    }

    private void PlaySE(AudioClip _clip)  // 발사 소리 재생
    {
        audioSource.clip = _clip;  // 오디오 클립 할당
        audioSource.Play();       // 오디오 재생
    }
}