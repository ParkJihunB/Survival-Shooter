using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    [SerializeField] private string fireName;  // 불의 이름 (난로, 모닥불, 화롯불)
    [SerializeField] private int damage; // 불의 데미지

    [SerializeField] private float damageTime; // 데미지가 들어갈 딜레이 (매 프레임마다가 아닌 일정 시간마다 데미지를 주기 위하여)
    private float currentDamageTime;

    [SerializeField] private float durationTime;  // 불의 지속 시간
    private float currentDurationTime;

    [SerializeField]
    private ParticleSystem ps_Flame;  // 불 파티클 시스템. 꺼주기 위하여

    // 상태 변수
    private bool isFire = true; public bool GetIsFire(){return isFire;}
    // 필요한 컴포넌트
    private StatusController thePlayerStatus;

    void Start()
    {
        currentDurationTime = durationTime;
        thePlayerStatus = FindObjectOfType<StatusController>();
    }

    void Update()
    {
        if (isFire)
        {
            ElapseTime();
        }
    }

    private void ElapseTime()
    {
        currentDurationTime -= Time.deltaTime;

        if (currentDurationTime <= 0)
            Off();
 

        if (currentDamageTime > 0)
            currentDamageTime -= Time.deltaTime;  // 1초에 1씩
    }

    private void Off()
    {
        ps_Flame.Stop();
        isFire = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if(isFire && other.transform.tag == "Player")
        {
            if (currentDamageTime <= 0)
            {
                other.GetComponent<Burn>().StartBurning();
                thePlayerStatus.DecreaseHP(damage);
                currentDamageTime = damageTime;
            }
        }
    }
}