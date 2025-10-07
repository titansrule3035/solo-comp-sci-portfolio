using Ink.Parsed;
using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkDialogueTrigger : MonoBehaviour
{
    [HideInInspector] public string triggerName;

    public string npcName;

    public AudioClip speechClip;

    public BoxCollider2D radius;

    public TextAsset inkJSON;

    public InkDialogueTrigger[] transitions;

    public bool hasTransition;

    public bool playerInRange = false;

    private bool lockBool;

    private void Update()
    {
        if (radius != null)
        {
            if (playerInRange && !FindFirstObjectByType<InkDialogueManager>().dialogueIsPlaying)
            {
                Player.GetInstance().inRange = true;
                lockBool = true;
                if (Input.GetKeyDown(KeyCode.Z) && InkDialogueManager.GetInstance().activeChoices == 0 && !InventoryManager.inventoryOpened && !DebugManager.typing && !GameManager.paused)
                {
                    if (hasTransition == true)
                    {
                        FindFirstObjectByType<InkDialogueManager>().EnterDialogueMode(this, inkJSON, npcName, speechClip);
                    }
                    else
                    {
                        FindFirstObjectByType<InkDialogueManager>().EnterDialogueMode(this, inkJSON, npcName, speechClip, radius);
                    }
                }
            }
            else
            {
                if (lockBool)
                {
                    Player.GetInstance().inRange = false;
                    lockBool = false;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (radius != null)
        {
            if (collision.CompareTag("Player"))
            {
                playerInRange = true;
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (radius != null)
        {
            if (collision.CompareTag("Player"))
            {
                playerInRange = false;
            }
        }
    }
    public void RefreshTrigger()
    {
        StartCoroutine(RebootAndEnableCollider());
    }
    private IEnumerator RebootAndEnableCollider()
    {
        yield return new WaitForEndOfFrame();
        radius.enabled = false;
        yield return new WaitForEndOfFrame();
        radius.enabled = true;
    }
    private void OnEnable()
    {
        if (transitions != null && transitions.Length > 0)
        {
            if (transitions[0] == null)
            {
                hasTransition = false;
            }
            else
            {
                hasTransition = true;
            }
        }
        else
        {
            hasTransition = false;
        }
        triggerName = gameObject.name;
    }
}