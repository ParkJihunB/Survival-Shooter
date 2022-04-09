using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 자동으로 이 스크립트가 활성화되면 GunController.cs 가 컴포넌트로 붙게 된다.
//실수 방지(GunController.cs가 필요하다고 강제)
[RequireComponent(typeof(GunController))]       
public class WeaponManager : MonoBehaviour
{
    //무기의 종류가 어떤 것이든(Gun,Hand...) 모든 무기들이 현재 교체중인지 알아야 한다.
    public static bool isChangeWeapon = false; //무기 교체 중인지(중복실행 방지-True면 못하게)

    [SerializeField]
    private float changeweaponDelayTime;  //무기 교체 딜레이 시간. 대략 Weapon_Out 애니메이션 시간.
    [SerializeField]
    private float changeweaponEndDelayTime;  //무기 교체가 완전히 끝난 시점. 대략 Weapon_In 애니메이션 시간.

    [SerializeField]
    private Gun[] guns;  //모든 종류의 총을 원소로 가지는 배열
    [SerializeField]
    private CloseWeapon[] hands;  //모든 종류의 Hand 형 무기를 가지는 배열
    [SerializeField]
    private CloseWeapon[] axes;  // 도끼 형 무기를 가지는 배열
    [SerializeField]
    private CloseWeapon[] pickaxes;  // 곡괭이 형 무기를 가지는 배열

    // 관리 차원에서 이름으로 쉽게 무기 접근이 가능하도록 Dictionary 자료 구조 사용.
    private Dictionary<string, Gun> gunDictionary = new Dictionary<string, Gun>();
    private Dictionary<string, CloseWeapon> handDictionary = new Dictionary<string, CloseWeapon>();
    private Dictionary<string, CloseWeapon> axeDictionary = new Dictionary<string, CloseWeapon>();
    private Dictionary<string, CloseWeapon> pickaxeDictionary = new Dictionary<string, CloseWeapon>();

    [SerializeField]
    private string currentWeaponType;  //현재 무기의 타입
    //무기의 종류에 상관없이 모두 Transform 컴포넌트를 가지고 있다.
    //Controller.cs가 자기자신(또는 자신의 Animator 컴포넌트)을 할당
    public static Transform currentWeapon;  //현재 무기. 여러 스크립트에서 클래스 이름으로 바로 접근
    public static Animator currentWeaponAnim; //현재 무기의 애니메이션. 여러 스크립트에서 클래스 이름으로 바로 접근

    //무기 교체시 어떤 무기를 활성화, 비활성화 할지 알아야 한다.
    [SerializeField]
    private GunController theGunController;  //총 일땐 GunController.cs 활성화, 손일 땐 📜GunController.cs 비활성화 
    [SerializeField]
    private HandController theHandController; //손 일땐 HandController.cs 활성화, HandController.cs 비활성화
    [SerializeField]
    private AxeController theAxeController; // 도끼 일땐 📜AxeController.cs 활성화, 다른 무기일 땐 📜AxeController.cs 비활성화
    [SerializeField]
    private PickaxeController thePickaxeController; // 곡괭이 일땐 📜PickaxeController.cs 활성화, 다른 무기일 땐 📜PickaxeController.cs 비활성화

    void Start()
    {
        for (int i = 0; i < guns.Length; i++)
        {
            gunDictionary.Add(guns[i].gunName, guns[i]);
        }
        for (int i = 0; i < hands.Length; i++)
        {
            handDictionary.Add(hands[i].closeWeaponName, hands[i]);
        }
        for (int i = 0; i < axes.Length; i++)
        {
            axeDictionary.Add(axes[i].closeWeaponName, axes[i]);
        }
        for (int i = 0; i < pickaxes.Length; i++)
        {
            pickaxeDictionary.Add(pickaxes[i].closeWeaponName, pickaxes[i]);
        }

        thePickaxeController.CloseWeaponChange(pickaxeDictionary["Pickaxe"]);
    }

    void Update()
    {
        // if(!Inventory.invectoryActivated)
        // {
        //     //무기 교체중이 아닐 때만 키 입력을 받는다
        //     if(!isChangeWeapon)
        //     {
        //         if (Input.GetKeyDown(KeyCode.Alpha1)) // 1 누르면 '맨손'으로 무기 교체 실행
        //         {
        //             StartCoroutine(ChangeWeaponCoroutine("HAND", "맨손"));
        //         }
        //         else if (Input.GetKeyDown(KeyCode.Alpha2)) // 2 누르면 '서브 머신건'으로 무기 교체 실행
        //         {
        //             StartCoroutine(ChangeWeaponCoroutine("GUN", "SubMachineGun1"));
        //         }
        //         else if (Input.GetKeyDown(KeyCode.Alpha3)) // 3 누르면 '도끼'로 무기 교체 실행
        //             StartCoroutine(ChangeWeaponCoroutine("AXE", "Axe"));
        //         else if (Input.GetKeyDown(KeyCode.Alpha4)) // 4 누르면 '곡괭이'로 무기 교체 실행
        //             StartCoroutine(ChangeWeaponCoroutine("PICKAXE", "Pickaxe"));
        //     }
        // }

    }

    public IEnumerator ChangeWeaponCoroutine(string _type, string _name)
    {
        isChangeWeapon = true;
        currentWeaponAnim.SetTrigger("Weapon_Out"); //무기 꺼내는 애니메이션 재생

        yield return new WaitForSeconds(changeweaponDelayTime); //무기 넣는 애니메이션 재생할 동안 대기

        CancelPreWeaponAction(); //기존의 무기 해제
        WeaponChange(_type, _name); //바꾸고자 하는 무기로 교체

        yield return new WaitForSeconds(changeweaponEndDelayTime); //무기 꺼내는 애니메이션 재생 대기

        currentWeaponType = _type; //현재 무기 타입 업데이트
        isChangeWeapon = false; //무기 교체 과정 끝났으므로 무기 교체 가능
    }

    //들고 있던 무기 내리기
    private void CancelPreWeaponAction()
    {
        switch(currentWeaponType)
        {
            case "GUN":
                theGunController.CancelFineSight(); //정조준 해제
                theGunController.CancelReload(); //재장전 해제
                GunController.isActivate = false; //해당 무기 비활성화
                break;
            case "HAND":
                HandController.isActivate = false; //해당 무기 비활성화
                if (HandController.currentKit != null)
                    theHandController.Cancel();
                if (QuickSlotController.go_HandItem != null)
                    Destroy(QuickSlotController.go_HandItem);
                break;
             case "AXE":
                AxeController.isActivate = false;
                break;
            case "PICKAXE":
                PickaxeController.isActivate = false;
                break;
        }
    }

    private void WeaponChange(string _type, string _name)
    {

        if(_type == "GUN")
        {
            theGunController.GunChange(gunDictionary[_name]);
        }
        else if(_type == "HAND")
        {
            theHandController.CloseWeaponChange(handDictionary[_name]);
        }
        else if (_type == "AXE")
        {
            theAxeController.CloseWeaponChange(axeDictionary[_name]);
        }
        else if (_type == "PICKAXE")
        {
            thePickaxeController.CloseWeaponChange(pickaxeDictionary[_name]);
        }
    }
}