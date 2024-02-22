using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class saveNotification : MonoBehaviour, IActivate
{
    [SerializeField] float visibleDuration;

    [SerializeField] Image backgroundImage;
    [SerializeField] GameObject[] children;

    timer timer;

    private void Start()
    {
        timer = GetComponent<timer>();

        Hide();

        saveSystem.SaveFileUpdated += Show;
    }

    private void OnDestroy()
    {
        saveSystem.SaveFileUpdated -= Show;
    }

    public void Show()
    {
        GetComponent<Image>().enabled = true;
        foreach (GameObject child in children)
            child.gameObject.SetActive(true);

        timer.StartTimer(visibleDuration);
    }
    public void Hide()
    {
        GetComponent<Image>().enabled = false;
        foreach (GameObject child in children)
            child.gameObject.SetActive(false);
    }

    public IEnumerator Activate()
    {
        Hide();

        yield return null;
    }
}
