using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor;
# endif

public class InventoryPage : MonoBehaviour
{
    public enum PageTypes { None, consumables, weapons, armor, keyItems, stats }
    public PageTypes pageType;
    [HideInInspector] public int[] inventory;
    [HideInInspector] public int[] itemAmounts;
    [HideInInspector] public int equippedArmorGrid;
    [HideInInspector] public int[] equippedWeaponGrids;
    [HideInInspector] public GameObject inventoryMenu;
    [HideInInspector] public bool inventoryMenuOpened = false;
    [HideInInspector] public GameObject inventoryPanel;
    public InventoryGrid[] grids;
    [HideInInspector] public GameObject selector;
    [HideInInspector] public int maxSelectorXIndex;
    [HideInInspector] public int maxSelectorYIndex;
    [HideInInspector] public int[] selectorPos;
    [HideInInspector] public int selectedGrid = 0;
    [HideInInspector] public TextMeshProUGUI[] statTextObjects;
    [HideInInspector] public string[] statTexts;
    void Start()
    {
        if (pageType != PageTypes.stats)
        {
            selectorPos = new int[2] { 0, 0 };
            inventoryMenu.GetComponent<InventoryMenu>().parentPage = this;
        }
    }
    void Update()
    {
        if (pageType != InventoryPage.PageTypes.None && pageType != InventoryPage.PageTypes.stats)
        {
            if (inventory.Length != grids.Length)
            {
                Debug.LogError("Inventory array size does not match the total number of grids!");
            }
            UpdateSprites();
            if (InkDialogueManager.GetInstance().activeChoices == 0 && !DebugManager.typing && !GameManager.paused && !InkDialogueManager.GetInstance().dialogueIsPlaying && !inventoryMenuOpened)
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
                {
                    selectorPos[0]++;
                }
                if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    selectorPos[0]--;
                }
                if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
                {
                    selectorPos[1]++;
                }
                if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
                {
                    selectorPos[1]--;
                }
                if (Input.GetKeyDown(KeyCode.Z) && !inventoryMenuOpened && InkDialogueManager.GetInstance().activeChoices == 0 && !DebugManager.typing && !GameManager.paused && !InkDialogueManager.GetInstance().dialogueIsPlaying && inventory[selectedGrid] != 0 && pageType != PageTypes.stats && pageType != PageTypes.None)
                {
                    inventoryMenuOpened = true;
                    inventoryMenu.GetComponent<InventoryMenu>().HandleOptions();
                    selector.SetActive(false);
                }
                inventoryMenu.SetActive(inventoryMenuOpened);
            }
            if (selectorPos[0] < 0)
            {
                selectorPos[0] = maxSelectorXIndex;
            }
            if (selectorPos[0] > maxSelectorXIndex)
            {
                selectorPos[0] = 0;
            }
            if (selectorPos[1] < 0)
            {
                selectorPos[1] = maxSelectorYIndex;
            }
            if (selectorPos[1] > maxSelectorYIndex)
            {
                selectorPos[1] = 0;
            }
            selectedGrid = (selectorPos[0] + selectorPos[1] * (maxSelectorXIndex + 1));
            selector.transform.position = grids[selectedGrid].gameObject.transform.position;
        }
        else if (pageType == PageTypes.stats)
        {
            HandleStats(this);
        }
        else
        {
            Debug.LogError("Inventory Page is useless! Delete page or change page type.");
        }
    }
    public void HandleStats(InventoryPage inventoryPage)
    {
        inventoryPage.grids[5].itemImage.sprite = Player.GetInstance().playerSprite.GetComponent<SpriteRenderer>().sprite;
        for (int i = 0; i < InventoryManager.inventoryPages.Length; i++)
        {
            if (InventoryManager.inventoryPages[i].pageType == PageTypes.weapons)
            {
                for (int j = 0; j < InventoryManager.inventoryPages[i].equippedWeaponGrids.Length; j++)
                {
                    if (InventoryManager.inventoryPages[i].equippedWeaponGrids[j] != 1111111)
                    {
                        if (InventoryManager.inventoryPages[i].inventory[InventoryManager.inventoryPages[i].equippedWeaponGrids[j]] != 0)
                        {
                            inventoryPage.grids[j].itemImage.sprite = ItemDB.GetItemIcon(InventoryManager.inventoryPages[i].inventory[InventoryManager.inventoryPages[i].equippedWeaponGrids[j]]);
                            inventoryPage.grids[j].itemEquipped = true;
                        }
                        else
                        {
                            InventoryManager.inventoryPages[i].equippedWeaponGrids[j] = 1111111;
                        }
                    }
                    else
                    {
                        inventoryPage.grids[j].itemImage.sprite = ItemDB.GetItemIcon(0);
                        inventoryPage.grids[j].itemEquipped = false;
                    }
                }
            }
            if (InventoryManager.inventoryPages[i].pageType == PageTypes.armor)
            {
                inventoryPage.grids[5].itemEquipped = true;
                if (InventoryManager.inventoryPages[i].equippedArmorGrid != 1111111)
                {
                    if (InventoryManager.inventoryPages[i].inventory[InventoryManager.inventoryPages[i].equippedArmorGrid] != 0)
                    {
                        inventoryPage.grids[5].backgroundImage.sprite = Player.GetInstance().armorSprite.GetComponent<SpriteRenderer>().sprite;
                        inventoryPage.grids[5].itemEquipped = true;
                    }
                    else
                    {
                        InventoryManager.inventoryPages[i].grids[InventoryManager.inventoryPages[i].equippedArmorGrid].itemEquipped = false;
                        InventoryManager.inventoryPages[i].equippedArmorGrid = 1111111;
                    }
                }
                else
                {
                    inventoryPage.grids[5].itemEquipped = false;
                }
            }
        }
        inventoryPage.statTextObjects[0].text = $"{Player.GetInstance().maxHealth}";
        inventoryPage.statTextObjects[1].text = $"{Player.GetInstance().speed}";
        inventoryPage.statTextObjects[2].text = $"{Player.GetInstance().jumpPower}";
        inventoryPage.statTextObjects[3].text = $"{Player.GetInstance().defense}";
        inventoryPage.statTextObjects[4].text = $"{Player.GetInstance().attack}";
    }

    public void UpdateSprites()
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            grids[i].itemImage.sprite = ItemDB.GetItemIcon(inventory[i]);
            if (itemAmounts[i] <= 1)
            {
                grids[i].counter.text = "";
            }
            else
            {
                grids[i].counter.text = $"x {itemAmounts[i]}";
            }
        }
    }
    private void OnDisable()
    {
        if (pageType != PageTypes.stats)
        {
            inventoryMenuOpened = false;
            inventoryMenu.SetActive(false);
            selector.SetActive(true);
        }
    }
}
public class InventoryPageFrame
{
    public int[] inventory;
    public int[] itemAmounts;
}