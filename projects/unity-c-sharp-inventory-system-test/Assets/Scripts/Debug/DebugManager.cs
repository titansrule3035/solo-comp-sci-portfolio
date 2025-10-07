using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance { get; private set; }

    public static bool debug = false;
    public bool debugConsoleActive = false;
    public GameObject debugConsole;
    public TextMeshProUGUI outputText;
    public TMP_InputField debugConsoleInput;

    public List<string> debugMessages = new();

    // Commands
    public static DebugCommand<int, int> SET;
    public static DebugCommand<int, int, int> SET_AT;
    public static DebugCommand<int, int> ADD;
    public static DebugCommand<int, int> FLOOD;
    public static DebugCommand<int, int> REMOVE;
    public static DebugCommand<int> REMOVE_ALL;
    public static DebugCommand<int, int, int> REMOVE_AT;
    public static DebugCommand<int, int> DROP;
    public static DebugCommand CLEAR_INV;
    public static DebugCommand DELETE_STRAY_ITEMS;
    public static DebugCommand EXIT_DIALOGUE;
    public static DebugCommand<string> LOG_WORD;
    public static DebugCommand HELP;
    public static DebugCommand CLEAR_CONSOLE;

    public static bool typing = false;
    public List<object> commandList;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        debugConsoleInput.text = "";
        outputText.text = "";

        // Command registration
        SET = new DebugCommand<int, int>(".set", "Sets all items in inventory to the given item ID.", ".set <itemID> <amount>", (x, y) =>
        {
            InventoryManager.Set(x, y);
        });
        SET_AT = new DebugCommand<int, int, int>(".set_at", "Sets the given index in the inventory to the given item ID.", ".set_at <index> <itemID> <amount>", (i, x, y) =>
        {
            InventoryManager.SetIndex(i, x, y);
        });
        ADD = new DebugCommand<int, int>(".add", "Adds the given item ID into the next available inventory slot.", ".add <itemID> <amount>", (x, y) =>
        {
            InventoryManager.Add(x, y);
        });
        FLOOD = new DebugCommand<int, int>(".flood", "Floods the given item ID into all available inventory slots.", ".flood <itemID> <amount>", (x, y) =>
        {
            InventoryManager.Flood(x, y);
        });
        REMOVE = new DebugCommand<int, int>(".remove", "Removes the given item ID from the inventory starting at the end.", ".remove <itemID> <amount>", (x, y) =>
        {
            InventoryManager.Remove(x, y);
        });
        REMOVE_ALL = new DebugCommand<int>(".remove_all", "Removes all instances of the given item ID from the inventory.", ".remove_all <itemID>", (x) =>
        {
            InventoryManager.RemoveAll(x);
        });
        REMOVE_AT = new DebugCommand<int, int, int>(".remove_at", "Removes the given item ID from the inventory using the given index.", ".remove_at <index> <itemID> <amount>", (i, x, y) =>
        {
            InventoryManager.RemoveAtIndex(i, x, y);
        });
        DROP = new DebugCommand<int, int>(".drop", "Drops whatever item is in the given index, assuming it is not empty.", ".drop <index> <inventoryPageIndex>", (i, d) =>
        {
            InventoryPage.PageTypes p = InventoryManager.GetDestinationFromIndex(d);
            InventoryManager.DropItem(i, p);
        });
        CLEAR_INV = new DebugCommand(".clear_inv", "Clears all items from the player's inventory.", ".clear_inv", () =>
        {
            InventoryManager.FlushInventories(InventoryManager.inventoryPages);
        });
        DELETE_STRAY_ITEMS = new DebugCommand(".delete_stray_items", "Deletes all items from the scene that are not in the inventory.", ".delete_stray_items", () =>
        {
            Item[] items = FindObjectsByType<Item>(FindObjectsSortMode.None);
            if (items != null && items.Length > 0)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    Destroy(items[i].gameObject);
                }
                Log("<Debugger>: Deleted all stray items from scene.");
            }
            else
            {
                Log("<Debugger>: No stray items to be deleted from scene.");
            }
        });
        EXIT_DIALOGUE = new DebugCommand(".exit_dialogue", "Closes the dialogue box and exits dialogue mode.", ".exit_dialogue", () =>
        {
            FindFirstObjectByType<InkDialogueManager>().ExitDialogueMode();
        });
        LOG_WORD = new DebugCommand<string>(".log_word", "Logs a single word into the console.", ".log_word <word>", (s) =>
        {
            Log("<Debugger>: Logged word: " + s + ".");
        });
        HELP = new DebugCommand(".help", "Displays all commands in the console.", ".help", () =>
        {
            List<object> reversedCommandList = GetReversedOrder(commandList);
            foreach (var cmd in reversedCommandList)
            {
                if (cmd is DebugCommandBase command)
                {
                    PlainLog($" - {command.CommandFormat} - {command.CommandDescription}");
                }
            }
            PlainLog("<Debugger>: Commands:");
        });
        CLEAR_CONSOLE = new DebugCommand(".clear_console", "Clears the debug console and wipes the logging file.", ".clear_console", () =>
        {
            debugMessages = new();
            using (StreamWriter writer = new(SaveManager.logFilePath))
            {
                writer.WriteLine("");
            }
            outputText.text = "";
        });

        commandList = new List<object> {
            SET, SET_AT, ADD, FLOOD, REMOVE, REMOVE_ALL, REMOVE_AT, DROP,
            CLEAR_INV, DELETE_STRAY_ITEMS, EXIT_DIALOGUE, LOG_WORD, HELP, CLEAR_CONSOLE
        };

        // Load existing log
        List<string> readMessages = new();
        if (!File.Exists(SaveManager.logFilePath))
        {
            Directory.CreateDirectory($"{SaveManager.centralFolderPath}");
            File.Create(SaveManager.logFilePath).Close();
        }
        using (StreamReader reader = File.OpenText(SaveManager.logFilePath))
        {
            string message;
            while ((message = reader.ReadLine()) != null)
            {
                readMessages.Add(message);
            }
        }
        if (readMessages.Count > 100)
        {
            readMessages.RemoveAt(0);
        }
        debugMessages = readMessages;
        outputText.text = string.Join("\n", debugMessages.AsEnumerable().Reverse());
    }

    private void Update()
    {
        if (debug)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleDebugConsole();
            }
        }
        else
        {
            debugConsoleActive = false;
        }

        debugConsole.SetActive(debugConsoleActive);

        if (debugConsoleActive && Input.GetKeyDown(KeyCode.Return))
        {
            if (SceneManager.GetActiveScene().buildIndex != 0)
            {
                ProcessCommand();
                debugConsoleInput.text = "";
            }
            else
            {
                Log("<Debug Manager>: Debug commands not supported in main menu, enter a level first.");
                debugConsoleInput.text = "";
            }
        }

        typing = !string.IsNullOrEmpty(debugConsoleInput.text);
    }

    public static void ToggleDebug()
    {
        debug = !debug;
    }

    public void ToggleDebugConsole()
    {
        debugConsoleActive = !debugConsoleActive;
    }

    private void ProcessCommand()
    {
        string input = debugConsoleInput.text.Trim();
        if (string.IsNullOrEmpty(input))
            return;

        string[] properties = input.Split(' ');
        string commandID = properties[0];

        foreach (var obj in commandList)
        {
            if (obj is DebugCommandBase debugCommandBase && debugCommandBase.CommandID == commandID)
            {
                try
                {
                    switch (obj)
                    {
                        case DebugCommand cmd:
                            cmd.Invoke();
                            break;
                        case DebugCommand<int> cmd1:
                            cmd1.Invoke(int.Parse(properties[1]));
                            break;
                        case DebugCommand<int, int> cmd2:
                            cmd2.Invoke(int.Parse(properties[1]), int.Parse(properties[2]));
                            break;
                        case DebugCommand<int, int, int> cmd3:
                            cmd3.Invoke(int.Parse(properties[1]), int.Parse(properties[2]), int.Parse(properties[3]));
                            break;
                        case DebugCommand<string> cmdStr:
                            cmdStr.Invoke(properties[1]);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Log($"<Debugger>: Error processing command '{input}' - {e.Message}");
                }
            }
        }
    }

    public void CollectDebugMessage(string input)
    {
        if (!File.Exists(SaveManager.logFilePath))
        {
            Directory.CreateDirectory($"{SaveManager.centralFolderPath}");
            File.Create(SaveManager.logFilePath).Close();
        }

        string[] readMessages = File.ReadAllLines(SaveManager.logFilePath);
        debugMessages = readMessages.ToList();
        debugMessages.Add(input);

        using (StreamWriter writer = File.AppendText(SaveManager.logFilePath))
        {
            writer.WriteLine(input);
        }

        outputText.text = string.Join("\n", debugMessages.AsEnumerable().Reverse());
    }

    private List<T> GetReversedOrder<T>(List<T> inputList)
    {
        List<T> reversedList = new(inputList.Count);
        for (int i = inputList.Count - 1; i >= 0; i--)
        {
            reversedList.Add(inputList[i]);
        }
        return reversedList;
    }

    public static string GetCurrentDateTimeFormatted()
    {
        DateTime now = DateTime.Now;
        return now.ToString("dd/MM/yyyy - HH:mm:ss");
    }

    public static void Log(object input)
    {
        Instance?.CollectDebugMessage($" {GetCurrentDateTimeFormatted()}: {input}");
    }

    public static void PlainLog(object input)
    {
        Instance?.CollectDebugMessage($" {input}");
    }
}
