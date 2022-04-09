using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//아이템들이 가지는 기본적인 데이터 관리
//에셋으로 만들어둘 수 있다.
[CreateAssetMenu(fileName = "New Item", menuName = "New Item/item")]
public class Item : ScriptableObject  // 게임 오브젝트에 붙일 필요 X 
{
    public enum ItemType  // 아이템 유형
    {
        Equipment,
        Used,
        Ingredient,
        Kit,
        ETC,
    }

    public string itemName; // 아이템의 이름
    public ItemType itemType; // 아이템 유형
    [TextArea]  // 여러 줄 가능해짐
    public string itemDesc; // 아이템의 설명
    public Sprite itemImage; // 아이템의 이미지(인벤 토리 안에서 띄울)
    public GameObject itemPrefab;  // 아이템의 프리팹 (아이템 생성시 프리팹으로 찍어냄)
    public GameObject kitPrefab;  // 키트 프리팹
    public GameObject kitPreviewPrefab; // 키트 프리뷰 프리팹
    public string weaponType;  // 무기 유형
}