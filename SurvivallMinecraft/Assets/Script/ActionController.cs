using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionController : MonoBehaviour
{
    [SerializeField]
    private float range;  // 아이템 습득이 가능한 최대 거리
    private bool pickupActivated = false;  // 아이템 습득 가능할시 True 
    private bool dissolveActivated = false; // 고기 해체 가능할 시 True (돼지 시체를 바라 볼 때)
    private bool isDissolving = false;  // 고기 해체 중일 때 True (중복해서 해체하지 않도록)
    private bool firedLookActivated = false; // 불에 접근해서 불을 바라볼 시 True
    private bool lookComputer = false; // 컴퓨터를 바라볼 시 true
    private bool lookArchemyTable = false;
    
    [SerializeField]
    private Transform tf_MeatDissolveTool;  // 고기 해체 손. 즉 Meat Knife. 고기 해체할 때 활성화 해야 함
    private RaycastHit hitInfo;  // 충돌체 정보 저장
    [SerializeField]
    private LayerMask layerMask;  // 특정 레이어를 가진 오브젝트에 대해서만 습득할 수 있어야 한다.
    [SerializeField]
    private string sound_meat; // 고기 해체 소리

    [SerializeField]
    private Text actionText;  // 행동을 보여 줄 텍스트
    [SerializeField]
    private Text itemFullText;  // 아이템이 꽉 찼다는 경고 메세지를 보여줄 텍스트
    [SerializeField]
    private Inventory theInventory;  // 📜Inventory.cs

    [SerializeField]
    private QuickSlotController theQuickSlotController; // 📜QuickSlotController.cs
    [SerializeField] private ComputerKit theComputer; // 📜ComputerKit.cs


    void Update()
    {
        CheckAction();// 고기 or 아이템 액션 텍스트 띄우기 시도
        TryAction();
    }

    private void CheckAction()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, range, layerMask))
        {
            if (hitInfo.transform.tag == "Item")
                ItemInfoAppear();
            else if (hitInfo.transform.tag == "Weak_Animal")
                MeatInfoAppear();
            else if (hitInfo.transform.tag == "Fire")
                FireInfoAppear();
            else if (hitInfo.transform.tag == "Computer")
                ComputerInfoAppear();
            else if (hitInfo.transform.tag == "ArchemyTable")
                ArchemyInfoAppear();
            else
                InfoDisappear();
        }
        else
            InfoDisappear();
    }

    private void TryAction()
    {
        if(Input.GetKeyDown(KeyCode.E))  // E 키 입력이 들어 오면 
        {
            CheckAction();   // 고기 or 아이템 액션 텍스트 띄우기 시도
            CanPickUp();  // 아이템을 주울 수 있는지 
            CanMeat();  // 고기를 주울 수 있는지
            CanDropFire();
            CanComputerPowerOn();
            CanArchemyTableOpen();
        }
    }
    
    private void CheckItem()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, range, layerMask))
        {
            if (hitInfo.transform.tag == "Item")
            {
                ItemInfoAppear();
            }
        }
        else
            ItemInfoDisappear();
    }

    private void ArchemyInfoAppear()
    {
        if (!hitInfo.transform.GetComponent<ArchemyTable>().GetIsOpen())
        {
            Reset();
            lookArchemyTable = true;
            actionText.gameObject.SetActive(true);
            actionText.text = "연금 테이블 조작 " + "<color=yellow>" + "(E)" + "</color>";
        }
    }
    private void CanArchemyTableOpen()
    {
        if (lookArchemyTable)
        {
            if (hitInfo.transform != null)
            {
                hitInfo.transform.GetComponent<ArchemyTable>().Window();
                InfoDisappear();
            }
        }
    }

    private void ItemInfoAppear()
    {
        Reset();
        pickupActivated = true;
        actionText.gameObject.SetActive(true);
        actionText.text = hitInfo.transform.GetComponent<ItemPickUp>().item.itemName + " 획득 " + "<color=yellow>" + "(E)" + "</color>";
    }

    private void ItemInfoDisappear()
    {
        pickupActivated = false;
        dissolveActivated = false;
        firedLookActivated = false;
        lookComputer = false;
        actionText.gameObject.SetActive(false);
    }

    private void ComputerInfoAppear()
    {
        if (!hitInfo.transform.GetComponent<ComputerKit>().isPowerOn)
        {
            Reset();
            lookComputer = true;
            actionText.gameObject.SetActive(true);
            actionText.text = "컴퓨터 가동 " + "<color=yellow>" + "(E)" + "</color>";
        }
    }

    private void CanComputerPowerOn()
    {
        if (lookComputer)
        {
            if (hitInfo.transform != null)
            {
                if (!hitInfo.transform.GetComponent<ComputerKit>().isPowerOn)
                {
                    hitInfo.transform.GetComponent<ComputerKit>().PowerOn();
                    InfoDisappear();
                }
            }
        }
    }

    private void CanPickUp()
    {
        if(pickupActivated)
        {
            if(hitInfo.transform != null)
            {
                Debug.Log(hitInfo.transform.GetComponent<ItemPickUp>().item.itemName + " 획득 했습니다.");
                theInventory.AcquireItem(hitInfo.transform.GetComponent<ItemPickUp>().item);
                Destroy(hitInfo.transform.gameObject);
                InfoDisappear();
            }
        }
    }

    public IEnumerator WhenInventoryIsFull()
    {
        itemFullText.gameObject.SetActive(true);
        itemFullText.text = "아이템이 꽉 찼습니다.";

        yield return new WaitForSeconds(3.0f);  // 3 초 후 메세지는 사라짐. 메세지는 3 초만 띄움.
        itemFullText.gameObject.SetActive(false);
    }

    private void Reset()
    {
        pickupActivated = false;
        dissolveActivated = false;
        firedLookActivated = false;
    }


    private void MeatInfoAppear()
    {
        if (hitInfo.transform.GetComponent<Animal>().isDead) // 죽은 돼지를 바라봤을 경우에만 (카메라에서 쏜 Raycast가 죽은 돼지와 충돌)
        {
            Reset();
            dissolveActivated = true;
            actionText.gameObject.SetActive(true);
            actionText.text = hitInfo.transform.GetComponent<Animal>().animalName + " 해체하기 " + "<color=yellow>" + "(E)" + "</color>";
        }
    }

    private void FireInfoAppear()
    {
        Reset();
        firedLookActivated = true;
        
        if (hitInfo.transform.GetComponent<Fire>().GetIsFire())
        {
            actionText.gameObject.SetActive(true);
            actionText.text = "선택된 아이템 불에 넣기" + "<color=yellow>" + "(E)" + "</color>";
        }
    }

    private void InfoDisappear()
    {
        pickupActivated = false;
        dissolveActivated = false;
        firedLookActivated = false;
        lookComputer = false;
        actionText.gameObject.SetActive(false);
    }

    IEnumerator MeatCoroutine()
    {
        WeaponManager.isChangeWeapon = true;  // 고기 해체 중에 무기가 교체되지 않도록
        WeaponSway.isActivated = false;

        // 들고 있던 무기 비활
        WeaponManager.currentWeaponAnim.SetTrigger("Weapon_Out");
        PlayerController.isActivated = false;
        yield return new WaitForSeconds(0.2f);  // 애니메이션 재생 후 비활되도록
        WeaponManager.currentWeapon.gameObject.SetActive(false);

        // 칼 꺼내기
        tf_MeatDissolveTool.gameObject.SetActive(true);  // 애니메이션은 이때 자동으로 실행됨 (디폴트상태니까)
        yield return new WaitForSeconds(0.2f);  // 애니메이션 0.2초 진행 후
        SoundManager.instance.PlaySE(sound_meat);  // 고기 해체 소리
        yield return new WaitForSeconds(1.8f);  // 칼 해체하는 애니메이션 다 끝나길 기다림

        // 고기 아이템 얻기
        theInventory.AcquireItem(hitInfo.transform.GetComponent<WeakAnimal>().GetItem(), hitInfo.transform.GetComponent<WeakAnimal>().itemNumber);

        // 칼 해체 손은 집어 넣고 다시 원래 무기로
        WeaponManager.currentWeapon.gameObject.SetActive(true);
        tf_MeatDissolveTool.gameObject.SetActive(false);

        PlayerController.isActivated = true;
        WeaponSway.isActivated = true;
        WeaponManager.isChangeWeapon = false;  // 다시 무기 교체가 가능하도록
        isDissolving = false;
    }

    private void CanMeat()
    {
        if  (dissolveActivated)
        {
            if (hitInfo.transform.tag == "Weak_Animal" && hitInfo.transform.GetComponent<Animal>().isDead && !isDissolving)
            {
                isDissolving = true;
                InfoDisappear();
                
                StartCoroutine(MeatCoroutine()); // 고기 해체 실시
            }
        }
    }

    private void CanDropFire()
    {
        if (firedLookActivated)
        {
            if (hitInfo.transform.tag == "Fire" && hitInfo.transform.GetComponent<Fire>().GetIsFire())
            {
                // 손에 들고 있는 아이템을 불에 넣음 <- 선택된 퀵슬롯의 아이템을 넣음 (Null 빈슬롯일 수도 있음. 판별이 필요)
                Slot _selectedSlot = theQuickSlotController.GetSelectedSlot();

                if (_selectedSlot.item != null)
                    DropAnItem(_selectedSlot);
            }
        }
    }

    private void DropAnItem(Slot _selectedSlot)
    {
        switch(_selectedSlot.item.itemType)
        {
            case Item.ItemType.Used:
                if (_selectedSlot.item.itemName.Contains("고기"))
                {
                    Instantiate(_selectedSlot.item.itemPrefab, hitInfo.transform.position + Vector3.up, Quaternion.identity);
                    theQuickSlotController.DecreaseSelectedItem();
                }
                break;
            case Item.ItemType.Ingredient:
                break;
        }
    }
}
