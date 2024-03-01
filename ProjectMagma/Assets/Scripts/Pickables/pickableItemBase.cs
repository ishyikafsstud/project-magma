using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using UnityEngine.Audio;

public class pickableItemBase : MonoBehaviour, IPickable
{
    [Tooltip("Does it need to show a prompt to pick up? If no, the item will be picked on contact.")]
    [SerializeField] bool requiresPickupConfirmation = true;
    [Tooltip("If it requires confirmation, do you want to also show the prompt for pickup? " +
        "Keep it on unless you have a good reason not to hint user.")]
    [SerializeField] bool showPrompt = true;
    [Tooltip("Title of the item - keep it short.")]
    [SerializeField] string itemTitle;
    [Tooltip("A short description of the item that will be displayed beneath the item title.")]
    [SerializeField] string itemDescription;
    [SerializeField] AudioClip pickupSound;
    [Tooltip("Volume multiplier of the pickup sound. Keep at 1 for the default volume.")]
    [Range(0.1f, 5.0f)][SerializeField] float pickupSoundVolume = 1.0f;
    [SerializeField] AudioMixerGroup mixerGroup;

    bool canPickup = false;

    protected virtual void Start()
    {
    }

    /// <summary>
    /// Pickup the item.
    /// <para>This method is to be overriden most often.</para>
    /// </summary>
    public virtual void Pickup()
    {
        if (pickupSound != null)
            AudioSourceExtension.PlayClipAtPoint(pickupSound, transform.position, pickupSoundVolume, mixerGroup);
        Destroy(gameObject);
    }

    protected virtual void Update()
    {
        if (canPickup & Input.GetButtonDown("Interaction"))
        {
            Pickup();
            //Debug.Log("Picked " + itemTitle + " with the description '" + itemDescription + "'");
            gameManager.instance.StopShowingItemPrompt();
        }
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // If requires confirmation, show the prompt and wait for button press.
            if (requiresPickupConfirmation)
            {
                canPickup = true;
                if (showPrompt)
                {
                    gameManager.instance.ShowItemPrompt(itemTitle, itemDescription);
                }
            }
            // If confirmation is nonessential, just pickup.
            else
            {
                Pickup();
            }
        }
    }

    public virtual void OnTriggerExit(Collider other)
    {
        canPickup = false;
        if (showPrompt)
        {
            gameManager.instance.StopShowingItemPrompt();
        }
    }
}
