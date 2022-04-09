using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static bool invectoryActivated = false;  // 인벤토리 활성화 여부. true가 되면 카메라 움직임과 다른 입력을 막을 것이다.

    [SerializeField]
    private GameObject go_InventoryBase; // Inventory_Base 이미지
    [SerializeField] 
    private GameObject go_SlotsParent;  // Slot들의 부모인 Grid Setting 
    [SerializeField]
    private GameObject go_QuickSlotParent;  // 퀵슬롯 영역

    private Slot[] slots;  // 인벤토리 슬롯들
    private Slot[] quickSlots; // 퀵슬롯의 슬롯들
    private bool isNotPut;
    private bool isFull = false;  // 인벤토리 퀵슬롯 모두 꽉 찼는지

    [SerializeField]
    private ActionController theActionController;
    [SerializeField]
    private QuickSlotController theQuickSlotController;
    private int slotNumber;

    void Start()
    {
        slots = go_SlotsParent.GetComponentsInChildren<Slot>();
        quickSlots = go_QuickSlotParent.GetComponentsInChildren<Slot>();
    }

    void Update()
    {
        TryOpenInventory();
    }

    private void TryOpenInventory()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            invectoryActivated = !invectoryActivated;

            if (invectoryActivated)
                OpenInventory();
            else
                CloseInventory();

        }
    }

    private void OpenInventory()
    {
        GameManager.isOpenInventory = true;
        go_InventoryBase.SetActive(true);
    }

    private void CloseInventory()
    {
        GameManager.isOpenInventory = false;
        go_InventoryBase.SetActive(false);
    }
    
    public void AcquireItem(Item _item, int _count = 1)
    {
        PutSlot(quickSlots, _item, _count);
        if (!isNotPut)
            theQuickSlotController.IsActivatedQuickSlot(slotNumber);
        if (isNotPut)
            PutSlot(slots, _item, _count);

        if (isNotPut)
        {
            isFull = true;
            StartCoroutine(theActionController.WhenInventoryIsFull());
        }    
    }

    public bool GetIsFull()
    {
        return isFull;
    }

    public void SetIsFull(bool _flag)
    {
        isFull = _flag;
    }

    private void PutSlot(Slot[] _slots, Item _item, int _count)
    {
        if (Item.ItemType.Equipment != _item.itemType && Item.ItemType.Kit != _item.itemType)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].item != null) //null 이라면 slots[i].item.itemName 할 때 런타임 에러
                {
                    if (_slots[i].item.itemName == _item.itemName)
                    {
                        slotNumber = i;
                        _slots[i].SetSlotCount(_count);
                        isNotPut = false;
                        return;
                    }
                }
            }
        }

        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].item == null)
            {
                _slots[i].AddItem(_item, _count);
                isNotPut = false;
                return;
            }
        }

        isNotPut = true;
    }

    public int GetItemCount(string _itemName)
    {
        int temp = SearchSlotItem(slots, _itemName);
        return temp != 0 ? temp : SearchSlotItem(quickSlots, _itemName);
    }

    private int SearchSlotItem(Slot[] _slots, string _itemName)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].item != null)
            {
                if (_itemName == _slots[i].item.itemName)
                    return _slots[i].itemCount;
            }
        }

        return 0;
    }

    public void SetItemCount(string _itemName, int _itemCount)
    {
        if (!ItemCountAdjust(slots, _itemName, _itemCount))
            ItemCountAdjust(quickSlots, _itemName, _itemCount);
    }

    private bool ItemCountAdjust(Slot[] _slots, string _itemName, int _itemCount)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].item != null)
            {
                if (_itemName == _slots[i].item.itemName)
                {
                    _slots[i].SetSlotCount(-_itemCount);
                    return true;
                }
            }
        }
        return false; // 인벤토리에 없어서 퀵슬롯에서 빼야 됨
    }
}