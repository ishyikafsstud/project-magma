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

    private void Start()
    {
        cooldownCircle.fillAmount = 1.0f;
    }

    public void Reload(float rate)
    {
        if (!isCooldown)
        {
            shootRate = rate;
            cooldownTimer = 0;
            isCooldown = true;
            //GetComponent<Image>().enabled = true;
        }
    }
    void Update()
    {
        if (isCooldown)
        {
            cooldownTimer += Time.deltaTime;

            cooldownCircle.fillAmount = cooldownTimer / shootRate;

            if (cooldownTimer >= shootRate)
            {
                isCooldown = false;
                //GetComponent<Image>().enabled = false;
            }
        }
    }

}
