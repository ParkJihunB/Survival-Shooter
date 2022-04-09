using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ComputerButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ComputerKit theComputer;
    [SerializeField] private int buttonNum;

    public void OnPointerEnter(PointerEventData eventData) // 마우스가 슬롯 안에 들어갔을 때
    {
        theComputer.ShowToolTip(buttonNum);
    }

    public void OnPointerExit(PointerEventData eventData) // 마우스가 슬롯에서 빠져 나갔을 때
    {
        theComputer.HideToolTip();
    }
}