using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class portalKey : pickableItemBase
{
    [SerializeField] AudioSource noiseSource;

    protected override void Start()
    {
        base.Start();

        gameManager.GamePausedEvent += PauseStoneNoise;
        gameManager.GameUnPausedEvent += UnPauseStoneNoise;
    }

    private void OnDestroy()
    {
        gameManager.GamePausedEvent -= PauseStoneNoise;
        gameManager.GameUnPausedEvent -= UnPauseStoneNoise;
    }

    public override void Pickup()
    {
        gameManager.instance.keyPicked();
        base.Pickup();
    }

    void PauseStoneNoise()
    {
        if (noiseSource != null)
            noiseSource.Pause();
    }

    void UnPauseStoneNoise()
    {
        if (noiseSource != null)
            noiseSource.UnPause();
    }
}
