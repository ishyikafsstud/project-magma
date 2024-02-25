using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class enemyUI : MonoBehaviour
{
    [SerializeField] Image healthBG;
    [SerializeField] Image healthBar;
    [SerializeField] Image exclamationTop;
    [SerializeField] Image exclamationBottom;
    [SerializeField] Animator animator;

    private enemyAI parentEnemy;

    private bool hasTakenDmg;

    // Start is called before the first frame update
    void Start()
    {
        // Get the parent enemy component
        parentEnemy = transform.parent.gameObject.GetComponent<enemyAI>();

        parentEnemy.HealthChanged += ParentEnemy_HealthChanged;
        parentEnemy.AlertedEvent += ParentEnemy_AlertedEvent;

        EnableHealthBar(false);
    }

    public void ParentEnemy_HealthChanged(float health, float maxHealth)
    {
        // Enable the health bar only when the enemy takes damage for the first time
        if (!healthBG.IsActive() && health < maxHealth)
            EnableHealthBar(true);

        float healthbarFillAmount = health / maxHealth;

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

    void ParentEnemy_AlertedEvent(GameObject enemy)
    {
        if (!exclamationBottom.IsActive())
            EnableExclamationMark(true);

        if (animator != null)
        {
            animator.SetTrigger("AlertTrigger");
        }

    }
    
    void EnableExclamationMark(bool isEnabled)
    {
        if (exclamationTop != null)
            exclamationTop.gameObject.SetActive(isEnabled);
        
        if (exclamationBottom != null)
            exclamationBottom.gameObject.SetActive(isEnabled);
    }
}
