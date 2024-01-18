using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class levelBarrier : MonoBehaviour, ILockable
{
    [SerializeField] BoxCollider barrierCollider;
    [SerializeField] TMP_Text textMesh;
    [SerializeField] GameObject panel;

    [Header("Settings")]
    [TextArea(minLines:1, maxLines:3)]
    [SerializeField] string messageLocked;
    [TextArea(minLines:1, maxLines:3)]
    [SerializeField] string messageUnlocked;
    [SerializeField] Color textColorLocked;
    [SerializeField] Color textColorUnlocked;
    [SerializeField] Color panelColorLocked;
    [SerializeField] Color panelColorUnlocked;

    bool isLocked;


    // Start is called before the first frame update
    void Start()
    {
        isLocked = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ToggleLock()
    {
        isLocked = !isLocked;
        if (isLocked)
            Lock();
        else
            Unlock();
    }

    public void Lock()
    {
        barrierCollider.enabled = true;
        textMesh.text = messageLocked;
        textMesh.color = textColorLocked;
        panel.GetComponent<Image>().color = panelColorLocked;
    }

    public void Unlock()
    {
        barrierCollider.enabled = false;
        textMesh.text = messageUnlocked;
        textMesh.color = textColorUnlocked;
        panel.GetComponent<Image>().color = panelColorUnlocked;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !gameManager.instance.isKeyPicked)
        {
            gameManager.instance.ShowHint("Can't proceed: Find Key Card");
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
