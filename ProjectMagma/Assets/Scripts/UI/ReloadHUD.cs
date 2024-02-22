using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReloadHUD : MonoBehaviour
{
    [SerializeField] float shootRate = 1.0f;
    [SerializeField] Image cooldownCircle;
    float cooldownTimer = 0.0f;
    bool isCooldown = false;

    public void Reload(float rate)
    {
        if (!isCooldown)
        {
            shootRate = rate;
            cooldownTimer = shootRate;
            isCooldown = true;
        }
    }
    void Update()
    {
        if (isCooldown)
        {
            cooldownTimer -= Time.deltaTime;

            cooldownCircle.fillAmount = cooldownTimer / shootRate;

            if (cooldownTimer <= 0.0f)
            {
                isCooldown = false;
                cooldownCircle.fillAmount = 0.0f;
            }
        }
    }

}
