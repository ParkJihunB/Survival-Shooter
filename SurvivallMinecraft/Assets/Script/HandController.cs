using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : CloseWeaponController
{
    public static bool isActivate = false;  // 활성화 여부

    private bool isPreview = false; // 현재 프리뷰 활성화 중인지 (프리뷰 중복 생성 방지용)
    private GameObject go_preview;  // 설치할 키트 프리뷰
    private Vector3 previewPos; // 설치할 키트 위치 (크로스헤어 따라 움직이게 계속 갱신할 것)
    [SerializeField] private float rangeAdd;  // 건축시 추가 사정거리(코 앞에 말고 보통의 근접무기의 사정 거리 range보다는 더 먼 곳에 건축할 수 있도록.)
    [SerializeField] private LayerMask preview_Kit_LayerMask; // 키트 건축할 수 있는 곳 레이어 (Building, Terrain)
    public static Item currentKit; // 설치하려는 킷 (연금 테이블)

    [SerializeField] private Hand currentHand; // 현재 장착된 Hand 형 타입 무기
    //private bool isAttack = false;  // 현재 공격 중인지 
    //private bool isSwing = false;  // 팔을 휘두르는 중인지. isSwing = True 일 때만 데미지를 적용할 것이다.
    private RaycastHit hitInfo;  // 현재 무기(Hand)에 닿은 것들의 정보.

    [SerializeField]
    private QuickSlotController theQuickSlotController;
    
    
    private void Start()
    {
        WeaponManager.currentWeapon = currentCloseWeapon.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentCloseWeapon.anim;
    }

    void Update()
    {
        if (isActivate && !Inventory.invectoryActivated)
        {
            if (currentKit == null)
            {
                if (QuickSlotController.go_HandItem == null) // 손에 들린 아이템이 없으면 맨손 공격 가능 상태
                    TryAttack();
                else  // 손에 들린 아이템이 있다면 포션 같은 소비아이템이므로 먹을 수 있는 상태
                    TryEating();
            }
            else 
            {
                if (!isPreview)
                    InstallPreviewKit();
                PreviewPositionUpdate();
                Build();
            }
        }     
    }

    private void InstallPreviewKit()
    {
        isPreview = true;
        go_preview = Instantiate(currentKit.kitPreviewPrefab, transform.position, Quaternion.identity);
    }

    private void PreviewPositionUpdate()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, currentCloseWeapon.range + rangeAdd, preview_Kit_LayerMask))
        {
            previewPos = hitInfo.point;
            go_preview.transform.position = previewPos;
        }
    }

    private void Build()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            if (go_preview.GetComponent<PreviewObject>().isBuildable())
            {
                theQuickSlotController.DecreaseSelectedItem(); // 슬롯 아이템 개수 1 차감
                GameObject temp = Instantiate(currentKit.kitPrefab, previewPos, Quaternion.identity);
                temp.name = currentKit.itemName;
                Destroy(go_preview);
                currentKit = null;
                isPreview = false;
            }
        }
    }

    public void Cancel()
    {
        Destroy(go_preview);
        currentKit = null;
        isPreview = false;
    }

    //한 프레임씩 Hand Holder의 위치로부터 Raycast에 충돌한 오브젝트가 있는지 검사
    protected override IEnumerator HitCoroutine()
    {
        while (isSwing)
        {
            if(CheckObject()) //충돌했다면 끝낸다
            {
                isSwing = false;
                Debug.Log(hitInfo.transform.name);
            }
            yield return null;
        }
    }

    public override void CloseWeaponChange(CloseWeapon _closeWeapon)
    {
        base.CloseWeaponChange(_closeWeapon);
        isActivate = true;
    }

    private void TryEating()
    {
        if (Input.GetButtonDown("Fire2") && theQuickSlotController.GetIsCoolTime())
        {
            currentCloseWeapon.anim.SetTrigger("Eat");
            theQuickSlotController.DecreaseSelectedItem();
        }
    }

    
    //매 프레임 마다 좌클릭 입력이 들어오는지 검사
    //좌클릭 입력시 공격의 과정을 처리하는 코루틴 함수 실행
    // private void TryAttack()
    // {
    //     if(Input.GetButton("Fire1"))
    //     {
    //         if(!isAttack)
    //         {
    //             StartCoroutine(AttackCoroutine());
    //         }
    //     }
    // }

    // public void HandChange(Hand _hand)
    // {
    //     if (WeaponManager.currentWeapon != null)
    //         WeaponManager.currentWeapon.gameObject.SetActive(false);

    //     currentHand = _hand;
    //     WeaponManager.currentWeapon = currentHand.GetComponent<Transform>();
    //     WeaponManager.currentWeaponAnim = currentHand.anim;

    //     currentHand.transform.localPosition = Vector3.zero;
    //     currentHand.gameObject.SetActive(true);

    //     isActivate = true;
    // }

    // IEnumerator AttackCoroutine()
    // {
    //     isAttack = true;
    //     currentHand.anim.SetTrigger("Attack");
    //     //attackDelayA 시간동안 대기한다(팔을 뻗는 부분 재생되기까지 걸리는 시간)
    //     yield return new WaitForSeconds(currentHand.attackDelayA);
    //     isSwing = true; //데미지가 들어가기 시작

    //     StartCoroutine(HitCoroutine()); //HitCoroutine 함수로 Raycast에 충돌 오브젝트가 있는지 검사
    //     //atackDelayB시간동안 추가 대기(뻗은 팔이 돌아오는 시간)
    //     yield return new WaitForSeconds(currentHand.attackDelayB);
    //     isSwing = false; //더이상 데미지가 들어가지 않음

    //     yield return new WaitForSeconds(currentHand.attackDelay - currentHand.attackDelayA - currentHand.attackDelayB); //전체 시간 attackDelay에서 남은 시간 추가 대기
    //     isAttack = false; //공격 처리 과정을 마침
    // }

    // private bool CheckObject()
    // {
    //     if(Physics.Raycast(transform.position, transform.forward, out hitInfo, currentHand.range))
    //     {
    //         return true;
    //     }
       
    //     return false;
    // }
}