using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    //크로스 헤어의 상태에 따른 총의 정확도. 작으면 작을 수록 좋다. 총알이 뻗치는 정도!
    private float gunAccuracy; 
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private GameObject go_CrosshairHUD;  // 크로스 헤어 활성화/비활성화를 위한 부모 객체

    [SerializeField]
    private GunController theGunController;

    public void WalkingAnimation(bool _flag)
    {
        WeaponManager.currentWeaponAnim.SetBool("Walk", _flag);  // 걷기시 무기 애니메이션
        animator.SetBool("Walking", _flag); // 걷기시 크로스헤어 애니메이션
    }

    public void RunningAnimation(bool _flag)
    {
        WeaponManager.currentWeaponAnim.SetBool("Run", _flag); // 뛰기시 무기 애니메이션
        animator.SetBool("Running", _flag); // 뛰기시 크로스헤어 애니메이션
    }

    public void JumpingAnimation(bool _flag)
    {
        animator.SetBool("Running", _flag); // 점프시 크로스헤어 애니메이션
    }

    public void CrouchingAnimation(bool _flag)
    {
        animator.SetBool("Crouching", _flag);
    }

    public void FineSightAnimation(bool _flag)
    {
        animator.SetBool("FineSight", _flag);
    }

    public void FireAnimaion()
    {
        if(animator.GetBool("Walking"))
        {
            animator.SetTrigger("Walk_Fire");
        }
        else if(animator.GetBool("Crouching"))
        {
            animator.SetTrigger("Crouch_Fire");
        }
        else
        {
            animator.SetTrigger("Idle_Fire");
        }
    }

    public float GetAccuracy()
    {
        if (animator.GetBool("Walking"))
        {
            gunAccuracy = 0.06f;
        }
        else if (animator.GetBool("Crouching"))
        {
            gunAccuracy = 0.015f;
        }
        else if (theGunController.GetFineSightMode())
        {
            gunAccuracy = 0.001f;
        }
        else
        {
            gunAccuracy = 0.035f;
        }

        return gunAccuracy;
    }
}