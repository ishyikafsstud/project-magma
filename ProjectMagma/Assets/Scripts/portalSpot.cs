using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using UnityEngine;

public class portalSpot : MonoBehaviour, ILockable
{
    [Tooltip("The portal itself, with the VFX and portal passing functionality.")]
    [SerializeField] GameObject portal;
    [Tooltip("The collider of the portal that triggers level completion.")]
    [SerializeField] Collider portalCollider;
    [Tooltip("The collider of the \"go find the activator stone\" reminder.")]
    [SerializeField] BoxCollider reminderTriggerCollider;
    [SerializeField] Light spotLight;

    [Header("---- Settings ----")]
    [Tooltip("Whether the portal should be activated on start (false by default).")]
    [SerializeField] bool activateOnStart;

    [SerializeField] AudioSource portalActivatedSFX;
    [SerializeField] AudioSource portalSFX;

    bool isLocked;

    // Start is called before the first frame update
    void Start()
    {
        SetLock(!activateOnStart);

        if (!activateOnStart)
            gameManager.OnKeyPicked += Unlock; // Subscribe to call Unlock() on key pickup 

        gameManager.AmbushRewardDropped += DisablePortalTrigger;
        gameManager.AmbushRewardPickedEvent += EnablePortalTrigger;
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
        spotLight.enabled = false;
    }

    public void Unlock()
    {
        portalActivatedSFX.Play();
        portalSFX.PlayDelayed(5.5f);
        portal.SetActive(true);
        spotLight.enabled = true;
    }

    /// <summary>
    /// Enable just the trigger area that triggers the level completion scenario.
    /// </summary>
    void EnablePortalTrigger()
    {
        if (portalCollider != null)
            portalCollider.enabled = true;
    }

    /// <summary>
    /// Disable just the trigger area that triggers the level completion scenario.
    /// </summary>
    void DisablePortalTrigger()
    {
        if (portalCollider != null)
            portalCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            if (!gameManager.instance.IsKeyPicked)
            {
                StartCoroutine(gameManager.instance.ShowHint("Can't proceed: find the activator stone!", 10.0f));
            }
            else if (gameManager.instance.IsKeyPicked && !gameManager.instance.IsAmbushRewardPicked)
            {
                StartCoroutine(gameManager.instance.ShowHint("Can't proceed: pick up the ambush reward!", 10.0f));
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
