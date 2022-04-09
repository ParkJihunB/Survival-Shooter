using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTrap : MonoBehaviour
{
    private Rigidbody[] rigid; // Twig, Board 의 Rigidbody 가지고 올 것
    [SerializeField] private GameObject go_Meat;  // 트랩에 걸리면 고기가 사라지게
    [SerializeField] private int damage; // 플레이어에게 입힐 데미지

    private bool isActivated = false; // 트랩이 발동되면 true

    private AudioSource theAudio;
    [SerializeField] private AudioClip sound_Activate; // 트랩 발동 사운드

    void Start()
    {
        rigid = GetComponentsInChildren<Rigidbody>();
        theAudio = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActivated)
        {
            if (other.transform.tag != "Untagged")
            {
                isActivated = true;
                theAudio.clip = sound_Activate;
                theAudio.Play();

                Destroy(go_Meat);

                for (int i = 0; i < rigid.Length; i++)
                {
                    rigid[i].useGravity = true;
                    rigid[i].isKinematic = false;
                }

                if (other.transform.name == "Player")
                    other.transform.GetComponent<StatusController>().DecreaseHP(damage);
            }
        }
    }
}