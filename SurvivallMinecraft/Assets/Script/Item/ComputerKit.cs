using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Kit  // 하나의 아이템이 담고 있는 정보
{
    public string kitName;
    public string kitDescription;
    public string[] needItemName;
    public int[] needItemNumber;
    public GameObject go_Kit_Prefab;
}

public class ComputerKit : MonoBehaviour
{
    [SerializeField]
    private Kit[] kits;  // 컴퓨터로 생성할 수 있는 아이템 배열
    [SerializeField]
    private ComputerToolTip theToolTip;
    [SerializeField] private GameObject go_BaseUI;  // 모니터 화면 배경 이미지UI

    [SerializeField]
    private Transform tf_ItemAppear; // 아이템 나오는 구멍의 트랜스폼 정보 (Item Appear Pos)

    private bool isCraft = false;  // 지금 제작 되는 중인지. false 일 때만 제작 가능하게 할 것.
    public bool isPowerOn = false; // 전원 켜진 상태인지. 중복 켜기 방지.

    // 필요한 컴포넌트
    private Inventory theInventory; // 📜Inventory.cs

    private AudioSource theAudio;
    [SerializeField] private AudioClip sound_ButtonClick; // 버튼 클릭 사운드
    [SerializeField] private AudioClip sound_Beep; // 아이템 부족하여 키트 생성이 불가능할 때 사운드
    [SerializeField] private AudioClip sound_Activated; // 아이템 생성 소리
    [SerializeField] private AudioClip sound_Output;  // 아이템 나오는 소리

    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false; 

        theInventory = FindObjectOfType<Inventory>();
        theAudio = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isPowerOn)
            if (Input.GetKeyDown(KeyCode.Escape))
                PowerOff();
    }

    private void PlaySE(AudioClip _clip) // 효과음 재생
    {
        theAudio.clip = _clip;
        theAudio.Play();
    }

    public void ClickButton(int _slotNumber) // 슬롯 버튼 클릭시 호출
    {
        PlaySE(sound_ButtonClick);  // 일단 버튼 클릭음 재생
        if(!isCraft)  // 현재 제작 중이 아니라면 제작 가능
        {
            if (!CheckIngredient(_slotNumber)) // 재료 있는지 체크
                return;

            isCraft = true; // 중복 실행 방지. 제작 중입니다.
            UseIngredient(_slotNumber); // 재료 사용(보유 아이템에서 제작에 쓰인 갯수만큼 차감)
            StartCoroutine(CraftCoroutine(_slotNumber)); // 키트 생성
        }
    }

    private bool CheckIngredient(int _slotNumber)
    {
        for (int i = 0; i < kits[_slotNumber].needItemName.Length; i++)
        {
            if (theInventory.GetItemCount(kits[_slotNumber].needItemName[i]) < kits[_slotNumber].needItemNumber[i])
            {
                PlaySE(sound_Beep);  // 아이템 제작 재료 모잘라서 생성할 수 없는 경우 경고음 재생
                return false;
            }
        }
        return true;
    }

    private void UseIngredient(int _slotNumber)
    {
        for (int i = 0; i < kits[_slotNumber].needItemName.Length; i++)
            theInventory.SetItemCount(kits[_slotNumber].needItemName[i], kits[_slotNumber].needItemNumber[i]);
    }

    IEnumerator CraftCoroutine(int _slotNumber) // 버튼 누르면 3 초 대기 후 아이템 생성
    {
        PlaySE(sound_Activated); // 아이템 생성 소리 재생
        yield return new WaitForSeconds(3f);
        PlaySE(sound_Output); // 아이템 빠져나오는 소리 재생 

        Instantiate(kits[_slotNumber].go_Kit_Prefab, tf_ItemAppear.position, Quaternion.identity); // 아이템 생성
        isCraft = false;  // 제작이 끝났으니 다시 false로
    }

    public void ShowToolTip(int _buttonNum)
    {
        theToolTip.ShowToolTip(kits[_buttonNum].kitName, kits[_buttonNum].kitDescription, kits[_buttonNum].needItemName, kits[_buttonNum].needItemNumber);
    }

    public void HideToolTip()
    {
        theToolTip.HideToolTip();
    }

    public void PowerOn() // 📜ActionController 에서 호출
    {
        GameManager.isOnComputer = true;

        isPowerOn = true;
        go_BaseUI.SetActive(true);
    }

    private void PowerOff()
    {
        GameManager.isOnComputer = false;

        theToolTip.HideToolTip();
        isPowerOn = false;
        go_BaseUI.SetActive(false);
    }
}
