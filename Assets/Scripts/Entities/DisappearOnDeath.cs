using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearOnDeath : MonoBehaviour
{
    private HealthSystem _healthSystem;
    private Rigidbody2D _rigidbody;
    public GameObject BossSlain;

	private void Awake()
	{
        BossSlain = GameObject.Find("BossSlain");
	}

	private void Start()
    {
        _healthSystem = GetComponent<HealthSystem>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _healthSystem.OnDeath += OnDeath;
        BossSlain.SetActive(false);
    }

    void OnDeath()
    {
        _rigidbody.velocity = Vector3.zero;

        foreach (SpriteRenderer renderer in transform.GetComponentsInChildren<SpriteRenderer>())
        {
            Color color = renderer.color;
            color.a = 0.3f;
            renderer.color = color;
        }

        foreach (Behaviour component in transform.GetComponentsInChildren<Behaviour>())
        {
            component.enabled = false;
        }

        
		if (gameObject.CompareTag("Boss"))
		{
            Destroy(gameObject, 2f);
            BossSlain.SetActive(true);
        }
		else
		{
            Destroy(gameObject, 2f);
        }
        
    }
}
