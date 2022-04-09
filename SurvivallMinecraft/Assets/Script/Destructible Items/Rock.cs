using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    [SerializeField]
    private int hp; // 바위의 체력. 0 이 되면 파괴됨
    [SerializeField]
    private int destroyTime; // 파괴된 바위의 파편들의 생명 (이 시간이 지나면 Destroy)
    [SerializeField]
    private int count;  // 바위 파괴시 생성할 아이템 갯수

    [SerializeField]
    private SphereCollider col; // 구체 콜라이더. 바위 파괴시키면 비활성화시킬것.
    [SerializeField]
    private GameObject go_rock;  //일반 바위 오브젝트. 평소에 활성화, 바위 깨지면 비활성화
    [SerializeField]
    private GameObject go_debris;  //깨진 바위 오브젝트. 평소에 비활성화, 바위 깨지면 활성화
    [SerializeField]
    private GameObject go_effect_prefabs;  // 채굴 이펙트 효과로 사용할 깨진 바위 오브젝트.
    [SerializeField]
    private GameObject go_rock_item_prefab; // 바위 파괴시 생성할 아이템 프리팹


    // 필요한 사운드 이름
    [SerializeField]
    private string strike_Sound;
    [SerializeField]
    private string destroy_Sound;

    //채굴: 한번 실행때 마다 hp 깎기
    public void Mining()
    {
        SoundManager.instance.PlaySE(strike_Sound);

        GameObject clone = Instantiate(go_effect_prefabs, col.bounds.center, Quaternion.identity);
        Destroy(clone, destroyTime);

        hp--;
        if (hp <= 0)
            Destruction();
    }

    //바위 파괴
    private void Destruction()
    {
        // 바위가 파괴될 때 effect_sound_2 오디오 클립 재생
        SoundManager.instance.PlaySE(destroy_Sound);

        col.enabled = false;

        Destroy(go_rock);

        go_debris.SetActive(true); //go_debris는 콜라이더가 있어서 활성화되자마자 부딪혀 날아간다
        Destroy(go_debris, destroyTime);//destroyTime만큼의 시간을 가진 후 go_debris 오브젝트도 파괴
        for (int i = 0; i < count; i++)
        {
            Instantiate(go_rock_item_prefab, go_rock.transform.position, Quaternion.identity);
        }
    }
}
