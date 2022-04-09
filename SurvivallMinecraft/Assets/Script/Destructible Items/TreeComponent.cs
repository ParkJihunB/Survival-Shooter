using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeComponent : MonoBehaviour
{
    [SerializeField]
    private GameObject[] go_treePieces;  // 가운데 조각을 제외한 깎일 5 개의 나무 테두리 조각들.
    [SerializeField]
    private GameObject go_treeCenter;  // 가운데 조각. 이 조각이 마지막으로 파괴되면 나무가 쓰러져야 한다.

    [SerializeField]
    private GameObject go_Log_Prefabs;  // 통나무. 나무가 쓰러진 이후 생성할.
         
    [SerializeField]
    private float force;  // 나무가 땅에 쓰러지도록 밀어줄 힘의 세기(랜덤으로 정할 것) 
    [SerializeField]
    private GameObject go_ChildTree;  // 쓰러질 나무 윗부분. 쓰러지고 난 다음에 지연 시간 후 파괴 되야 해서 필요함.

    [SerializeField]
    private CapsuleCollider parentCol;  // 전체적인 나무에 붙어있는 캡슐 콜라이더. 나무가 쓰러지면 이걸 비활성화 해주어야 함.
    [SerializeField]
    private CapsuleCollider childCol;  // 쓰러질 나무인 나무 윗부분에 붙어있는 캡슐 콜라이더. 나무가 쓰러지면 이걸 활성화 해주어야 함.
    [SerializeField]
    private Rigidbody childRigid; // 쓰러질 나무인 나무 윗부분에 붙어있는 Rigidbody를 통해 나무가 쓰러지면 중력을 활성화 해주어야 함.

    [SerializeField]
    private GameObject go_hit_effect_prefab;  // 나무 도끼질 할 때마다 이펙트 효과(나무 파편)
    [SerializeField]
    private float debrisDestroyTime;  // 파편 제거 시간. 나무 도끼질 이펙트(나무 파편) 파괴될 시간

    [SerializeField]
    private float destroyTime;  // 나무 제거 시간. 나무 윗 부분이 땅에 쓰러지고 나서 파괴될 시간.

    [SerializeField]
    private string chop_sound;  // 나무 도끼질시 재생시킬 사운드 이름 
    [SerializeField]
    private string falldown_sound;  // 나무 쓰러질 때 재생시킬 사운드 이름 
    [SerializeField]
    private string logChange_sound;  // 나무 쓰러져서 통나무로 바뀔 때 재생시킬 사운드 이름


    // 도끼질 한 위치를 알아야 그 곳에서 이펙트 효과 재생함
    // 플레이어가 도끼질 한 Y 방향 회전값을 알아야 어느 조각이 맞았는지를 알 수 있음.
    public void Chop(Vector3 _pos, float angleY)   
    {
        Hit(_pos);

        AngleCalc(angleY);

        if (CheckTreePieces())
            return;

        FallDownTree();
    }

    private void Hit(Vector3 _pos)
    {
        SoundManager.instance.PlaySE(chop_sound);

        GameObject clone = Instantiate(go_hit_effect_prefab, _pos, Quaternion.Euler(Vector3.zero));
        Destroy(clone, debrisDestroyTime);
    }

    private void AngleCalc(float _angleY)
    {
        if (_angleY >= 0 && _angleY <= 70)
            DestroyPiece(2);
        else if (_angleY >= 70 && _angleY <= 140)
            DestroyPiece(3);
        else if (_angleY >= 140 && _angleY <= 210)
            DestroyPiece(4);
        else if (_angleY >= 210 && _angleY <= 280)
            DestroyPiece(0);
        else if (_angleY >= 280 && _angleY <= 360)
            DestroyPiece(1);
    }

    private void DestroyPiece(int _num)
    {
        if(go_treePieces[_num].gameObject != null)
        {
            GameObject clone = Instantiate(go_hit_effect_prefab, go_treePieces[_num].transform.position, Quaternion.Euler(Vector3.zero));
            Destroy(clone, debrisDestroyTime);
            Destroy(go_treePieces[_num].gameObject);
        }
    }

    private bool CheckTreePieces()
    {
        for (int i = 0; i < go_treePieces.Length; i++)
        {
            if(go_treePieces[i].gameObject != null)
            {
                return true;
            }
        }
        return false;
    }

    private void FallDownTree()
    {
        SoundManager.instance.PlaySE(falldown_sound);

        Destroy(go_treeCenter);
        
        parentCol.enabled = false;
        childCol.enabled = true;
        childRigid.useGravity = true;

        childRigid.AddForce(Random.Range(-force, force), 0f, Random.Range(-force, force));

        StartCoroutine(LogCoroutine());
    }

    IEnumerator LogCoroutine()
    {
        yield return new WaitForSeconds(destroyTime);

        SoundManager.instance.PlaySE(logChange_sound);

        Instantiate(go_Log_Prefabs, go_ChildTree.transform.position + (go_ChildTree.transform.up * 3f), Quaternion.LookRotation(go_ChildTree.transform.up));
        Instantiate(go_Log_Prefabs, go_ChildTree.transform.position + (go_ChildTree.transform.up * 6f), Quaternion.LookRotation(go_ChildTree.transform.up));
        Instantiate(go_Log_Prefabs, go_ChildTree.transform.position + (go_ChildTree.transform.up * 9f), Quaternion.LookRotation(go_ChildTree.transform.up));

        Destroy(go_ChildTree.gameObject);
    }

    public Vector3 GetTreeCenterPosition()
    {
        return go_treeCenter.transform.position;
    }
}