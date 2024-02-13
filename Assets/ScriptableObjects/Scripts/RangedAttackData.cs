using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RangedAttackData", menuName = "TopDownController/Attacks/Ranged", order = 1)]
public class RangedAttackData : AttackSO
{
    [Header("Ranged Attack Data")]
    public string bulletNameTag;
    public float duration;
    public float spread;
    public int numberofProjectilesPerShot;  // 연사 기능 쓸때 수정필요
    public float multipleProjectilesAngel;  // 공격시 각도
    public Color projectileColor;
}
