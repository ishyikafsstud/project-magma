using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class contiuousDamageOnContact : MonoBehaviour
{
    [SerializeField] int damage = 1;
    [Tooltip("Time between dealing damage to the same entity that stayed within the AOE.")]
    [SerializeField] float cooldown = 1.0f;

    List<GameObject> damagedObjects = new List<GameObject>();

    IEnumerator DealContinuousDamage(GameObject entity)
    {
        if (entity != null)
            entity.GetComponent<IDamage>().takeDamage(damage);

        yield return new WaitForSeconds(cooldown);

        // If the entity is still within the AOE
        if (damagedObjects.Contains(entity))
            // and is valid, continue damaging
            if (entity != null)
                StartCoroutine(DealContinuousDamage(entity));
            // or if does no longer exist, remove from the damaged objects list
            else
                damagedObjects.Remove(entity);
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamage dmg = other.gameObject.GetComponent<IDamage>();

        // If the body can be damaged and is not already affected by this AOE
        if (dmg != null && !damagedObjects.Contains(other.gameObject))
        {
            damagedObjects.Add(other.gameObject);
            StartCoroutine(DealContinuousDamage(other.gameObject));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (damagedObjects.Contains(other.gameObject))
        {
            damagedObjects.Remove(other.gameObject);
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        damagedObjects.Clear();
    }
}
