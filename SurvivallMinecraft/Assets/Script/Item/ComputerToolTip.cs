using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComputerToolTip : MonoBehaviour
{
    [SerializeField] private GameObject go_BaseUI; // 툴팁 UI
    
    // 툴팁에 필요한 3 개의 텍스트
    [SerializeField] private Text kitName;
    [SerializeField] private Text kitDescription;
    [SerializeField] private Text kitNeedItem;

    public void ShowToolTip(string _kitName, string _kitDescription, string[] _needItem, int [] _needItemNumber)
    {
        go_BaseUI.SetActive(true);
        kitName.text = _kitName;
        kitDescription.text = _kitDescription;

        for (int i = 0; i < _needItem.Length; i++)
        {
            kitNeedItem.text += _needItem[i];
            kitNeedItem.text += " x " + _needItemNumber[i].ToString() + "\n";
        }
    }

    public void HideToolTip()
    {
        go_BaseUI.SetActive(false);
        kitName.text = "";
        kitDescription.text = "";
        kitNeedItem.text = "";
    }
}
