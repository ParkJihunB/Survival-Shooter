using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public Type type;

    public enum Type  // 건축물 타입 지정
    {
        Normal, // 건축물이 아닌 것들
        Wall,   // 벽
        Foundation, // 토대
        Pillar  // 기둥
    }
}