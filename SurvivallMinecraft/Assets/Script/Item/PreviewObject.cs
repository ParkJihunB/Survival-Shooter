using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewObject : MonoBehaviour
{
    public Building.Type needType;
    private bool needTypeFlag = true;
    private List<Collider> colliderList = new List<Collider>(); // 충돌한 오브젝트들 저장할 리스트

    [SerializeField]
    private int layerGround; // 지형 레이어 (무시하게 할 것)
    private const int IGNORE_RAYCAST_LAYER = 2;  // ignore_raycast (무시하게 할 것)
    [SerializeField]
    private Material green;
    [SerializeField]
    private Material red;


    void Update()
    {
        ChangeColor();
    }

    private void ChangeColor()
    {
        if (needType == Building.Type.Normal)
        {
            if (colliderList.Count > 0)
            {
                SetColor(red);
            }
            else
            {
                SetColor(green);
            }
        }
        else
        {
            if (colliderList.Count > 0 || !needTypeFlag)
            {
                SetColor(red);
            }
            else
            {
                SetColor(green);
            }
        }
    }
    private void SetColor(Material mat)
    {
        foreach(Transform tf_Child in this.transform)
        {
            Material [] newMaterials = new Material[tf_Child.GetComponent<Renderer>().materials.Length];

            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = mat;
            }

            tf_Child.GetComponent<Renderer>().materials = newMaterials;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Structure")
        {
            if (other.GetComponent<Building>().type == needType)
            {
                needTypeFlag = true;
            }
                
            else
            {
                colliderList.Add(other);
            }
                
        }
        else
        {
            if (other.gameObject.layer != layerGround && other.gameObject.layer != IGNORE_RAYCAST_LAYER)
            {
                colliderList.Add(other);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Structure")
        {
            if (other.GetComponent<Building>().type == needType)
            {
                needTypeFlag = false;
            }
                
            else
                colliderList.Remove(other);
        }
        else
        {
            if (other.gameObject.layer != layerGround && other.gameObject.layer != IGNORE_RAYCAST_LAYER)
            {
                colliderList.Remove(other);
            }
                
        }
    }

    public bool isBuildable()
    {
        if (needType == Building.Type.Normal)
            return colliderList.Count == 0;
        else
            return colliderList.Count == 0 && needTypeFlag;
    }
}