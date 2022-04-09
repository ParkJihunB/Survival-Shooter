using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Item item; // 획득한 아이템
    public int itemCount; // 획득한 아이템의 개수
    public Image itemImage;  // 아이템의 이미지

    [SerializeField]
    private Text text_Count;
    [SerializeField]
    private GameObject go_CountImage;
    [SerializeField] 
    private RectTransform baseRect;  // Inventory_Base 의 영역
    [SerializeField] 
    RectTransform quickSlotBaseRect; // 퀵슬롯의 영역. 퀵슬롯 영역의 슬롯들을 묶어 관리하는 'Content' 오브젝트가 할당 됨.
    [SerializeField] private bool isQuickSlot;  // 해당 슬롯이 퀵슬롯인지 여부 판단
    [SerializeField] private int quickSlotNumber;  // 퀵슬롯 넘버

    //private Rect baseRect;  // Inventory_Base 이미지의 Rect 정보 받아 옴.
    private WeaponManager theWeaponManager;
    private InputNumber theInputNumber;
    private ItemEffectDatabase theItemEffectDatabase;
    
    void Start()
    {
        //baseRect = transform.parent.parent.GetComponent<RectTransform>().rect;
        theWeaponManager = FindObjectOfType<WeaponManager>();
        theInputNumber = FindObjectOfType<InputNumber>();
        theItemEffectDatabase = FindObjectOfType<ItemEffectDatabase>();
    }

    public int GetQuickSlotNumber()
    {
        return quickSlotNumber;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right)
        {
            if (item != null)
            {
                if (!isQuickSlot)  // 인벤토리 우클
                {
                    theItemEffectDatabase.UseItem(item);
                    if (item.itemType == Item.ItemType.Used)
                        SetSlotCount(-1);
                } 
                else if (!theItemEffectDatabase.GetIsCoolTime())  // 퀵슬롯 우클 + 쿨타임 중이 아닐 때
                {
                    theItemEffectDatabase.UseItem(item);
                    if (item.itemType == Item.ItemType.Used)
                        SetSlotCount(-1);
                }
            }
        }
    }

    // 마우스 드래그가 시작 됐을 때 발생하는 이벤트
    public void OnBeginDrag(PointerEventData eventData)
    {
        if(item != null && Inventory.invectoryActivated)
        {
            DragSlot.instance.dragSlot = this;
            DragSlot.instance.DragSetImage(itemImage);
            DragSlot.instance.transform.position = eventData.position;
        }
    }

    // 마우스 드래그 중일 때 계속 발생하는 이벤트
    public void OnDrag(PointerEventData eventData)
    {
        if (item != null)
            DragSlot.instance.transform.position = eventData.position;
    }

    // 마우스 드래그가 끝났을 때 발생하는 이벤트
    public void OnEndDrag(PointerEventData eventData)
    {

        // 인벤토리와 퀵슬롯 영역을 벗어난 곳에서 드래그를 끝냈다면
        if (!((DragSlot.instance.transform.localPosition.x > baseRect.rect.xMin 
            && DragSlot.instance.transform.localPosition.x < baseRect.rect.xMax
            && DragSlot.instance.transform.localPosition.y > baseRect.rect.yMin
            && DragSlot.instance.transform.localPosition.y < baseRect.rect.yMax)
            ||
            (DragSlot.instance.transform.localPosition.x > quickSlotBaseRect.rect.xMin
            && DragSlot.instance.transform.localPosition.x < quickSlotBaseRect.rect.xMax
            && DragSlot.instance.transform.localPosition.y + baseRect.transform.localPosition.y > quickSlotBaseRect.rect.yMin + quickSlotBaseRect.transform.localPosition.y
            && DragSlot.instance.transform.localPosition.y + baseRect.transform.localPosition.y < quickSlotBaseRect.rect.yMax + quickSlotBaseRect.transform.localPosition.y)))
        {
            if (DragSlot.instance.dragSlot != null)
                theInputNumber.Call();
        }
        // 인벤토리 혹은 퀵슬롯 영역에서 드래그가 끝났다면
        else
        {
            DragSlot.instance.SetColor(0);
            DragSlot.instance.dragSlot = null;
        }

    }

        // 해당 슬롯에 무언가가 마우스 드롭 됐을 때 발생하는 이벤트
    public void OnDrop(PointerEventData eventData)
    {
        if (DragSlot.instance.dragSlot != null)
            ChangeSlot();
    }

    // 마우스 커서가 슬롯에 들어갈 때 발동
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null)
            theItemEffectDatabase.ShowToolTip(item, transform.position);
    }

    // 마우스 커서가 슬롯에서 나올 때 발동
    public void OnPointerExit(PointerEventData eventData)
    {
        theItemEffectDatabase.HideToolTip();
    }

    // 아이템 이미지의 투명도 조절
    private void SetColor(float _alpha)
    {
        Color color = itemImage.color;
        color.a = _alpha;
        itemImage.color = color;
    }
    
    private void ChangeSlot()
    {
        Item _tempItem = item;
        int _tempItemCount = itemCount;

        AddItem(DragSlot.instance.dragSlot.item, DragSlot.instance.dragSlot.itemCount);

        if (_tempItem != null)
            DragSlot.instance.dragSlot.AddItem(_tempItem, _tempItemCount);
        else
            DragSlot.instance.dragSlot.ClearSlot();
    }

    // 인벤토리에 새로운 아이템 슬롯 추가
    public void AddItem(Item _item, int _count = 1)
    {
        item = _item;
        itemCount = _count;
        itemImage.sprite = item.itemImage;

        if(item.itemType != Item.ItemType.Equipment)
        {
            go_CountImage.SetActive(true);
            text_Count.text = itemCount.ToString();
        }
        else
        {
            text_Count.text = "0";
            go_CountImage.SetActive(false);
        }

        SetColor(1);
    }

    // 해당 슬롯의 아이템 갯수 업데이트
    public void SetSlotCount(int _count)
    {
        itemCount += _count;
        text_Count.text = itemCount.ToString();

        if (_count < 0)
        {
            if (theItemEffectDatabase.GetIsFull())
                theItemEffectDatabase.SetIsFull(false);
        }

        if (itemCount <= 0)
        {
            ClearSlot();
            if (isQuickSlot)
                if (QuickSlotController.go_HandItem != null)
                    if (QuickSlotController.go_HandItem.GetComponent<ItemPickUp>().item.itemType == Item.ItemType.Used)
                        Destroy(QuickSlotController.go_HandItem);
        }  
    }

    // 해당 슬롯 하나 삭제
    private void ClearSlot()
    {
        item = null;
        itemCount = 0;
        itemImage.sprite = null;
        SetColor(0);

        text_Count.text = "0";
        go_CountImage.SetActive(false);

        if (theItemEffectDatabase.GetIsFull())
            theItemEffectDatabase.SetIsFull(false);
    }
}