using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class helperClass : MonoBehaviour
{
    // TODO: this should be a serialized dictionary. Unity does not have built-in serialized
    // dictionaries, so we would need to write our own.
    [Tooltip("Make sure the position of weapon scriptable objects in the list corresponds " +
        "with the int value of their wandType. (e.g. Electric has the value of 0 so it must be the first in the list).\n" +
        "TL;DR - UNLESS YOU ARE IVAN, DON'T MESS WITH THIS LIST.")]
    [SerializeField] public List<weaponStats> weaponList;
}
