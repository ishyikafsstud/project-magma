using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inventorySoundManager : soundManagerBase
{
    [Header("----- Audio SFX -----")]
    [SerializeField] AudioSource itemPickedSFX;
    [SerializeField] AudioSource weaponPickedSFX;
    [SerializeField] AudioSource weaponSwitchedSFX;
}
