using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseWeapon : MonoBehaviour
{
    public string closeWeaponName;  // 근접 무기 이름

    // 근접 무기 유형
    public bool isHand;
    public bool isAxe;
    public bool isPickaxe;

    public float range; // 공격 범위. 팔을 뻗으면 어디까지 공격이 닿을지
    public int damage; // 공격력
    public float workSpeed; // 작업 속도
    public float attackDelay;  // 공격 딜레이. 마우스 클릭하는 순간 마다 공격할 순 없으므로.
    public float attackDelayA;  // 공격 활성화 시점. 공격 애니메이션 중에서 주먹이 다 뻗어졌을 때 부터 공격 데미지가 들어가야 한다.
    public float attackDelayB;  // 공격 비활성화 시점. 이제 다 때리고 주먹을 빼는 애니메이션이 시작되면 공격 데미지가 들어가면 안된다.

    public float workDelay;  // 작업 딜레이
    public float workDelayA;  // 작업 활성화 시점.
    public float workDelayB;  // 작업 비활성화 시점. 
    
    public Animator anim;  // 애니메이터 컴포넌트
}