using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class enemyUI : MonoBehaviour
{
    [SerializeField] Image healthBG;
    [SerializeField] Image healthBar;

    private enemyAI parentEnemy;

    private bool hasTakenDmg;

    // Start is called before the first frame update
    void Start()
    {
        // Get the parent enemy component
        parentEnemy = transform.parent.gameObject.GetComponent<enemyAI>();

        EnableHealthBar(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateHealthbar(int health, int origHealth)
    {
        // Enable the health bar only when the enemy takes damage for the first time
        if (!healthBG.IsActive() && health < origHealth)
            EnableHealthBar(true);

        float healthbarFillAmount = (float)health / origHealth;

        if (healthBar != null)
        {
            healthBar.fillAmount = healthbarFillAmount;
        }
    }

    // function to enable/disable the health bar and BG
    void EnableHealthBar(bool isEnabled)
    {
        if (healthBG != null)
            healthBG.gameObject.SetActive(isEnabled);

        if (healthBar != null)
            healthBar.gameObject.SetActive(isEnabled);
    }
}
