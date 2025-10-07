using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ItemDB : MonoBehaviour
{
    private static Item[] itemDB;
    public Item[] inspectorItemDB;
    void Start()
    {
        itemDB = inspectorItemDB;
    }
    public static bool DoesItemExist(int itemID)
    {
        foreach (Item item in itemDB)
        {
            if (item.itemID == itemID)
            {
                return true;
            }
        }
        return false;
    }
    public static GameObject GetGameObject(int itemID)
    {
        foreach (Item item in itemDB)
        {
            if (item.itemID == itemID)
            {
                return item.gameObject;
            }
        }
        return null;
    }
    public static Sprite GetItemIcon(int itemID)
    {
        foreach (Item item in itemDB)
        {
            if (item.itemID == itemID)
            {
                return item.GetComponentInChildren<SpriteRenderer>().sprite;
            }
        }
        return null;
    }
    public static string GetItemName(int itemID)
    {
        foreach (Item item in itemDB)
        {
            if (item.itemID == itemID)
            {
                return item.itemName;
            }
        }
        return null;
    }
    public static TextAsset GetItemDescription(int itemID)
    {
        foreach (Item item in itemDB)
        {
            if (item.itemID == itemID)
            {
                return item.textAssets[0];
            }
        }
        return null;
    }
    public static bool DoesItemHavePurpose(int itemID)
    {
        foreach (Item item in itemDB)
        {
            if (item.itemID == itemID)
            {
                if (item.textAssets == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        return false;
    }
    public static TextAsset[] GetItemTextAssets(int itemID)
    {
        foreach (Item item in itemDB)
        {
            if (item.itemID == itemID)
            {
                if (item.textAssets == null)
                {
                    return null;
                }
                else
                {
                    return item.textAssets;
                }
            }
        }
        return null;
    }
    public static int[] GetItemIntTags(int itemID)
    {
        foreach (Item item in itemDB)
        {
            if (item.itemID == itemID)
            {
                if (item.intTags == null)
                {
                    return null;
                }
                else
                {
                    return item.intTags;
                }
            }
        }
        return null;
    }
    public static AudioClip GetItemAudioClip(int itemID)
    {
        foreach (Item item in itemDB)
        {
            if (item.itemID == itemID)
            {
                return item.itemSpeechClip;
            }
        }
        return null;
    }
    public static InventoryPage.PageTypes GetItemDestination(int itemID)
    {
        foreach (Item item in itemDB)
        {
            if (item.itemID == itemID)
            {
                return item.destination;
            }
        }
        return InventoryPage.PageTypes.None;
    }
    public static int GetItemStackAmount(int itemID)
    {
        foreach (Item item in itemDB)
        {
            if (item.itemID == itemID)
            {
                return item.stackAmount;
            }
        }
        return 0;
    }
    public static void ExecuteItemPurpose(int itemID, InventoryPage inventoryPage)
    {
        foreach (Item item in itemDB)
        {
            if (item.itemID == itemID)
            {
                switch (item.destination)
                {
                    case InventoryPage.PageTypes.consumables:
                        inventoryPage.inventoryMenuOpened = false;
                        InkDialogueManager.GetInstance().EnterDialogueMode(item.textAssets[1], item.itemName, item.itemName.Replace(" ", string.Empty), ItemDB.GetItemAudioClip(item.itemID));
                        InventoryManager.RemoveAtIndex(InventoryManager.GetSelectedGrid(item.destination), item.itemID, 1);
                        break;
                    default:
                        Debug.LogError("Invalid item destination.");
                        break;
                }

            }
        }
    }

    public static void EnterItemLoreDialogue(int itemID, InventoryPage inventoryPage)
    {
        foreach (Item item in itemDB)
        {
            if (item.itemID == itemID)
            {
                InkDialogueManager.GetInstance().EnterDialogueMode(item.textAssets[0], item.itemName, item.itemName.Replace(" ", string.Empty), ItemDB.GetItemAudioClip(item.itemID));
            }
        }
    }
}