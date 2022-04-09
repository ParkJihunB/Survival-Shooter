using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    private Vector3 originPos;  // 기존 위치. 원래 위치 값으로 돌아가기 위해 보존.
    private Vector3 currentPos;  // 현재 위치. 계속 계산해나가며 업데이트 할 것.
    public static bool isActivated = true;

    [SerializeField]
    private Vector3 limitPos;  // 정조준이 아닐 때 무기가 최대 얼만큼까지 흔들릴 수 있는지.
    [SerializeField]
    private Vector3 fineSightLimitPos;  // 정조준 할 때 무기가 최대 얼만큼까지 흔들릴 수 있는지.
    [SerializeField]
    private Vector3 smoothSway;  // 총이 흔들릴 때 부드럽게 흔들리는 정도.

    [SerializeField]
    private GunController theGunController;  // 📜GunController.cs 의 isFineSightMode 를 받아 오기 위해. 정조준 상태 여부에 따라 흔들림 정도가 다르므로 알아야 함.

    void Start()
    {
        originPos = this.transform.localPosition;
        currentPos = originPos;
    }

    void Update()
    {
        if (!Inventory.invectoryActivated && isActivated)
            TrySway();
    }

    private void TrySway()
    {
        if (Input.GetAxisRaw("Mouse X") != 0 || Input.GetAxisRaw("Mouse Y") != 0)
            Swaying();
        else
            BackToOriginPos();
    }

    private void Swaying()
    {
        float _moveX = Input.GetAxisRaw("Mouse X");  // -1.0 혹은 0.0 혹은 1.0
        float _moveY = Input.GetAxisRaw("Mouse Y");  // -1.0 혹은 0.0 혹은 1.0

        if(!theGunController.GetFineSightMode())
        {
            currentPos.Set(Mathf.Clamp(Mathf.Lerp(currentPos.x, -_moveX, smoothSway.x), -limitPos.x, limitPos.x),
                        Mathf.Clamp(Mathf.Lerp(currentPos.y, -_moveY, smoothSway.y), -limitPos.y, limitPos.y),
                        originPos.z);
        }
        else
        {
            currentPos.Set(Mathf.Clamp(Mathf.Lerp(currentPos.x, -_moveX, smoothSway.x), -fineSightLimitPos.x, fineSightLimitPos.x),
                        Mathf.Clamp(Mathf.Lerp(currentPos.y, -_moveY, smoothSway.y), -fineSightLimitPos.y, fineSightLimitPos.y),
                        originPos.z);
        }

        transform.localPosition = currentPos;
    }

    private void BackToOriginPos()
    {
        currentPos = Vector3.Lerp(currentPos, originPos, smoothSway.x);
        transform.localPosition = currentPos;
    }
}