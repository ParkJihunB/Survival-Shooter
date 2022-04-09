using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusController : MonoBehaviour
{
    // 체력
    [SerializeField]
    private int hp;  // 최대 체력. 유니티 에디터 슬롯에서 지정할 것.
    private int currentHp;

    // 스태미나
    [SerializeField]
    private int sp;  // 최대 스태미나. 유니티 에디터 슬롯에서 지정할 것.
    private int currentSp;

    // 스태미나 증가량
    [SerializeField]
    private int spIncreaseSpeed;

    // 스태미나 재회복 딜레이 시간
    [SerializeField]
    private int spRechargeTime;  
    private int currentSpRechargeTime;

    // 스태미나 감소 여부
    private bool spUsed;

    // 방어력
    [SerializeField]
    private int dp;  // 최대 방어력. 유니티 에디터 슬롯에서 지정할 것.
    private int currentDp;

    // 배고픔
    [SerializeField]
    private int hungry;  // 최대 배고픔. 유니티 에디터 슬롯에서 지정할 것.
    private int currentHungry;

    // 배고픔이 줄어드는 속도
    [SerializeField]
    private int hungryDecreaseTime;
    private int currentHungryDecreaseTime;

    // 목마름
    [SerializeField]
    private int thirsty;  // 최대 목마름. 유니티 에디터 슬롯에서 지정할 것.
    private int currentThirsty;

    // 목마름이 줄어드는 속도
    [SerializeField]
    private int thirstyDecreaseTime;
    private int currentThirstyDecreaseTime;

    // 만족감
    [SerializeField]
    private int satisfy;  // 최대 만족감. 유니티 에디터 슬롯에서 지정할 것.
    private int currentSatisfy;

    // 필요한 이미지
    [SerializeField]
    private Image[] images_Gauge;

    // 각 상태를 대표하는 인덱스
    private const int HP = 0, DP = 1, SP = 2, HUNGRY = 3, THIRSTY = 4, SATISFY = 5;

    void Start()
    {
        currentHp = hp;
        currentDp = dp;
        currentSp = sp;
        currentHungry = hungry;
        currentThirsty = thirsty;
        currentSatisfy = satisfy;
    }

    void Update()
    {
        Hungry();
        Thirsty();
        SPRechargeTime();
        SPRecover();
        GaugeUpdate();
    }

    private void GaugeUpdate()
    {
        images_Gauge[HP].fillAmount = (float)currentHp / hp;
        images_Gauge[SP].fillAmount = (float)currentSp / sp;
        images_Gauge[DP].fillAmount = (float)currentHp / dp;
        images_Gauge[HUNGRY].fillAmount = (float)currentHungry / hungry;
        images_Gauge[THIRSTY].fillAmount = (float)currentThirsty / thirsty;
        images_Gauge[SATISFY].fillAmount = (float)currentSatisfy / satisfy;
    }

    private void Hungry()
    {
        if (currentHungry > 0)
        {
            if (currentHungryDecreaseTime <= hungryDecreaseTime)
                currentHungryDecreaseTime++;
            else
            {
                currentHungry--;
                currentHungryDecreaseTime = 0;
            }
        }
        else
            Debug.Log("배고픔 수치가 0 이 되었습니다.");
    }

    private void Thirsty()
    {
        if (currentThirsty > 0)
        {
            if (currentThirstyDecreaseTime <= thirstyDecreaseTime)
                currentThirstyDecreaseTime++;
            else
            {
                currentThirsty--;
                currentThirstyDecreaseTime = 0;
            }
        }
        else
            Debug.Log("목마름 수치가 0 이 되었습니다.");
    }

    public void IncreaseHP(int _count)
    {
        if (currentHp + _count < hp)
            currentHp += _count;
        else
            currentHp = hp;
    }

    public void DecreaseHP(int _count)
    {
        if(currentDp > 0)
        {
            DecreaseDP(_count);
            return;
        }
        currentHp -= _count;

        if (currentHp <= 0)
            Debug.Log("캐릭터의 체력이 0이 되었습니다!!");
    }

    public void IncreaseDP(int _count)
    {
        if (currentDp + _count < dp)
            currentDp += _count;
        else
            currentDp = dp;
    }

    public void DecreaseDP(int _count)
    {
        currentDp -= _count;

        if (currentDp <= 0)
            Debug.Log("캐릭터의 방어력이 0이 되었습니다!!");
    }

    public void IncreaseHungry(int _count)
    {
        if (currentHungry + _count < hungry)
            currentHungry += _count;
        else
            currentHungry = hungry;
    }

    public void DecreaseHungry(int _count)
    {
        if (currentHungry - _count < 0)
            currentHungry = 0;
        else
            currentHungry -= _count;
    }

    public void IncreaseThirsty(int _count)
    {
        if (currentThirsty + _count < thirsty)
            currentThirsty += _count;
        else
            currentThirsty = thirsty;
    }

    public void DecreaseThirsty(int _count)
    {
        if (currentThirsty - _count < 0)
            currentThirsty = 0;
        else
            currentThirsty -= _count;
    }

    public void DecreaseStamina(int _count)
    {
        spUsed = true;
        currentSpRechargeTime = 0;

        if (currentSp + _count > 0)
        {
            currentSp -= _count;
        }
        else
            currentSp = 0;
    }

    public void IncreaseStamina(int _count)
    {
        currentSpRechargeTime = 0;

        if (currentSp + _count <= sp)
        {
            currentSp += _count;
        }
        else
            currentSp = sp;
    }

    private void SPRechargeTime()
    {
        if (spUsed)
        {
            if (currentSpRechargeTime < spRechargeTime)
                currentSpRechargeTime++;
            else
                spUsed = false;
        }
    }

    private void SPRecover()
    {
        if (!spUsed && currentSp < sp)
        {
            currentSp += spIncreaseSpeed;
        }
    }

    public int GetCurrentSP()
    {
        return currentSp;
    }
}