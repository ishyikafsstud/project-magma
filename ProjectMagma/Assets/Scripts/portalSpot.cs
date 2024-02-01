using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class portalSpot : MonoBehaviour, ILockable
{
    [Tooltip("The portal itself, with the VFX and portal passing functionality.")]
    [SerializeField] GameObject portal;
    [Tooltip("The collider of the \"go find the activator stone\" reminder.")]
    [SerializeField] BoxCollider reminderTriggerCollider;
    [SerializeField] Light spotLight;

    [Header("---- Settings ----")]
    [Tooltip("Whether the portal should be activated on start (false by default).")]
    [SerializeField] bool activateOnStart;

    bool isLocked;

    // Start is called before the first frame update
    void Start()
    {
        SetLock(!activateOnStart);

        if (!activateOnStart)
            gameManager.OnKeyPicked += Unlock; // Subscribe to call Unlock() on key pickup 
    }

    private void OnDisable()
    {
        gameManager.OnKeyPicked -= Unlock;
    }

    public void SetLock(bool lockValue)
    {
        isLocked = lockValue;
        if (isLocked)
            Lock();
        else
            Unlock();
    }

    public void Lock()
    {
        portal.SetActive(false);
        reminderTriggerCollider.enabled = true;
        spotLight.enabled = false;
    }

    public void Unlock()
    {
        portal.SetActive(true);
        reminderTriggerCollider.enabled = false;
        spotLight.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !gameManager.instance.IsKeyPicked)
        {
            StartCoroutine(gameManager.instance.ShowHint("Can't proceed: find the activator stone!", 10.0f));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.instance.HideHint();
        }
    }
}
