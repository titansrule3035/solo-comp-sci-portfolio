using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InventoryMenu : MonoBehaviour
{
    public InventoryPage parentPage;
    public TextMeshProUGUI titleText;
    public GameObject[] options;
    public TextMeshProUGUI[] optionTexts;
    public GameObject[] activeOptions;
    public int activeOptionsNo;
    public GameObject selector;
    public int selectorIndex = 0;
    bool hasInventoryMenu = false;
    InventoryPage.PageTypes pageType;
    private void Start()
    {
        pageType = parentPage.pageType;
        activeOptions = options;
        if (pageType != InventoryPage.PageTypes.stats || pageType != InventoryPage.PageTypes.None)
        {
            hasInventoryMenu = true;
            titleText.text = ItemDB.GetItemName(parentPage.inventory[parentPage.selectedGrid]);
        }
    }
    private void Update()
    {
        if (hasInventoryMenu)
        {
            titleText.text = ItemDB.GetItemName(parentPage.inventory[parentPage.selectedGrid]);
            if (!InkDialogueManager.GetInstance().dialogueIsPlaying && InkDialogueManager.GetInstance().activeChoices == 0 && !DebugManager.typing && !GameManager.paused)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    selectorIndex--;
                }
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    selectorIndex++;
                }
            }
            if (selectorIndex < 0)
            {
                selectorIndex = activeOptions.Length - 1;
            }
            if (selectorIndex > activeOptions.Length - 1)
            {
                selectorIndex = 0;
            }
            selector.transform.position = activeOptions[selectorIndex].transform.position;
            if (parentPage.pageType == InventoryPage.PageTypes.consumables)
            {
                SetTexts("Use", "Info", true, true, true, true);
            }
            else if (parentPage.pageType == InventoryPage.PageTypes.weapons)
            {
                bool found = false;
                for (int i = 0; i < parentPage.equippedWeaponGrids.Length; i++)
                {
                    if (parentPage.equippedWeaponGrids[i] == parentPage.selectedGrid)
                    {
                        found = true;
                        break;
                    }
                }
                if (found)
                {
                    SetTexts("Unequip", "Info", true, true, false, true);
                }
                else
                {
                    if (AreWeaponGridsFull())
                    {
                        SetTexts("", "Info", false, true, true, true);
                    }
                    else
                    {
                        SetTexts("Equip", "Info", true, true, true, true);
                    }
                }
            }
            else if (parentPage.pageType == InventoryPage.PageTypes.armor)
            {
                if (parentPage.selectedGrid == parentPage.equippedArmorGrid)
                {
                    SetTexts("Unequip", "Info", true, true, false, true);
                }
                else
                {
                    SetTexts("Equip", "Info", true, true, true, true);
                }
            }
            else if (parentPage.pageType == InventoryPage.PageTypes.keyItems)
            {
                SetTexts("", "Info", false, true, true, true);
            }
            else
            {
                Debug.LogError("Parental Inventory Page is useless! Change page type or remove the page.");
            }
            if (Input.GetKeyDown(KeyCode.Z) && !InkDialogueManager.GetInstance().dialogueIsPlaying && InkDialogueManager.GetInstance().activeChoices == 0 && !DebugManager.typing && !GameManager.paused)
            {
                if (parentPage.inventory[parentPage.selectedGrid] != 0)
                {
                    OnButtonPressed();
                }
            }
        }

    }
    private void SetTexts(string text1, string text2, bool button1, bool button2, bool button3, bool button4)
    {
        optionTexts[0].text = text1;
        optionTexts[1].text = text2;
        this.options[0].SetActive(button1);
        this.options[1].SetActive(button2);
        this.options[2].SetActive(button3);
        this.options[3].SetActive(button4);
        ControlPosition(options.ToList());
    }
    public void HandleOptions()
    {
        pageType = parentPage.pageType;
        if (pageType != InventoryPage.PageTypes.stats || pageType != InventoryPage.PageTypes.None)
        {
            titleText.text = ItemDB.GetItemName(parentPage.inventory[parentPage.selectedGrid]);
            if (parentPage.pageType == InventoryPage.PageTypes.consumables)
            {
                SetTexts("Use", "Info", true, true, true, true);
            }
            else if (parentPage.pageType == InventoryPage.PageTypes.weapons)
            {
                bool found = false;
                for (int i = 0; i < parentPage.equippedWeaponGrids.Length; i++)
                {
                    if (parentPage.equippedWeaponGrids[i] == parentPage.inventory[parentPage.selectedGrid])
                    {
                        found = true;
                        break;
                    }
                }
                if (found)
                {
                    SetTexts("Unequip", "Info", true, true, false, true);
                }
                else
                {
                    if (AreWeaponGridsFull())
                    {
                        SetTexts("", "Info", false, true, true, true);
                    }
                    else
                    {
                        SetTexts("Equip", "Info", true, true, true, true);
                    }
                }
            }
            else if (parentPage.pageType == InventoryPage.PageTypes.armor)
            {
                if (parentPage.selectedGrid == parentPage.equippedArmorGrid)
                {
                    SetTexts("Unequip", "Info", true, true, false, true);
                }
                else
                {
                    SetTexts("Equip", "Info", true, true, true, true);
                }
            }
            else if (parentPage.pageType == InventoryPage.PageTypes.keyItems)
            {
                SetTexts("", "Info", false, true, true, true);
            }
            else
            {
                Debug.LogError("Parental Inventory Page is useless! Change page type or remove the page.");
            }
        }
    }
    public void ControlPosition(List<GameObject> options)
    {
        List<GameObject> activeOptions = new();
        int activeOptionsNo = 0;
        for (int i = 0; i < options.Count; i++)
        {
            if (options[i].activeSelf)
            {
                activeOptions.Add(options[i]);
                activeOptionsNo++;
            }
        }
        switch (activeOptionsNo)
        {
            case 0:
                return;
            case 1:
                activeOptions[0].transform.localPosition = Vector2.zero;
                break;
            case 2:
                activeOptions[0].transform.localPosition = new(0, 37);
                activeOptions[1].transform.localPosition = new(0, activeOptions[0].transform.localPosition.y - 114);
                break;
            case 3:
                activeOptions[0].transform.localPosition = new(0, 67);
                activeOptions[1].transform.localPosition = new(0, activeOptions[0].transform.localPosition.y - 114);
                activeOptions[2].transform.localPosition = new(0, activeOptions[1].transform.localPosition.y - 114);
                break;
            case 4:
                activeOptions[0].transform.localPosition = new(0, 142);
                activeOptions[1].transform.localPosition = new(0, activeOptions[0].transform.localPosition.y - 114);
                activeOptions[2].transform.localPosition = new(0, activeOptions[1].transform.localPosition.y - 114);
                activeOptions[3].transform.localPosition = new(0, activeOptions[2].transform.localPosition.y - 114);
                break;
        }
        this.activeOptions = activeOptions.ToArray();
    }
    public void OnButtonPressed()
    {
        int itemID = parentPage.inventory[parentPage.selectedGrid];
        if (selectorIndex == 0)
        {
            if (options[0] == activeOptions[0])
            {
                Button1();
            }
            else if (options[1] == activeOptions[0])
            {
                Button2();
            }
            else if (options[2] == activeOptions[0])
            {
                Button3();
            }
            else if (options[3] == activeOptions[0])
            {
                Button4();
            }
        }
        if (selectorIndex == 1)
        {
            if (options[1] == activeOptions[1])
            {
                Button2();
            }
            else if (options[2] == activeOptions[1])
            {
                Button3();
            }
            else if (options[3] == activeOptions[1])
            {
                Button4();
            }
        }
        if (selectorIndex == 2)
        {
            if (options[2] == activeOptions[2])
            {
                Button3();
            }
            else if (options[3] == activeOptions[2])
            {
                Button4();
            }
        }
        if (selectorIndex == 3)
        {
            if (options[3] == activeOptions[3])
            {
                Button4();
            }
        }
        void Button1()
        {
            parentPage.inventoryMenuOpened = false;
            gameObject.SetActive(false);
            switch (ItemDB.GetItemDestination(itemID))
            {
                case InventoryPage.PageTypes.consumables:
                    ItemDB.ExecuteItemPurpose(itemID, parentPage);
                    break;
                case InventoryPage.PageTypes.armor:
                    if (parentPage.equippedArmorGrid == parentPage.selectedGrid)
                    {
                        parentPage.equippedArmorGrid = 1111111;
                        parentPage.grids[parentPage.selectedGrid].itemEquipped = false;
                    }
                    else
                    {
                        if (parentPage.equippedArmorGrid != 1111111)
                        {
                            parentPage.grids[parentPage.equippedArmorGrid].itemEquipped = false;
                        }
                        parentPage.equippedArmorGrid = parentPage.selectedGrid;
                        parentPage.grids[parentPage.selectedGrid].itemEquipped = true;
                    }
                    break;
                case InventoryPage.PageTypes.weapons:
                    bool found = false;
                    for (int i = 0; i < parentPage.equippedWeaponGrids.Length; i++)
                    {
                        if (parentPage.equippedWeaponGrids[i] == parentPage.selectedGrid)
                        {
                            found = true;
                            parentPage.equippedWeaponGrids[i] = 1111111;
                            parentPage.grids[parentPage.selectedGrid].itemEquipped = false;
                            return;
                        }
                    }
                    if (!found && !AreWeaponGridsFull())
                    {
                        for (int i = 0; i < parentPage.equippedWeaponGrids.Length; i++)
                        {
                            if (parentPage.equippedWeaponGrids[i] == 1111111)
                            {
                                parentPage.equippedWeaponGrids[i] = parentPage.selectedGrid;
                                parentPage.grids[parentPage.selectedGrid].itemEquipped = true;
                                break;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        void Button2()
        {
            ItemDB.EnterItemLoreDialogue(itemID, parentPage);

        }
        void Button3()
        {
            InventoryManager.DropItem(parentPage.selectedGrid, parentPage.pageType);
            parentPage.inventoryMenuOpened = false;
            gameObject.SetActive(false);
        }
        void Button4()
        {
            parentPage.inventoryMenuOpened = false;
            gameObject.SetActive(false);
        }
    }
    private bool AreWeaponGridsFull()
    {
        bool full = true;
        for (int i = 0; i < parentPage.equippedWeaponGrids.Length; i++)
        {
            if (parentPage.equippedWeaponGrids[i] == 1111111)
            {
                full = false;
                break;
            }
        }
        return full;
    }

    private void OnEnable()
    {
        Start();
        ControlPosition(activeOptions.ToList());
    }
    private void OnDisable()
    {
        selectorIndex = 0;
        parentPage.selector.SetActive(true);
    }
}
