using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotScript : MonoBehaviour
{
    [SerializeField] GameObject emptySlotText;
    [SerializeField] GameObject icon;
    [SerializeField] GameObject selectionAura;

    private void Start()
    {
        emptySlotText.SetActive(true);
        icon.SetActive(false);
        selectionAura.SetActive(false);
    }

    /// <summary>
    /// Fill the slot with an icon and disable the empty slot text.
    /// </summary>
    /// <param name="iconTexture">Texture of the weapon icon.</param>
    public void FillSlot(Texture2D iconTexture)
    {
        emptySlotText.SetActive(false);
        icon.SetActive(true);

        if (iconTexture != null)
            icon.GetComponent<Image>().sprite = Sprite.Create(iconTexture,
                new Rect(0.0f, 0.0f, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// Enable/disable the selection look.
    /// </summary>
    /// <param name="isSelected"></param>
    public void Select(bool isSelected)
    {
        selectionAura.SetActive(isSelected);
    }
}
