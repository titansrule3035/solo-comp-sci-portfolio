using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static InventoryPage;

public class GameManager : MonoBehaviour
{
    public GameObject inventoryPanel;
    public bool inventoryOpened = false;
    public static bool _inventoryOpened;
    public int itemNo;
    public int inventoryIndex;
    public int dialogueNo;

    public static bool paused;
    public GameObject pauseMenu;
    public GameObject[] pauseMenuOptions;
    public GameObject pauseMenuSelector;
    public int pauseOption;
    public GameObject savedPopup;

    public static bool load = false;
    public static bool newGame = false;

    public TextAsset dummyTA;
    public AudioSource audioBlip;

    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("More than one instance of dialogue manager found! Please ensure that there is no other dialogue manager present in the scene.");
        }
    }
    private void Start()
    {
        inventoryOpened = _inventoryOpened;
        _inventoryOpened = inventoryOpened;
        inventoryOpened = false;
        load = true;
        paused = false;
        SaveManager.NormalizeDir();
        if (newGame)
        {
            SaveManager.WipeSaveData();
            newGame = false;
        }
        else
        {
            InventoryManager.inventoryPages = SaveManager.LoadGame(InventoryManager.inventoryPages);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !paused)
        {
            paused = true;
            InventoryManager.SetAllSelectorSpeeds(0);
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && paused)
        {
            paused = false;
            InventoryManager.SetAllSelectorSpeeds(1);
        }
        if (paused)
        {
            Time.timeScale = 0;
            pauseMenu.SetActive(true);
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                pauseOption--;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                pauseOption++;
            }
            if (pauseOption < 1)
            {
                pauseOption = 3;
            }
            if (pauseOption > 3)
            {
                pauseOption = 1;
            }
            if (pauseOption == 1)
            {
                pauseMenuSelector.transform.position = pauseMenuOptions[0].transform.position;
            }
            if (pauseOption == 2)
            {
                pauseMenuSelector.transform.position = pauseMenuOptions[1].transform.position;
            }
            if (pauseOption == 3)
            {
                pauseMenuSelector.transform.position = pauseMenuOptions[2].transform.position;
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (pauseOption == 1)
                {
                    paused = false;
                }
                else if (pauseOption == 2)
                {
                    savedPopup.GetComponent<Animator>().SetTrigger("popup");
                    SaveManager.SaveGame(InventoryManager.inventoryPages);
                }
                else if (pauseOption == 3)
                {
                    SceneManager.LoadScene(0);
                    inventoryOpened = false;
                    InventoryManager.CloseInventory();
                    pauseMenu.SetActive(false);
                    paused = false;
                }
            }
        }
        else
        {
            pauseMenu.SetActive(false);
            pauseOption = 4;
            Time.timeScale = 1;
            if (!InkDialogueManager.GetInstance().dialogueIsPlaying)
            {
                if (Input.GetKeyDown(KeyCode.E) && !InventoryManager.inventoryMenuOpen && !DebugManager.typing)
                {
                    for (int i = 0; i < InventoryManager.inventoryPages.Length; i++)
                    {
                        if (InventoryManager.inventoryPages[i].pageType == PageTypes.stats)
                        {
                            InventoryManager.inventoryPages[i].HandleStats(InventoryManager.inventoryPages[i]);
                        }
                    }
                    ToggleInv();
                }
            }
            if (inventoryOpened)
            {
                InventoryManager.inventoryOpened = true;
                Time.timeScale = 0;
            }
            else
            {
                InventoryManager.inventoryOpened = false;
                Time.timeScale = 1;
            }
        }
    }
    void ToggleInv()
    {
        if (inventoryOpened == true)
        {
            inventoryOpened = false;
        }
        else
        {
            inventoryOpened = true;
        }
    }
    public static GameManager GetInstance()
    {
        return instance;
    }
    public void Transition(int choiceID, InkDialogueTrigger trigger)
    {
        string dialogueTriggerName = $"{trigger.npcName}/{trigger.triggerName.Replace(" ", string.Empty)}";
        switch (dialogueTriggerName)
        {
            case "NPC/NPC1":
                InventoryPage.PageTypes pageType = InventoryManager.GetDestinationFromIndex(choiceID);
                System.Random random = new();
                int itemID = 0;

                if (choiceID != 3)
                {
                    switch (choiceID)
                    {
                        case 0:
                            itemID = random.Next(1, 9);
                            if (InventoryManager.IsFull(pageType, itemID))
                            {
                                InkDialogueManager.GetInstance().EnterDialogueMode(trigger.transitions[0], trigger.transitions[0].inkJSON, "NPC", trigger.transitions[0].speechClip);
                            }
                            else
                            {
                                InkDialogueManager.GetInstance().EnterDialogueMode(trigger.transitions[1], trigger.transitions[1].inkJSON, "NPC", trigger.transitions[1].speechClip);
                            }
                            break;
                        case 1:
                            itemID = random.Next(19201, 19205);
                            if (InventoryManager.IsFull(pageType, itemID))
                            {
                                InkDialogueManager.GetInstance().EnterDialogueMode(trigger.transitions[2], trigger.transitions[2].inkJSON, "NPC", trigger.transitions[2].speechClip);
                            }
                            else
                            {
                                InkDialogueManager.GetInstance().EnterDialogueMode(trigger.transitions[3], trigger.transitions[3].inkJSON, "NPC", trigger.transitions[3].speechClip);
                            }
                            break;
                        case 2:
                            itemID = random.Next(12001, 12005);
                            if (InventoryManager.IsFull(pageType, itemID))
                            {
                                InkDialogueManager.GetInstance().EnterDialogueMode(trigger.transitions[4], trigger.transitions[4].inkJSON, "NPC", trigger.transitions[4].speechClip);
                            }
                            else
                            {
                                InkDialogueManager.GetInstance().EnterDialogueMode(trigger.transitions[5], trigger.transitions[5].inkJSON, "NPC", trigger.transitions[5].speechClip);
                            }
                            break;
                    }
                }
                else
                {
                    InkDialogueManager.GetInstance().EnterDialogueMode(trigger.transitions[6], trigger.transitions[6].inkJSON, "NPC", trigger.transitions[6].speechClip);
                }
                break;
            default:
                break;
        }
    }
    public void OnDialogueExit(int choiceID, InkDialogueTrigger trigger, string npcName, InkDialogueManager.DialogueMode dialogueMode)
    {
        string dialogueTriggerName = "";
        if(dialogueMode == InkDialogueManager.DialogueMode.Trigger)
        {
            dialogueTriggerName = $"{trigger.npcName}/{trigger.triggerName.Replace(" ", string.Empty)}";
        }
        else
        {
            dialogueTriggerName = $"{npcName}/{npcName.Replace(" ", string.Empty)}";
        }
        if (dialogueTriggerName == "NPC/Page1Success")
        {
            System.Random random = new();
            int itemID = random.Next(1, 9);
            InventoryManager.Add(itemID, 1);
        }
        else if (dialogueTriggerName == "NPC/Page2Success")
        {
            System.Random random = new();
            int itemID = random.Next(192001, 192005);
            InventoryManager.Add(itemID, 1);
        }
        else if (dialogueTriggerName == "NPC/Page3Success")
        {
            System.Random random = new();
            int itemID = random.Next(12001, 12005);
            InventoryManager.Add(itemID, 1);
        }
    }
}