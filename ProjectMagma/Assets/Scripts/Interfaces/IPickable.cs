using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPickable
{
    void OnTriggerEnter(Collider other);

    void Pick();
}
