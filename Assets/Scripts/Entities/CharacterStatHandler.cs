using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterStatHandler : MonoBehaviour
{
    [SerializeField] CharacterStats baseStats;
    public CharacterStats CurrentStates { get; private set; }
    public List<CharacterStats> statsModifiers = new List<CharacterStats>();

    private void Awake()
    {
        UpdatedCharacterStats();
    }

    private void UpdatedCharacterStats()
    {
        AttackSO attackSO = null;
        if(baseStats.attackSO != null)
        {
            attackSO = Instantiate(baseStats.attackSO);
        }

        CurrentStates = new CharacterStats { attackSO = attackSO };
        // TODO
        CurrentStates.statsChangeType = baseStats.statsChangeType;
        CurrentStates.maxHealth = baseStats.maxHealth;
        CurrentStates.speed = baseStats.speed;
    }
}


