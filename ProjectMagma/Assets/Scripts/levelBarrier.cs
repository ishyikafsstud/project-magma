using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class levelBarrier : MonoBehaviour
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

    void ToggleLock()
    {
        isLocked = !isLocked;
        if (isLocked)
            Lock();
        else
            Unlock();
    }

    void Lock()
    {
        barrierCollider.enabled = true;
        textMesh.text = messageLocked;
        textMesh.color = textColorLocked;
        panel.GetComponent<Image>().color = panelColorLocked;
    }

    void Unlock()
    {
        barrierCollider.enabled = false;
        textMesh.text = messageUnlocked;
        textMesh.color = textColorUnlocked;
        panel.GetComponent<Image>().color = panelColorUnlocked;
    }
}
