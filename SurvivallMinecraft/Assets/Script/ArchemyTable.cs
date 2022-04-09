using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ArchemyItem
{
    public string itemName;
    public string itemDescription;
    public Sprite itemImage;

    public string[] needItemName;
    public int[] needItemNumber;

    public float itemCraftingTime; // 포션 제조에 걸리는 시간 (5초, 10초, 100초)

    public GameObject go_ItemPrefab; // 실제 생성될 포션
}

public class ArchemyTable : MonoBehaviour
{
    private bool isOpen = false;
    private bool isCrafting = false; // 아이템의 제작 시작 여부 (ture면 제작 中)

    private Queue<ArchemyItem> archemyItemQueue = new Queue<ArchemyItem>(); // 연금 테이블 아이템 제작 대기열 큐
    private ArchemyItem currentCraftingItem;  // 현재 제작 중인 연금 아이템(큐의 첫 번째 원소)

    private float craftingTime;  // 제작 시간
    private float currentCraftingTime; // 실제 갱신되는 시간. craftingTime 가 되기까지 갱신됨
    private int page = 1; // 현재 페이지

    [SerializeField] private Slider slider_gauge; // 슬라이더 게이지
    [SerializeField] private GameObject go_Liquid; // 동작 시키면 액체 등장(포션 제작 중이면 등장)
    [SerializeField] private Image[] image_CraftingItems; // 대기열 슬롯에 있는 아이템 이미지들
    [SerializeField] private ArchemyItem[] archemyItems;  // 제작할 수 있는 연금 아이템 리스트
    [SerializeField] private Transform tf_BaseUI; // 연금 아이템 베이스 UI
    [SerializeField] private Transform tf_PotionAppearPos; // 포션이 생성될 위치

    [SerializeField] private int theNumberOfSlot; // 한 페이지당 슬롯의 최대 개수(4개)
    [SerializeField] private Image[] image_ArchemyItems; // 페이지에 따른 포션 이미지들(4개 사용)
    [SerializeField] private Text[] text_ArchemyItems; // 페이지에 따른 포션 텍스트들(4개 사용)
    [SerializeField] private Button[] btn_ArchemyItems; // 페이지에 따른 포션 버튼들(4개 사용)

    [SerializeField] private ArchemyToolTip theToolTip;
    private Inventory theInventory;

    private AudioSource theAudio;
    [SerializeField] private AudioClip sound_ButtonClick; // CREATE 버튼 클릭시 효과음
    [SerializeField] private AudioClip sound_Beep; // 아이템 부족으로 생성 불가할 때 효과음
    [SerializeField] private AudioClip sound_Activate; // 아이템을 만들기 시작했을 떄 효과음
    [SerializeField] private AudioClip sound_ExitItem; // 아이템이 완성되어 빠져나올 때 효과음

    void Start()
    {
        theInventory = FindObjectOfType<Inventory>();
        theAudio = GetComponent<AudioSource>();
        ClearSlot();
        PageSetting();
    }

    void Update()
    {
        if (!isFinish())
        {
            Crafting();
        }
        if (isOpen)
            if (Input.GetKeyDown(KeyCode.Escape))
                CloseWindow();
    }

    private bool isFinish()
    {
        if(archemyItemQueue.Count == 0 && !isCrafting)
        {
            go_Liquid.SetActive(false);
            slider_gauge.gameObject.SetActive(false);
            return true;
        }
        else
        {
            go_Liquid.SetActive(true);
            slider_gauge.gameObject.SetActive(true);
            return false;
        }
    }

    private void Crafting()
    {
        if (!isCrafting && archemyItemQueue.Count != 0)
            DequeueItem();

        if (isCrafting) // DequeItem 을 통해 isCrafting = true 즉 제작 중이된 후
        {
            currentCraftingTime += Time.deltaTime;
            slider_gauge.value = currentCraftingTime;

            if (currentCraftingTime >= craftingTime)
                ProductionComplete();
        }
    }

    private void DequeueItem()
    {
        PlaySE(sound_Activate);
        isCrafting = true; // 대기열에서 뺏으니 제작 시작
        currentCraftingItem = archemyItemQueue.Dequeue();

        craftingTime = currentCraftingItem.itemCraftingTime;
        currentCraftingTime = 0;
        slider_gauge.maxValue = craftingTime;

        CraftingImageChange();
    }

    private void CraftingImageChange()
    {
        image_CraftingItems[0].gameObject.SetActive(true); // 첫 번재 대기열 이미지는 무조건 보여져야 함. 제작 중이니까

        for (int i = 0; i < archemyItemQueue.Count + 1; i++) // 이미지는 그대로 남아있으니까 +1 (위에서 Dequeue 하기 전 상태)
        {
            image_CraftingItems[i].sprite = image_CraftingItems[i + 1].sprite;  // 이미지 하나씩 땡겨오기
            
            if (i + 1 == archemyItemQueue.Count + 1)
                image_CraftingItems[i + 1].gameObject.SetActive(false);
        }
    }

