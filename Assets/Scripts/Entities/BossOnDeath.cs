using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossOnDeath : MonoBehaviour
{
    private HealthSystem _healthSystem;
    private Rigidbody2D _rigidbody;
    public GameObject BossSlain;

    private void Start()
    {
        
        BossSlain.SetActive(false);
    }

    void OnDeath()
    {
        BossSlain.SetActive(true);
    }
}
