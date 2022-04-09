using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeakAnimal : Animal
{

    [SerializeField]
    protected Item item_Prefab;  // 해당 동물에게서 얻을 아이템
    [SerializeField]
    public int itemNumber;  // 아이템 획득 개수

    public void Run(Vector3 _targetPos)
    {
        destination = new Vector3(transform.position.x - _targetPos.x, 0f, transform.position.z - _targetPos.z).normalized;

        currentTime = runTime;
        isWalking = false;
        isRunning = true;
        nav.speed = runSpeed;

        anim.SetBool("Running", isRunning);
    }

    public override void Damage(int _dmg, Vector3 _targetPos)
    {
        base.Damage(_dmg, _targetPos);
        if (!isDead)
            Run(_targetPos);
    }

    public Item GetItem()
    {
        this.gameObject.tag = "Untagged";
        Destroy(this.gameObject, 3f);
        return item_Prefab;
    }
}