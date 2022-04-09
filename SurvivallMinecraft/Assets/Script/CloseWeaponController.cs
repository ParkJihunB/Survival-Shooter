using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//추상 함수를 가지고 있는 추상 클래스이므로 오브젝트에 붙지 못한다
public abstract class CloseWeaponController : MonoBehaviour
{
    [SerializeField]
    protected CloseWeapon currentCloseWeapon; // 현재 장착된 근접 무기 
    [SerializeField]
    protected LayerMask layerMask;

    protected PlayerController thePlayerController;
    protected RaycastHit hitInfo;  // 현재 무기(Hand)에 닿은 것들의 정보.

    protected bool isAttack = false;  // 현재 공격 중인지 
    protected bool isSwing = false;  // 팔을 휘두르는 중인지. isSwing = True 일 때만 데미지를 적용할 것이다.

    protected void TryAttack()
    {
        if (!Inventory.invectoryActivated)
        {
            if (Input.GetButton("Fire1"))
            {
                if (!isAttack)
                {
                    if(CheckObject())
                    {
                        if (currentCloseWeapon.isAxe && hitInfo.transform.tag == "Tree")
                        {
                            StartCoroutine(thePlayerController.TreeLookCoroutine(hitInfo.transform.GetComponent<TreeComponent>().GetTreeCenterPosition()));
                            StartCoroutine(AttackCoroutine("Chop", currentCloseWeapon.workDelayA, currentCloseWeapon.workDelayB, currentCloseWeapon.workDelay));
                            return;
                        }
                    }
                    
                    StartCoroutine(AttackCoroutine("Attack", currentCloseWeapon.attackDelayA, currentCloseWeapon.attackDelayB, currentCloseWeapon.attackDelay));
                }
            }
        }

    }

    protected IEnumerator AttackCoroutine(string swingType, float _delayA, float _delayB, float _delay)
    {
        isAttack = true;
        currentCloseWeapon.anim.SetTrigger(swingType);

        yield return new WaitForSeconds(_delayA);
        isSwing = true;

        StartCoroutine(HitCoroutine());

        yield return new WaitForSeconds(_delayB);
        isSwing = false;

        yield return new WaitForSeconds(_delay - _delayA - _delayB);
        isAttack = false;
    }

    protected bool CheckObject()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, currentCloseWeapon.range, layerMask))
        {
            return true;
        }
        return false;
    }

    protected abstract IEnumerator HitCoroutine();

    public virtual void CloseWeaponChange(CloseWeapon _closeWeapon)
    {
        if (WeaponManager.currentWeapon != null)
            WeaponManager.currentWeapon.gameObject.SetActive(false);

        currentCloseWeapon = _closeWeapon;
        WeaponManager.currentWeapon = currentCloseWeapon.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentCloseWeapon.anim;

        currentCloseWeapon.transform.localPosition = Vector3.zero;
        currentCloseWeapon.gameObject.SetActive(true);
    }
}