    public void Window()
    {
        isOpen = !isOpen;
        if (isOpen)
            OpenWindow();
        else
            CloseWindow();
    }

    private void OpenWindow()
    {
        isOpen = true;
        GameManager.isOpenArchemyTable = true;
        tf_BaseUI.localScale = new Vector3(1f, 1f, 1f);
    }

    private void CloseWindow()
    {
        isOpen = false;
        GameManager.isOpenArchemyTable = false;
        tf_BaseUI.localScale = new Vector3(0f, 0f, 0f); // 창 비활성화 하면 안됨. 아직 포션 제조중일 수 있어서 크기만 줄여 둠
    }

    public void Buttonclick(int _buttonNum)
    {
        PlaySE(sound_ButtonClick);

        if (archemyItemQueue.Count < 3)
        {
            int archemyItemArrayNumber = _buttonNum + (page - 1) * theNumberOfSlot;

            // 인벤토리에서 재료 검색
            for (int i = 0; i < archemyItems[archemyItemArrayNumber].needItemName.Length; i++)
            {
                if (theInventory.GetItemCount(archemyItems[archemyItemArrayNumber].needItemName[i]) < archemyItems[archemyItemArrayNumber].needItemNumber[i])
                {
                    PlaySE(sound_Beep);
                    return;  // 제작 안됨
                }
            }

            // 인벤토리에서 재료 차감
            for (int i = 0; i < archemyItems[archemyItemArrayNumber].needItemName.Length; i++)
            {
                theInventory.SetItemCount(archemyItems[archemyItemArrayNumber].needItemName[i], archemyItems[archemyItemArrayNumber].needItemNumber[i]);
            }

            // 제작 시작
            archemyItemQueue.Enqueue(archemyItems[archemyItemArrayNumber]);
            
            image_CraftingItems[archemyItemQueue.Count].gameObject.SetActive(true);
            image_CraftingItems[archemyItemQueue.Count].sprite = archemyItems[archemyItemArrayNumber].itemImage;
        }
        else
        {
            PlaySE(sound_Beep);
        }
    }

    public void UpButton()
    {
        PlaySE(sound_ButtonClick);

        if (page != 1)
            page--;
        else
            page = 1 + archemyItems.Length / theNumberOfSlot; // 최대 페이지

        ClearSlot();
        PageSetting();
    }

    public void DownButton()
    {
        PlaySE(sound_ButtonClick);

        if (page < 1 + archemyItems.Length / theNumberOfSlot)
            page++;
        else
            page = 1;

        ClearSlot();
        PageSetting();
    }

    private void ClearSlot()
    {
        for (int i = 0; i < theNumberOfSlot; i++)
        {
            image_ArchemyItems[i].sprite = null;
            image_ArchemyItems[i].gameObject.SetActive(false);
            btn_ArchemyItems[i].gameObject.SetActive(false);
            text_ArchemyItems[i].text = "";
        }
    }

    private void PageSetting()
    {
        int pageArrayStartNumber = (page - 1) * theNumberOfSlot; //  4의 배수
        
        for (int i = pageArrayStartNumber; i < archemyItems.Length; i++)
        {
            if (i == page * theNumberOfSlot)
                break;

            image_ArchemyItems[i - pageArrayStartNumber].sprite = archemyItems[i].itemImage;
            image_ArchemyItems[i - pageArrayStartNumber].gameObject.SetActive(true);
            btn_ArchemyItems[i - pageArrayStartNumber].gameObject.SetActive(true);
            text_ArchemyItems[i - pageArrayStartNumber].text = archemyItems[i].itemName + "\n" + archemyItems[i].itemDescription;
        }
    }

    private void ProductionComplete()
    {
        isCrafting = false;
        image_CraftingItems[0].gameObject.SetActive(false);

        Instantiate(currentCraftingItem.go_ItemPrefab, tf_PotionAppearPos.position, Quaternion.identity);
    }

    public bool GetIsOpen()
    {
        return isOpen;
    }

    public void ShowToolTip(int _buttonNum)
    {
        int _archemyItemArrayNumber = _buttonNum + ((page - 1) * theNumberOfSlot); // 페이지 고려
        theToolTip.ShowTooltip(archemyItems[_archemyItemArrayNumber].needItemName, archemyItems[_archemyItemArrayNumber].needItemNumber);
    }

    public void HideToolTip()
    {
        theToolTip.HideToolTip();
    }

    private void PlaySE(AudioClip _clip)
    {
        theAudio.clip = _clip;
        theAudio.Play();
    }
}
