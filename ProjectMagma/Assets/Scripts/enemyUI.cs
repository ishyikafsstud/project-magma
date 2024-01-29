using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class enemyUI : MonoBehaviour
{
    [SerializeField] Image healthBar;
    [SerializeField] Image healthBG;

    private enemyAI parentEnemy;
    private bool hasTakenDmg;
    
    // Start is called before the first frame update
    void Start()
    {
        // Get the parent enemy component
        parentEnemy = transform.parent.GetComponent<enemyAI>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateUI();
        // UI will always face player
        if (gameManager.instance.player != null)
        {
            transform.LookAt(gameManager.instance.player.transform.position);
        }
    }

    public void UpdateUI()
    {
        if (parentEnemy != null)
        {
            float health = parentEnemy.GetHealth();
            float origHealth = parentEnemy.GetOriginalHealth();
            
            // Ensures health is not zero to prevent division by zero
            origHealth = Mathf.Max(origHealth, 1f);

            float currHealth = health / origHealth;

            if (healthBar != null)
            {
                healthBar.fillAmount = currHealth;
            }
            // Enables the health bar only when the enemy takes damage for the first time
            if (!hasTakenDmg && health < origHealth)
            {
                hasTakenDmg = true;
                EnableHealthBar(true);
            }
            // function to enable/disable the health bar and BG
            void EnableHealthBar(bool isEnabled)
            {
                if (healthBG != null)
                {
                    healthBG.gameObject.SetActive(isEnabled);
                }

                if (healthBar != null)
                {
                    healthBar.gameObject.SetActive(isEnabled);
                }
            }
        }
    }
    
}
