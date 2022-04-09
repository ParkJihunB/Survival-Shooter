using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    [SerializeField]
    private int hp;  // 풀의 체력
    [SerializeField]
    private float destroyTime;  // 풀 이펙트 사라질 시간
    [SerializeField]
    private float explosionForce;  // 폭발 세기

    [SerializeField]
    private Item item_leaf;
    [SerializeField]
    private int leafCount;
    private Inventory theInventory;

    [SerializeField]
    private GameObject go_hit_effect_prefab;  // 풀 떄릴때 마다 발생시킬 이펙트 효과 프리팹

    private Rigidbody[] rigidbodys;  // 풀 파괴시 자식인 풀 조각들에게 붙어있는 Rigidbody들의 중력 활성화하기 위하여
    private BoxCollider[] boxColiders;  // 풀 파괴시 자식인 풀 조각들에게 붙어있는 Box Collider 들을 활성화하기 위하여

    [SerializeField]
    private string hit_sound;  // 풀 타격시 소리

    void Start()
    {
        rigidbodys = this.transform.GetComponentsInChildren<Rigidbody>();
        boxColiders = this.transform.GetComponentsInChildren<BoxCollider>();
        theInventory = FindObjectOfType<Inventory>(); 
    }

    public void Damage()
    {
        hp--;

        Hit();

        if(hp <= 0)
        {
            Destruction();
        }
    }

    private void Hit()
    {
        SoundManager.instance.PlaySE(hit_sound);

        var clone = Instantiate(go_hit_effect_prefab, transform.position + Vector3.up, Quaternion.identity);
        Destroy(clone, destroyTime);
    }

    private void Destruction()
    {
        theInventory.AcquireItem(item_leaf, leafCount);
        for (int i = 0; i < rigidbodys.Length; i++)
        {
            rigidbodys[i].useGravity = true;
            rigidbodys[i].AddExplosionForce(explosionForce, transform.position, 1f); // 제자리에서 중력 받아 그냥 떨어지는게 아니라 폭발 효과를 줌 (폭발 세기, 폭발 위치, 폭발 반경)
            boxColiders[i].enabled = true;
        }

        Destroy(this.gameObject, destroyTime);
    }
}
