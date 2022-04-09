using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrongAnimal : Animal
{

    [SerializeField]
    protected int attackDamage; // 공격 데미지
    [SerializeField]
    protected float attackDelay; // 공격 딜레이
    [SerializeField]
    protected float attackDistance; // 공격 가능한 사정 거리. 이 안에 플레이어가 들어오면 가까이 있다고 판단.
    [SerializeField]
    protected LayerMask targetMask; // 플레이어 레이어가 할당 될 것.

    [SerializeField]
    protected float chaseTime;  // 총 추격 시간
    protected float currentChaseTime;  
    [SerializeField]
    protected float chaseDelayTime; // 추격 딜레이

    public void Chase(Vector3 _targetPos)
    {
        destination = _targetPos;

        isChasing = true;

        isWalking = false;
        isRunning = true;
        nav.speed = runSpeed;

        anim.SetBool("Running", isRunning);

        if(!isDead)
          nav.SetDestination(destination);
    }

    public override void Damage(int _dmg, Vector3 _targetPos)
    {
        base.Damage(_dmg, _targetPos);
        if (!isDead)
            Chase(_targetPos);
    }

    protected IEnumerator ChaseTargetCoroutine()
    {
        currentChaseTime = 0;
        Chase(theFieldOfViewAngle.GetTargetPos());

        while (currentChaseTime < chaseTime)
        {
            Chase(theFieldOfViewAngle.GetTargetPos());
            // 플레이어와 충분히 가까이 있고 
            if (Vector3.Distance(transform.position, theFieldOfViewAngle.GetTargetPos()) <= attackDistance)
            {
                if (theFieldOfViewAngle.View())  // 눈 앞에 있을 경우
                {
                    Debug.Log("플레이어 공격 시도");
                    StartCoroutine(AttackCoroutine());
                }
            }
            yield return new WaitForSeconds(chaseDelayTime);
            currentChaseTime += chaseDelayTime;
        }

        isChasing = false;
        isRunning = false;
        anim.SetBool("Running", isRunning);
        nav.ResetPath();
    }

    protected IEnumerator AttackCoroutine()
    {
        isAttacking = true;

        // 공격은 제자리에서 이루어져야 함.
        nav.ResetPath();
        currentChaseTime = chaseTime;

        // 공격은 대상을 바라보고 하도록. (0.5초 대기한 후)
        yield return new WaitForSeconds(0.5f);
        transform.LookAt(theFieldOfViewAngle.GetTargetPos());

        // 공격 애니메이션이 어느정도 진행된 후 데미지를 입히도록 대기
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(0.5f);

        RaycastHit _hit;
        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out _hit, attackDistance, targetMask))
        {
            Debug.Log("플레이어가 적중!!");
            thePlayerStatus.DecreaseHP(attackDamage);
        }
        else
        {
            Debug.Log("플레이어 빗나감!!");
        }
        yield return new WaitForSeconds(attackDelay);

        isAttacking = false;
        StartCoroutine(ChaseTargetCoroutine());
    }

}