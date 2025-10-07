using UnityEngine;
using System;
using System.IO;
using Ink.Parsed;
using Ink.Runtime;
using System.Collections;
using JetBrains.Annotations;

public class Item : MonoBehaviour
{
    [HideInInspector] public string itemName;
    [HideInInspector] public int itemID;
    public InventoryPage.PageTypes destination;
    [HideInInspector] public int stackAmount;
    [HideInInspector] public bool showTextAssets;
    [HideInInspector] public TextAsset[] textAssets;
    [HideInInspector] public TextAsset[] defaultTextAssets;
    [HideInInspector] public bool showStats = false;
    [HideInInspector] public int[] intTags;
    [HideInInspector] public string[] stringTags;
    [HideInInspector] public AudioClip itemSpeechClip;
    private GameObject player;
    [SerializeField] private Collider2D selfCollider;
    private bool canPickup = false;
    private bool lockBool = false;

    private void Awake()
    {
        player = FindFirstObjectByType<Player>().gameObject;
        selfCollider = GetComponent<BoxCollider2D>();
    }
    private void Update()
    {
        if (!canPickup)
        {
            lockBool = true;
        }
        if (lockBool)
        {
            StartCoroutine(Wait());
            lockBool = false;
        }
        if (Physics2D.IsTouching(selfCollider, player.GetComponent<BoxCollider2D>()) && !InventoryManager.IsFull(destination, itemID) && canPickup && !GameManager.paused)
        {
            InventoryManager.Add(itemID, 1);
            Destroy(gameObject);
        }
    }
    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1f);
        canPickup = true;
    }
    public void DelayPickup()
    {
        canPickup = false;
    }
}