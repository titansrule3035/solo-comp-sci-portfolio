using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static readonly string centralFolderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Inventory System Test Version 2\\";
    private static readonly string path = centralFolderPath + "playerInfo.dat";
    public static string logFilePath = centralFolderPath + "console.log";

    private static string[] data;
    private static Vector3 loadedPos;
    private static int[] loadedInv;
    private static int[] loadedAmounts;

    private static Vector3 currentPos;
    private static int equippedWeaponGrids;
    private static int equippedArmorGrid;
    private static InventoryPageFrame[] inventoryPageFrames;


    public string[] readableData;

    public static void SaveGame(InventoryPage[] inventoryPages)
    {
        GameObject player = FindObjectOfType<Player>().gameObject;
        int equippedArmorGrid = 1111111;
        for (int i = 0; i < inventoryPages.Length; i++)
        {
            if (inventoryPages[i].pageType == InventoryPage.PageTypes.armor)
            {
                equippedArmorGrid = inventoryPages[i].equippedArmorGrid;
            }
        }
        int[] equippedWeaponGrids = new int[5];
        for (int i = 0; i < 5; i++)
        {
            equippedWeaponGrids[i] = 1111111;
        }
        for (int i = 0; i < inventoryPages.Length; i++)
        {
            if (inventoryPages[i].pageType == InventoryPage.PageTypes.weapons)
            {
                equippedWeaponGrids = inventoryPages[i].equippedWeaponGrids;
            }
        }
        int[] massInventory = InventoryManager.GetMassInventory(inventoryPages);
        int[] massItemAmount = InventoryManager.GetMassItemAmount(inventoryPages);
        DebugManager.Log("<Save Manager>: Writing to save file...");
        using (StreamWriter writer = new(path, false))
        {
            writer.WriteLine($"{Encode(player.transform.position.x)}\n{Encode(player.transform.position.y)}\n{Encode(player.transform.position.z)}\n{Encode(equippedArmorGrid)}");
            for (int i = 0; i < equippedWeaponGrids.Length; i++)
            {
                writer.WriteLine($"{Encode(equippedWeaponGrids[i])}");
            }
            writer.Close();
        }
        using (StreamWriter writer = new(path, true))
        {
            for (int i = 0; i < massInventory.Length; i++)
            {
                writer.WriteLine($"{Encode(massInventory[i])}");
            }
            for (int i = 0; i < massItemAmount.Length; i++)
            {
                writer.WriteLine($"{Encode(massItemAmount[i])}");
            }
            writer.Close();
        }
        DebugManager.Log("<Save Manager>: Game successfully saved!");
    }
    public static InventoryPage[] LoadGame(InventoryPage[] inventoryPages)
    {
        NormalizeDir();
        DebugManager.Log("<Save Manager>: Reading data from file...");
        data = File.ReadAllLines(path);
        loadedPos = Vector3.zero;
        int loadedEquippedArmorGrid = 1111111;
        int[] loadedEquippedWeaponGrids = new int[5];
        for (int j = 0; j < 5; j++)
        {
            loadedEquippedWeaponGrids[j] = 1111111;
        }
        loadedInv = new int[InventoryManager.GetMassInventoryLength(inventoryPages)];
        loadedAmounts = new int[InventoryManager.GetMassInventoryLength(inventoryPages)];
        int i = 0;
        loadedPos.x = (float)Convert.ToDouble(Decode(data[i]).Replace("f", string.Empty).Replace(" ", string.Empty));
        i++;
        loadedPos.y = (float)Convert.ToDouble(Decode(data[i]).Replace("f", string.Empty).Replace(" ", string.Empty));
        i++;
        loadedPos.z = (float)Convert.ToDouble(Decode(data[i]).Replace("f", string.Empty).Replace(" ", string.Empty));
        i++;
        loadedEquippedArmorGrid = Convert.ToInt32(Decode(data[i]).Replace("f", string.Empty).Replace(" ", string.Empty));
        i++;
        for (int j = 0; (j < loadedEquippedWeaponGrids.Length); j++)
        {
            loadedEquippedWeaponGrids[j] = Convert.ToInt32((Decode(data[i])));
            i++;
        }
        for (int j = 0; (j < loadedInv.Length); j++)
        {
            loadedInv[j] = Convert.ToInt32((Decode(data[i])));
            i++;
        }
        for (int j = 0; j < loadedAmounts.Length; j++)
        {
            loadedAmounts[j] = Convert.ToInt32((Decode(data[i])));
            i++;
        }
        DebugManager.Log("<Save Manager>: Save data successfully loaded!");
        data = new string[3 + loadedInv.Length];
        if (loadedPos != null && loadedInv == null)
        {
            FindFirstObjectByType<Player>().transform.position = loadedPos;
            DebugManager.Log("<Save Manager>: Loaded position to heap.");
            return null;
        }
        else if (loadedPos == null && loadedInv != null)
        {
            DebugManager.Log("<Save Manager>: Loaded inventory to heap.");
            for (int j = 0; j < inventoryPages.Length; j++)
            {
                if (inventoryPages[j].pageType == InventoryPage.PageTypes.armor)
                {
                    inventoryPages[j].equippedArmorGrid = loadedEquippedArmorGrid;
                }
            }
            for (int j = 0; j < inventoryPages.Length; j++)
            {
                if (inventoryPages[j].pageType == InventoryPage.PageTypes.weapons)
                {
                    inventoryPages[j].equippedWeaponGrids = loadedEquippedWeaponGrids;
                }
            }
            return InventoryManager.SeparateInventory(loadedInv, loadedAmounts, inventoryPages);
        }
        else if (loadedPos != null && loadedInv != null)
        {
            FindFirstObjectByType<Player>().transform.position = loadedPos;
            DebugManager.Log("<Save Manager>: Loaded position and inventory to heap.");
            for (int j = 0; j < inventoryPages.Length; j++)
            {
                if (inventoryPages[j].pageType == InventoryPage.PageTypes.armor)
                {
                    inventoryPages[j].equippedArmorGrid = loadedEquippedArmorGrid;
                }
            }
            for (int j = 0; j < inventoryPages.Length; j++)
            {
                if (inventoryPages[j].pageType == InventoryPage.PageTypes.weapons)
                {
                    inventoryPages[j].equippedWeaponGrids = loadedEquippedWeaponGrids;
                }
            }
            return InventoryManager.SeparateInventory(loadedInv, loadedAmounts, inventoryPages);
        }
        else
        {
            DebugManager.Log("<Save Manager>: Could not load position or inventory to heap.");
            return null;
        }
    }
    private static string Encode(object input)
    {
        string stringedInput = input.ToString();
        int n = 8;
        StringBuilder sb = new();
        foreach (char c in stringedInput.ToCharArray())
        {
            sb.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
        }
        string r1 = sb.ToString();
        string r2 = string.Join(string.Empty, r1.Select((x, i) => i > 0 && i % n == 0 ? string.Format(" {0}", x) : x.ToString()));
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(r2);
        string r3 = System.Convert.ToBase64String(plainTextBytes);
        StringBuilder sb2 = new();
        foreach (char c in r3.ToCharArray())
        {
            sb2.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
        }
        string r4 = sb2.ToString();
        string r5 = string.Join(string.Empty, r4.Select((x, i) => i > 0 && i % n == 0 ? string.Format(" {0}", x) : x.ToString()));
        return r5;
    }
    private static string Decode(string input)
    {
        string r1 = input.Replace(" ", string.Empty);
        List<Byte> byteList = new();
        for (int i = 0; i < r1.Length; i += 8)
        {
            byteList.Add(Convert.ToByte(r1.Substring(i, 8), 2));
        }
        string r2 = Encoding.ASCII.GetString(byteList.ToArray());
        var base64EncodedBytes = System.Convert.FromBase64String(r2);
        string r3 = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        string r4 = r3.Replace(" ", string.Empty);
        List<Byte> byteList2 = new();
        for (int i = 0; i < r4.Length; i += 8)
        {
            byteList2.Add(Convert.ToByte(r4.Substring(i, 8), 2));
        }
        string r5 = Encoding.ASCII.GetString(byteList2.ToArray());
        return r5;
    }
    public static void WipeSaveData()
    {
        using (StreamWriter writer = new(path, false))
        {
            writer.WriteLine($"{Encode(" - 2.647947 ")}\n{Encode(" - 1.028628 ")}\n{Encode(0)}\n{Encode(1111111)}\n{Encode(1111111)}\n{Encode(1111111)}\n{Encode(1111111)}\n{Encode(1111111)}\n{Encode(1111111)}");
            writer.Close();
        }
        using (StreamWriter writer = new(path, true))
        {
            for (int i = 0; i < InventoryManager.GetMassInventoryLength(InventoryManager.inventoryPages); i++)
            {
                writer.WriteLine(Encode(0));
            }
            for (int j = 0; j < InventoryManager.GetMassInventoryLength(InventoryManager.inventoryPages); j++)
            {
                writer.WriteLine(Encode(0));
            }
            writer.Close();
        }
        DebugManager.Log("<Save Manager>: Game successfully saved!");
        InventoryManager.FlushInventories(InventoryManager.inventoryPages);
    }
    public static bool HasSaveData()
    {
        if (File.Exists(path))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static void NormalizeDir()
    {
        if (!Directory.Exists(centralFolderPath))
        {
            DebugManager.Log("<Save Manager>: Save directory does not exist, creating new directory file...");
            Directory.CreateDirectory(centralFolderPath);
            DebugManager.Log("<Save Manager>: Creating new save file...");
            File.Create(path).Close();
            using (StreamWriter writer = new(path))
            {
                DebugManager.Log("<Save Manager>: Save file created! Writing to save file...");
                writer.WriteLine(
                  Encode("-2.647947") + "\n" +
                  Encode("-1.028628") + "\n" +
                  Encode(1111111) + "\n" +
                  Encode(1111111) + "\n" +
                  Encode(1111111) + "\n" +
                  Encode(1111111) + "\n" +
                  Encode(1111111) + "\n" +
                  Encode(1111111) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0)
                );
                writer.Close();
            }
            DebugManager.Log("<Save Manager>: Game successfully saved!");
        }
        if (!File.Exists(path))
        {
            File.Create(path).Close();
            DebugManager.Log("<Save Manager>: Save file does not exist, creating new save file...");
            using (StreamWriter writer = new(path))
            {
                DebugManager.Log("<Save Manager>: Save file created! Writing to save file...");
                writer.WriteLine(
                  Encode("-2.647947") + "\n" +
                  Encode("-1.028628") + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0) + "\n" +
                  Encode(0)
                );
                writer.Close();
            }
            DebugManager.Log("<Save Manager>: Game successfully saved!");
        }
    }

    public static void InitializeProfile()
    {
        inventoryPageFrames = new InventoryPageFrame[InventoryManager.inventoryPages.Length];
        currentPos = new();
        for (int i = 0; i < InventoryManager.inventoryPages.Length; i++)
        {
            inventoryPageFrames[i] = new InventoryPageFrame();
            inventoryPageFrames[i].inventory = InventoryManager.inventoryPages[i].inventory;
        }
    }
    public static void UpdateProfile()
    {

    }

}