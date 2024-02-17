using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundManager : MonoBehaviour
{
    [Header("----- Player Audio SFX -----")]
    [SerializeField] public AudioSource hitSFX;
    [SerializeField] public AudioSource jumpSFX;
    [SerializeField] public AudioSource deathSFX;

    [Header("----- Enemy Audio SFX -----")]
    [SerializeField] public AudioSource spawnerSFX;
    [SerializeField] public AudioSource skeletonSwordSFX;
    [SerializeField] public AudioSource spiderlingSFX;
    [SerializeField] public AudioSource spiderSFX;
    [SerializeField] public AudioSource slimeSFX;

    public void SkelatonSFX()
    {
        skeletonSwordSFX.Play();
    }
    public void SlimeSFX()
    {
        slimeSFX.Play();
    }
    public void SpiderSFX()
    {
        spiderSFX.Play();
    }
    public void SpiderlingSFX()
    {
        spiderlingSFX.Play();
    }
    public void BomblingSFX()
    {
        skeletonSwordSFX.Play();
    }
    public void SummonerSFX()
    {
        spawnerSFX.Play();
    }
}
