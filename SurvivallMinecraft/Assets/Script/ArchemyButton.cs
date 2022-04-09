using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ArchemyButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ArchemyTable theArchemy;
    [SerializeField] private int buttonNum;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        theArchemy.ShowToolTip(buttonNum);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        theArchemy.HideToolTip();
    }
}