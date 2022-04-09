using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickSlotController : MonoBehaviour
{
    // 퀵슬롯들 (8개) 
    [SerializeField] private Slot[] quickSlots; public Slot GetSelectedSlot(){return quickSlots[selectedSlot];}
    [SerializeField] private Transform tf_parent;  // 퀵슬롯들의 부모 오브젝트
    private int selectedSlot;  // 선택된 퀵슬롯의 인덱스 (0~7)
    [SerializeField] private GameObject go_SelectedImage;  // 선택된 퀵슬롯 이미지

    [SerializeField] private Image[] img_CoolTime;  // 퀵슬롯 쿨타임 이미지들 
    // 쿨타임 내용
    [SerializeField]
    private float coolTime;  // 정해짐 쿨타임  [SerializeField]로 유니티 인스펙터에서 결정
    private float currentCoolTime;  // coolTime 을 시작점으로 0 이 될 때까지 감소 업뎃
    private bool isCoolTime;  // 현재 쿨타임 중인지
    // 퀵슬롯 등장 내용
    [SerializeField] private float appearTime;  // 퀵슬롯이 나타나는 동안의 시간
    private float currentAppearTime;
    private bool isAppear;

    [SerializeField]
    private WeaponManager theWeaponManager;
    [SerializeField] private Transform tf_ItemPos;  //손 끝 오브젝트. 손 끝에 아이템이 위치도록
    public static GameObject go_HandItem;   //손에 든 아이템. static인 이유는 이거 하나 받아오려고 📜QuickSlotController 로딩하는건 낭비
    [SerializeField]
    private ItemEffectDatabase theItemEffectDatabase;

    private Animator anim;

    void Start()
    {
        quickSlots = tf_parent.GetComponentsInChildren<Slot>();
        selectedSlot = 0; 
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        TryInputNumber();
        CoolTimeCalc();
        AppearCalc();
    }

    private void TryInputNumber()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            ChangeSlot(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            ChangeSlot(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            ChangeSlot(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            ChangeSlot(3);
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            ChangeSlot(4);
        else if (Input.GetKeyDown(KeyCode.Alpha6))
            ChangeSlot(5);
        else if (Input.GetKeyDown(KeyCode.Alpha7))
            ChangeSlot(6);
        else if (Input.GetKeyDown(KeyCode.Alpha8))
            ChangeSlot(7);
    }

    private void ChangeSlot(int _num)
    {
        SelectedSlot(_num);
        Execute();
    }

    private void SelectedSlot(int _num)
    {
        // 선택된 슬롯
        selectedSlot = _num;

        // 선택된 슬롯으로 이미지 이동
        go_SelectedImage.transform.position = quickSlots[selectedSlot].transform.position;
    }

    public void IsActivatedQuickSlot(int _num)
    {
        if (selectedSlot == _num)
        {
            Execute();
            return;
        }
        if (DragSlot.instance != null)
        {
            if (DragSlot.instance.dragSlot != null)
            {
                if (DragSlot.instance.dragSlot.GetQuickSlotNumber() == selectedSlot)
                {
                    Execute();
                    return;
                }
            }
        }  
    }   

    private void Execute()
    {
        CoolTimeReset();
        AppearReset();

        if (quickSlots[selectedSlot].item != null)
        {
            if (quickSlots[selectedSlot].item.itemType == Item.ItemType.Equipment)
                StartCoroutine(theWeaponManager.ChangeWeaponCoroutine(quickSlots[selectedSlot].item.weaponType, quickSlots[selectedSlot].item.itemName));
            else if (quickSlots[selectedSlot].item.itemType == Item.ItemType.Used || quickSlots[selectedSlot].item.itemType == Item.ItemType.Kit)
                ChangeHand(quickSlots[selectedSlot].item);
            else
                ChangeHand();
        }
        else
        {
            ChangeHand();
        }
    }

    private void ChangeHand(Item _item = null)
    {
        StartCoroutine(theWeaponManager.ChangeWeaponCoroutine("HAND", "맨손"));

        if (_item != null)
        {
            StartCoroutine(HandItemCoroutine(_item));
        }
    }

    IEnumerator HandItemCoroutine(Item _item)
    {
        HandController.isActivate = false;
        yield return new WaitUntil(() => HandController.isActivate);  // 맨손 교체의 마지막 과정

        if (_item.itemType == Item.ItemType.Kit)
            HandController.currentKit = _item;

        go_HandItem = Instantiate(quickSlots[selectedSlot].item.itemPrefab, tf_ItemPos.position, tf_ItemPos.rotation);
        go_HandItem.GetComponent<Rigidbody>().isKinematic = true;  // 중력 영향 X 
        go_HandItem.GetComponent<Collider>().enabled = false;  // 콜라이더 끔 (플레이어와 충돌하지 않게)
        go_HandItem.tag = "Untagged";   // 획득 안되도록 레이어 태그 바꿈
        go_HandItem.layer = 6;  // "Weapon" 레이어는 int
        go_HandItem.transform.SetParent(tf_ItemPos);
    }

    public void DecreaseSelectedItem()
    {
        theItemEffectDatabase.UseItem(quickSlots[selectedSlot].item);
        quickSlots[selectedSlot].SetSlotCount(-1);

        if (quickSlots[selectedSlot].itemCount <= 0)
            Destroy(go_HandItem);
    }

    private void CoolTimeReset()
    {
        currentCoolTime = coolTime;
        isCoolTime = true;
    }

    private void CoolTimeCalc()
    {
        if (isCoolTime)
        {
            currentCoolTime -= Time.deltaTime;  // 1 초에 1 씩 감소

            for (int i = 0; i < img_CoolTime.Length; i++)
                img_CoolTime[i].fillAmount = currentCoolTime / coolTime;

            if (currentCoolTime <= 0)
                isCoolTime = false;
        }
    }

    public bool GetIsCoolTime()
    {
        return isCoolTime;
    }

    private void AppearReset()
    {
        currentAppearTime = appearTime;
        isAppear = true;
        anim.SetBool("Appear", isAppear);
    }

    private void AppearCalc()
    {
        if (Inventory.invectoryActivated)  // 인벤토리 켜져있을 땐 퀵슬롯도 늘 활성화
            AppearReset();
        else  // 인벤토리가 켜져 있지 않을때만 쿨타임 깎아야 함
        {
            if (isAppear)
            {
                currentAppearTime -= Time.deltaTime; // 1초에 1감소
                if (currentAppearTime <= 0)
                {
                    isAppear = false;
                    anim.SetBool("Appear", isAppear);
                }
            }
        }
        
    }
}