using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Processors;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static InventoryPage;

public class InventoryManager : MonoBehaviour
{
    //TODO:
    // - Finish NPC dialogue paths
    public InventoryPage[] _inventoryPages;
    public static InventoryPage[] inventoryPages;
    public static bool inventoryOpened;
    public static InventoryPage inventoryPage;
    public int inventoryIndex = 0;
    public static GameObject player;
    public static bool inventoryMenuOpen;
    public static GameObject staticInventoryMenu;
    public TextMeshProUGUI[] textMeshes;
    public static TextMeshProUGUI[] _textMeshes;
    public InventoryManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            UnityEngine.Debug.LogError("More than one instance of dialogue manager found! Please ensure that there is no other dialogue manager present in the scene.");
        }
        inventoryPages = _inventoryPages;
        _inventoryPages = inventoryPages;
        _textMeshes = textMeshes;
        textMeshes = _textMeshes;
    }
    public void Start()
    {
        player = FindFirstObjectByType<Player>().gameObject;
    }
    public void Update()
    {
        textMeshes[0].text = $"{inventoryIndex + 1}/{inventoryPages.Length}";
        UpdateStats();
        if (inventoryOpened)
        {
            if (!InkDialogueManager.GetInstance().dialogueIsPlaying && InkDialogueManager.GetInstance().activeChoices == 0 && !DebugManager.typing && !GameManager.paused)
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.Q))
                {
                    inventoryIndex--;
                }
                if (UnityEngine.Input.GetKeyDown(KeyCode.W))
                {
                    inventoryIndex++;
                }
            }
            if (inventoryIndex < 0)
            {
                inventoryIndex = inventoryPages.Length - 1;
            }
            if (inventoryIndex > inventoryPages.Length - 1)
            {
                inventoryIndex = 0;
            }
            if (inventoryPages[inventoryIndex].pageType != PageTypes.stats)
            {
                inventoryPages[inventoryIndex].UpdateSprites();
            }
            foreach (TextMeshProUGUI tm in textMeshes)
            {
                tm.gameObject.SetActive(true);
            }
            for (int i = 0; i < inventoryPages.Length; i++)
            {
                if (i == inventoryIndex)
                {
                    inventoryPages[i].gameObject.SetActive(true);
                }
                else
                {
                    inventoryPages[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            foreach (TextMeshProUGUI tm in textMeshes)
            {
                tm.gameObject.SetActive(false);
            }
            foreach (InventoryPage page in inventoryPages)
            {
                page.gameObject.SetActive(false);
            }
        }
    }
    public static void Set(int itemID, int amount)
    {
        PageTypes destination = ItemDB.GetItemDestination(itemID);
        if (!IsValidDestination(destination))
        {
            DebugManager.Log($"<Inventory Manager>: Given inventory index does not exist. Check destination of item {ItemDB.GetItemName(itemID)}.");
            return;
        }
        if (!ItemDB.DoesItemExist(itemID))
        {
            DebugManager.Log("<Item Database>: Given item ID does not exist.");
            return;
        }
        if (amount < 0)
        {
            DebugManager.Log("<Inventory Manager>: Amount cannot be less than zero.");
            return;
        }
        if (amount > ItemDB.GetItemStackAmount(itemID))
        {
            DebugManager.Log($"<Inventory Manager>: Amount cannot exceed {ItemDB.GetItemStackAmount(itemID)}.");
            return;
        }
        inventoryPage = GetInventoryPage(destination);
        for (int i = 0; i < inventoryPage.inventory.Length; i++)
        {
            inventoryPage.inventory[i] = itemID;
            inventoryPage.itemAmounts[i] = amount;
        }
        SetInventoryAndAmounts(destination);
        if (itemID == 0)
        {
            DebugManager.Log("<Inventory Manager>: Cleared inventory.");
        }
        else
        {
            DebugManager.Log($"<Inventory Manager>: Set all slots in inventory page {destination + 1} to {ItemDB.GetItemName(itemID)} x {amount}.");
        }
    }
    public static void SetIndex(int index, int itemID, int amount)
    {
        PageTypes destination = ItemDB.GetItemDestination(itemID);
        if (!IsValidDestination(destination))
        {
            DebugManager.Log($"<Inventory Manager>: Given inventory index does not exist. Check destination of item {ItemDB.GetItemName(itemID)}.");
            return;
        }
        if (!ItemDB.DoesItemExist(itemID))
        {
            DebugManager.Log("<Item Database>: Given item ID does not exist.");
            return;
        }
        if (amount < 0)
        {
            DebugManager.Log("<Inventory Manager>: Amount cannot be less than zero.");
            return;
        }
        if (amount > ItemDB.GetItemStackAmount(itemID))
        {
            DebugManager.Log($"<Inventory Manager>: Amount cannot exceed {ItemDB.GetItemStackAmount(itemID)}.");
            return;
        }
        inventoryPage = GetInventoryPage(destination);
        if (index < 0 || index > inventoryPage.inventory.Length - 1)
        {
            DebugManager.Log($"<Inventory Manager>: Invalid inventory page destination given..");
            return;
        }
        inventoryPage.inventory[index] = itemID;
        inventoryPage.itemAmounts[index] = amount;
        SetInventoryAndAmounts(destination);
        DebugManager.Log($"<Inventory Manager>: Set inventory slot index to {ItemDB.GetItemName(itemID)} x {amount}.");
    }
    public static void Add(int itemID, int amount)
    {
        int counter = amount;
        PageTypes destination = ItemDB.GetItemDestination(itemID);
        if (!IsValidDestination(destination))
        {
            DebugManager.Log($"<Inventory Manager>: Given inventory index does not exist. Check destination of item {ItemDB.GetItemName(itemID)}.");
            return;
        }
        if (!ItemDB.DoesItemExist(itemID))
        {
            DebugManager.Log("<Item Database>: Given item ID does not exist.");
            return;
        }
        if (counter < 0)
        {
            DebugManager.Log("<Inventory Manager>: Amount cannot be less than zero.");
            return;
        }
        if (counter > ItemDB.GetItemStackAmount(itemID))
        {
            DebugManager.Log($"<Inventory Manager>: Amount cannot exceed {ItemDB.GetItemStackAmount(itemID)}.");
            return;
        }
        inventoryPage = GetInventoryPage(destination);
        if (IsFull(destination, itemID))
        {
            DebugManager.Log("<Inventory Manager>: Inventory is full! Add request ignored.");
            return;
        }
        for (int i = 0; i < inventoryPage.inventory.Length; i++)
        {
            if (inventoryPage.inventory[i] == 0)
            {
                inventoryPage.inventory[i] = itemID;
                inventoryPage.itemAmounts[i] = counter;
                counter = 0;
                break;
            }
            else if (inventoryPage.inventory[i] == itemID)
            {
                if (inventoryPage.itemAmounts[i] + counter > ItemDB.GetItemStackAmount(itemID))
                {
                    if (inventoryPage.itemAmounts[i] != ItemDB.GetItemStackAmount(itemID) )
                    {
                        counter -= inventoryPage.itemAmounts[i];
                    }
                    inventoryPage.itemAmounts[i] = ItemDB.GetItemStackAmount(itemID);
                }
                else
                {
                    UnityEngine.Debug.Log($"{ItemDB.GetItemName(itemID)} fits into slot with current amount of {inventoryPage.itemAmounts[i]}.");
                    inventoryPage.itemAmounts[i] += counter;
                    counter = 0;
                    break;
                }
            }

        }
        DebugManager.Log($"<Inventory Manager>: Gave {ItemDB.GetItemName(itemID)} x {amount} to Player.");
        SetInventoryAndAmounts(destination);
    }
    public static void Flood(int itemID, int amount)
    {
        PageTypes destination = ItemDB.GetItemDestination(itemID);
        if (!IsValidDestination(destination))
        {
            DebugManager.Log($"<Inventory Manager>: Given inventory index does not exist. Check destination of item {ItemDB.GetItemName(itemID)}.");
            return;
        }
        if (!ItemDB.DoesItemExist(itemID))
        {
            DebugManager.Log("<Item Database>: Given item ID does not exist.");
            return;
        }
        if (amount < 0)
        {
            DebugManager.Log("<Inventory Manager>: Amount cannot be less than zero.");
            return;
        }
        if (amount > ItemDB.GetItemStackAmount(itemID))
        {
            DebugManager.Log($"<Inventory Manager>: Amount cannot exceed {ItemDB.GetItemStackAmount(itemID)}.");
            return;
        }
        inventoryPage = GetInventoryPage(destination);
        for (int i = 0; i < inventoryPage.inventory.Length; i++)
        {
            if (inventoryPage.inventory[i] == 0)
            {
                inventoryPage.inventory[i] = itemID;
                inventoryPage.itemAmounts[i] = amount;
            }
        }
        SetInventoryAndAmounts(destination);
        DebugManager.Log($"<Inventory Manager>: Filled all available slots in inventory with {ItemDB.GetItemName(itemID)} x {amount}.");
    }
    public static void Remove(int itemID, int amount)
    {
        PageTypes destination = ItemDB.GetItemDestination(itemID);
        if (!IsValidDestination(destination))
        {
            DebugManager.Log($"<Inventory Manager>: Given inventory index does not exist. Check destination of item {ItemDB.GetItemName(itemID)}.");
            return;
        }
        if (!ItemDB.DoesItemExist(itemID))
        {
            DebugManager.Log("<Item Database>: Given item ID does not exist.");
            return;
        }
        if (amount < 0)
        {
            DebugManager.Log("<Inventory Manager>: Amount cannot be less than zero.");
            return;
        }
        if (amount > ItemDB.GetItemStackAmount(itemID))
        {
            DebugManager.Log($"<Inventory Manager>: Amount cannot exceed {ItemDB.GetItemStackAmount(itemID)}.");
            return;
        }
        inventoryPage = GetInventoryPage(destination);
        if (!Contains(itemID))
        {
            DebugManager.Log($"<Inventory Manager>: Inventory #{destination} does not contain given item ID! Remove request ignored.");
            return;
        }
        for (int i = inventoryPage.inventory.Length - 1; i >= 0; i--)
        {
            if (inventoryPage.inventory[i] == itemID)
            {
                inventoryPage.itemAmounts[i] -= amount;
                if (inventoryPage.itemAmounts[i] < 0)
                {
                    inventoryPage.inventory[i] = 0;
                    inventoryPage.itemAmounts[i] = 0;
                }
                break;
            }
        }
        SetInventoryAndAmounts(destination);
        if (amount == 1)
        {
            DebugManager.Log($"<Inventory Manager>: Removed one instance of {ItemDB.GetItemName(itemID)} from inventory #{destination}.");
        }
        else
        {
            DebugManager.Log($"<Inventory Manager>: Removed {amount} instances of {ItemDB.GetItemName(itemID)} from inventory #{destination}.");
        }
    }
    public static void RemoveAll(int itemID)
    {
        PageTypes destination = ItemDB.GetItemDestination(itemID);
        if (!IsValidDestination(destination))
        {
            DebugManager.Log($"<Inventory Manager>: Given inventory index does not exist. Check destination of item {ItemDB.GetItemName(itemID)}.");
            return;
        }
        if (!ItemDB.DoesItemExist(itemID))
        {
            DebugManager.Log("<Item Database>: Given item ID does not exist.");
            return;
        }
        inventoryPage = GetInventoryPage(destination);
        if (!Contains(itemID))
        {
            DebugManager.Log($"<Inventory Manager>: Inventory #{destination} does not contain given item ID! Remove request ignored.");
            return;
        }
        for (int i = inventoryPage.inventory.Length - 1; i >= 0; i--)
        {
            if (inventoryPage.inventory[i] == itemID)
            {
                inventoryPage.inventory[i] = 0;
            }
        }
        SetInventoryAndAmounts(destination);
        DebugManager.Log($"<Inventory Manager>: Removed all instances of {ItemDB.GetItemName(itemID)} from inventory #{destination}.");
    }
    public static void RemoveAtIndex(int index, int itemID, int amount)
    {
        PageTypes destination = ItemDB.GetItemDestination(itemID);
        if (!IsValidDestination(destination))
        {
            DebugManager.Log($"<Inventory Manager>: Given inventory index does not exist. Check destination of item {ItemDB.GetItemName(itemID)}.");
            return;
        }
        if (!ItemDB.DoesItemExist(itemID))
        {
            DebugManager.Log("<Item Database>: Given item ID does not exist.");
            return;
        }
        inventoryPage = GetInventoryPage(destination);
        if (index < 0 || index > inventoryPage.inventory.Length - 1)
        {
            DebugManager.Log($"<Inventory Manager>: Invalid inventory page destination given..");
            return;
        }
        if (GetIDFromIndex(index, destination) == itemID)
        {
            inventoryPage.itemAmounts[index] -= amount;
            if (inventoryPage.itemAmounts[index] <= 0)
            {
                inventoryPage.inventory[index] = 0;
                inventoryPage.itemAmounts[index] = 0;
            }
        }
        SetInventoryAndAmounts(destination);
        DebugManager.Log("<Inventory Manager>: Removed " + ItemDB.GetItemName(itemID) + " from inventory slot " + index + ".");
    }
    public static bool Contains(int itemID)
    {
        if (!ItemDB.DoesItemExist(itemID))
        {
            DebugManager.Log("<Item Database>: Given item ID does not exist.");
            return false;
        }
        if (inventoryPage.inventory.Contains(itemID))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static bool IndexContains(int index, int itemID)
    {
        if (!ItemDB.DoesItemExist(itemID))
        {
            DebugManager.Log("<Item Database>: Given item ID does not exist.");
            return false;
        }
        if (index < 0 || index > inventoryPage.inventory.Length - 1)
        {
            DebugManager.Log($"<Inventory Manager>: Invalid inventory page destination given.");
            return false;
        }
        if (inventoryPage.inventory[index] == itemID)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static int GetIDFromIndex(int index, InventoryPage.PageTypes pageType)
    {
        if (!IsValidDestination(pageType))
        {
            DebugManager.Log($"<Inventory Manager>: Invalid inventory page destination given..");
            return 1111111;
        }
        for (int i = 0; i < inventoryPages.Length; i++)
        {
            if (inventoryPages[i].pageType == pageType)
            {
                if (index < 0 || index > inventoryPages[i].inventory.Length - 1)
                {
                    DebugManager.Log($"<Inventory Manager>: Given inventory index does not exist.");
                    return 1111111;
                }
                return inventoryPages[i].inventory[index];
            }
        }
        return 0;
    }
    public static int GetItemCount(int itemID, InventoryPage.PageTypes pageType)
    {
        if (!ItemDB.DoesItemExist(itemID))
        {
            DebugManager.Log("<Item Database>: Given item ID does not exist.");
            return 0;
        }
        if (!IsValidDestination(pageType))
        {
            DebugManager.Log($"<Inventory Manager>: Given inventory index does not exist.");
            return 0;
        }
        int count = 0;
        for (int i = 0; i < inventoryPages.Length; i++)
        {
            if (inventoryPages[i].pageType == pageType)
            {
                for (int j = 0; j < inventoryPages[i].inventory.Length; j++)
                {
                    if ((inventoryPages[i].inventory[j] == itemID))
                    {
                        count += inventoryPages[i].itemAmounts[j];
                    }
                }
            }
        }
        return count;
    }
    public static bool IsFull(InventoryPage.PageTypes pageType, int itemID)
    {
        if (!IsValidDestination(pageType))
        {
            DebugManager.Log($"<Inventory Manager>: Given inventory index does not exist.");
            return false;
        }
        for (int i = 0; i < inventoryPages.Length; i++)
        {
            if (inventoryPages[i].pageType == pageType)
            {
                for (int j = 0; j < inventoryPages[i].inventory.Length; j++)
                {
                    if (inventoryPages[i].inventory[j] == 0)
                    {
                        return false;
                    }
                    else if (inventoryPages[i].inventory[j] == itemID)
                    {
                        if ((inventoryPages[i].itemAmounts[j] + 1) > ItemDB.GetItemStackAmount(itemID))
                        {
                            if ((j + 1) != inventoryPages[i].inventory.Length)
                            {
                                j++;
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                    if (inventoryPages[i].inventory[j] == 0)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
    public static bool IsEmpty(InventoryPage.PageTypes pageType)
    {
        if (!IsValidDestination(pageType))
        {
            DebugManager.Log($"<Inventory Manager>: Given inventory index does not exist.");
            return false;
        }
        for (int i = 0; i < inventoryPages.Length; i++)
        {
            if (inventoryPages[i].pageType == pageType)
            {
                foreach (int item in inventoryPages[i].inventory)
                {
                    if (item != 0)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
    private static bool IsValidDestination(InventoryPage.PageTypes pageType)
    {
        if (pageType == InventoryPage.PageTypes.consumables)
        {
            return true;
        }
        else if (pageType == InventoryPage.PageTypes.weapons)
        {
            return true;
        }
        else if (pageType == InventoryPage.PageTypes.armor)
        {
            return true;
        }
        else if (pageType == InventoryPage.PageTypes.keyItems)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static InventoryPage.PageTypes GetDestinationFromIndex(int index)
    {

        return index switch
        {
            0 => InventoryPage.PageTypes.consumables,
            1 => InventoryPage.PageTypes.weapons,
            2 => InventoryPage.PageTypes.armor,
            3 => InventoryPage.PageTypes.keyItems,
            _ => InventoryPage.PageTypes.None,
        };
    }
    public static void DropItem(int index, InventoryPage.PageTypes pageType)
    {
        int pageIndex = -1;
        PageTypes destination;
        for (int i = 0; i < inventoryPages.Length; i++)
        {
            if (inventoryPages[i].pageType == pageType)
            {
                pageIndex = i;
            }
        }
        if (pageIndex != -1)
        {
            destination = ItemDB.GetItemDestination(inventoryPages[pageIndex].inventory[index]);
        }
        else
        {
            return;
        }
        if (index < 0 || index > 14)
        {
            DebugManager.Log("<Inventory Manager>: Given index must be between 0 and 14.");
            return;
        }
        if (!IsValidDestination(pageType))
        {
            DebugManager.Log($"<Inventory Manager>: Given inventory index does not exist.");
            return;
        }
        if (GetIDFromIndex(index, destination) == 0)
        {
            DebugManager.Log("<Inventory Manager>: Cannot drop empty item.");
            return;
        }
        inventoryMenuOpen = false;
        for (int i = 0; i < inventoryPages.Length; i++)
        {
            if (inventoryPages[i].pageType == pageType)
            {
                inventoryMenuOpen = false;
            }
        }
        inventoryOpened = false;
        GameManager._inventoryOpened = false;
        GameManager.GetInstance().inventoryOpened = false;
        Vector2 pos = new(Player.GetInstance().transform.position.x + .5f, Player.GetInstance().transform.position.y + .5f);
        Quaternion rot = Quaternion.Euler(0f, 0f, 0f);
        GameObject newItem = Instantiate(ItemDB.GetGameObject(GetIDFromIndex(index, destination)), pos, rot);
        newItem.GetComponent<Item>().DelayPickup();
        newItem.transform.parent = null;
        RemoveAtIndex(index, GetIDFromIndex(index, destination), 1);
        newItem.GetComponent<Rigidbody2D>().velocity = new(3, 3);
        DebugManager.Log("<Inventory Manager>: Dropped item from inventory slot " + index + ".");
    }
    public static void FlushInventory(InventoryPage.PageTypes pageType, InventoryPage[] inventoryPages)
    {
        if (!IsValidDestination(pageType))
        {
            DebugManager.Log($"<Inventory Manager>: Given inventory index does not exist.");
            return;
        }
        for (int i = 0; i < inventoryPages.Length; i++)
        {
            for (int j = 0; i < inventoryPages[i].inventory.Length; j++)
            {
                inventoryPages[i].inventory[j] = 0;
            }
        }
    }
    public static InventoryPage[] FlushInventories(InventoryPage[] inventoryPages)
    {
        for (int i = 0; i < inventoryPages.Length; i++)
        {
            for (int j = 0; j < inventoryPages[i].inventory.Length; j++)
            {
                inventoryPages[i].inventory[j] = 0;
                inventoryPages[i].itemAmounts[j] = 0;
            }
        }
        return inventoryPages;
    }
    public static void SetSelectorSpeed(InventoryPage.PageTypes pageType, float speed)
    {
        if (!IsValidDestination(pageType))
        {
            DebugManager.Log($"<Inventory Manager>: Given inventory index does not exist.");
            return;
        }
        for (int i = 0; i < inventoryPages.Length; i++)
        {
            if (inventoryPages[i].pageType == pageType)
            {
                inventoryPages[i].selector.GetComponent<Animator>().speed = speed;
            }
        }
    }
    public static void SetAllSelectorSpeeds(float speed)
    {
        foreach (InventoryPage page in inventoryPages)
        {
            if (page.pageType != PageTypes.stats)
            {
                page.selector.GetComponent<Animator>().speed = speed;
            }
        }
    }
    public static int GetSelectedGrid(InventoryPage.PageTypes pageType)
    {
        if (!IsValidDestination(pageType))
        {
            DebugManager.Log($"<Inventory Manager>: Given inventory index does not exist.");
            return 0;
        }
        for (int i = 0; i < inventoryPages.Length; i++)
        {
            if (inventoryPages[i].pageType == pageType)
            {
                return inventoryPages[i].selectedGrid;
            }
        }
        return 0;
    }
    private static InventoryPage GetInventoryPage(InventoryPage.PageTypes pageType)
    {
        if (!IsValidDestination(pageType))
        {
            DebugManager.Log($"<Inventory Manager>: Given inventory index does not exist.");
            return null;
        }
        for (int i = 0; i < inventoryPages.Length; i++)
        {
            if (inventoryPages[i].pageType == pageType)
            {
                return inventoryPages[i];
            }
        }
        return null;
    }
    private static void SetInventoryAndAmounts(InventoryPage.PageTypes pageType)
    {
        if (!IsValidDestination(pageType))
        {
            DebugManager.Log($"<Inventory Manager>: Given inventory index does not exist.");
            return;
        }
        for (int i = 0; i < inventoryPages.Length; i++)
        {
            if (inventoryPages[i].pageType == pageType)
            {
                inventoryPages[i].inventory = inventoryPage.inventory;
                inventoryPages[i].itemAmounts = inventoryPage.itemAmounts;
            }
        }
    }
    public static int[] GetMassInventory(InventoryPage[] inventoryPages)
    {
        int size = 0;
        foreach (InventoryPage page in inventoryPages)
        {
            if (page.pageType != InventoryPage.PageTypes.None && page.pageType != InventoryPage.PageTypes.stats)
            {
                size += page.inventory.Length;
            }
        }
        int[] massInventory = new int[size];
        int currentIndex = 0;
        foreach (InventoryPage page in inventoryPages)
        {
            if (page.pageType != InventoryPage.PageTypes.None && page.pageType != InventoryPage.PageTypes.stats)
            {
                Array.Copy(page.inventory, 0, massInventory, currentIndex, page.inventory.Length);
                currentIndex += page.inventory.Length;
            }
        }
        return massInventory;
    }
    public static int[] GetMassItemAmount(InventoryPage[] inventoryPages)
    {
        int size = 0;
        foreach (InventoryPage page in inventoryPages)
        {
            if (page.pageType != InventoryPage.PageTypes.None && page.pageType != InventoryPage.PageTypes.stats)
            {
                size += page.itemAmounts.Length;
            }
        }
        int[] massItemAmount = new int[size];
        int currentIndex = 0;
        foreach (InventoryPage page in inventoryPages)
        {
            if (page.pageType != InventoryPage.PageTypes.None && page.pageType != InventoryPage.PageTypes.stats)
            {
                Array.Copy(page.itemAmounts, 0, massItemAmount, currentIndex, page.itemAmounts.Length);
                currentIndex += page.itemAmounts.Length;
            }
        }
        return massItemAmount;
    }
    public static int GetMassInventoryLength(InventoryPage[] inventoryPages)
    {
        int size = 0;
        for (int i = 0; i < inventoryPages.Length; i++)
        {
            if (inventoryPages[i].pageType != InventoryPage.PageTypes.None && inventoryPages[i].pageType != InventoryPage.PageTypes.stats)
            {
                size += inventoryPages[i].inventory.Length;
            }
        }
        return size;
    }
    public static InventoryPage[] SeparateInventory(int[] massInventory, int[] massItemAmount, InventoryPage[] inventoryPages)
    {
        int numPages = inventoryPages.Length;
        InventoryPageFrame[] separatedInventoryPages = new InventoryPageFrame[numPages];
        int currentInventoryIndex = 0;
        for (int i = 0; i < numPages; i++)
        {
            separatedInventoryPages[i] = new InventoryPageFrame();
            int inventoryLength = 0;
            if (inventoryPages[i].pageType != InventoryPage.PageTypes.stats)
            {
                inventoryLength = inventoryPages[i].inventory.Length;
            }
            separatedInventoryPages[i].inventory = new int[inventoryLength];
            Array.Copy(massInventory, currentInventoryIndex, separatedInventoryPages[i].inventory, 0, inventoryLength);
            currentInventoryIndex += inventoryLength;
        }
        for (int i = 0; i < separatedInventoryPages.Length; i++)
        {
            inventoryPages[i].inventory = separatedInventoryPages[i].inventory;
        }
        int currentAmountIndex = 0;
        for (int i = 0; i < numPages; i++)
        {
            separatedInventoryPages[i] = new InventoryPageFrame();
            int amountLength = 0;
            if (inventoryPages[i].pageType != InventoryPage.PageTypes.stats)
            {
                amountLength = inventoryPages[i].itemAmounts.Length;
            }
            separatedInventoryPages[i].itemAmounts = new int[amountLength];
            Array.Copy(massItemAmount, currentAmountIndex, separatedInventoryPages[i].itemAmounts, 0, amountLength);
            currentAmountIndex += amountLength;
        }

        for (int i = 0; i < separatedInventoryPages.Length; i++)
        {
            if (inventoryPages[i].pageType != InventoryPage.PageTypes.None && inventoryPages[i].pageType != InventoryPage.PageTypes.stats)
            {
                inventoryPages[i].itemAmounts = separatedInventoryPages[i].itemAmounts;
            }
        }

        return inventoryPages;
    }
    public static InventoryPage[] SeparateItemAmount(int[] massItemAmount, InventoryPage[] inventoryPages)
    {
        int numPages = inventoryPages.Length;
        InventoryPageFrame[] separatedInventoryPages = new InventoryPageFrame[numPages];

        int currentIndex = 0;
        for (int i = 0; i < numPages; i++)
        {
            separatedInventoryPages[i] = new InventoryPageFrame();
            int pageLength = inventoryPages[i].itemAmounts.Length;
            separatedInventoryPages[i].itemAmounts = new int[pageLength];
            Array.Copy(massItemAmount, currentIndex, separatedInventoryPages[i].itemAmounts, 0, pageLength);
            currentIndex += pageLength;
        }

        for (int i = 0; i < separatedInventoryPages.Length; i++)
        {
            inventoryPages[i].itemAmounts = separatedInventoryPages[i].itemAmounts;
        }

        return inventoryPages;
    }
    public static void CloseInventory()
    {
        if (inventoryPages != null)
        {
            foreach (TextMeshProUGUI tm in _textMeshes)
            {
                tm.gameObject.SetActive(false);
            }
            foreach (InventoryPage page in inventoryPages)
            {
                page.gameObject.SetActive(false);
            }
        }
    }
    public void UpdateStats()
    {
        int healthModifier = 0;
        int speedModifier = 0;
        int jumpModifier = 0;
        int attackModifier = 0;
        int defenseModifier = 0;
        for (int i = 0; i < inventoryPages.Length; i++)
        {
            if (inventoryPages[i].pageType == InventoryPage.PageTypes.weapons)
            {
                for (int j = 0; j < inventoryPages[i].equippedWeaponGrids.Length; j++)
                {
                    if (inventoryPages[i].equippedWeaponGrids[j] != 1111111)
                    {
                        inventoryPages[i].grids[inventoryPages[i].equippedWeaponGrids[j]].itemEquipped = true;
                        int item = inventoryPages[i].inventory[inventoryPages[i].equippedWeaponGrids[j]];
                        if (item != 0)
                        {
                            int[] weaponTags = ItemDB.GetItemIntTags(item);
                            healthModifier += weaponTags[0];
                            speedModifier += weaponTags[1];
                            jumpModifier += weaponTags[2];
                            attackModifier += weaponTags[3];
                            defenseModifier += weaponTags[4];
                        }
                        else
                        {
                            inventoryPages[i].grids[inventoryPages[i].equippedWeaponGrids[j]].itemEquipped = false;
                        }
                    }
                }
            }
        }
        for (int i = 0; i < inventoryPages.Length; i++)
        {
            if (inventoryPages[i].pageType == InventoryPage.PageTypes.armor)
            {
                if (inventoryPages[i].equippedArmorGrid != 1111111)
                {
                    inventoryPages[i].grids[inventoryPages[i].equippedArmorGrid].itemEquipped = true;
                    int equippedArmor = inventoryPages[i].inventory[inventoryPages[i].equippedArmorGrid];
                    if (inventoryPages[i].pageType == InventoryPage.PageTypes.armor)
                    {
                        if (equippedArmor != 0)
                        {
                            Player.GetInstance().armorSprite.sprite = ItemDB.GetItemIcon(equippedArmor);
                            int[] armorTags = ItemDB.GetItemIntTags(equippedArmor);
                            healthModifier += armorTags[0];
                            speedModifier += armorTags[1];
                            jumpModifier += armorTags[2];
                            attackModifier += armorTags[3];
                            defenseModifier += armorTags[4];
                        }
                        else
                        {
                            Player.GetInstance().armorSprite.sprite = null;
                        }
                    }
                }
                else
                {
                    Player.GetInstance().armorSprite.sprite = null;
                }
            }
        }
        Player.GetInstance().maxHealth = Player.GetInstance().defaultMaxHealth + healthModifier;
        Player.GetInstance().speed = Player.GetInstance().defaultSpeed + speedModifier;
        Player.GetInstance().jumpPower = Player.GetInstance().defaultJump + jumpModifier;
        Player.GetInstance().attack = Player.GetInstance().defaultAttack + attackModifier;
        Player.GetInstance().defense = Player.GetInstance().defaultDefense + defenseModifier;
    }
}