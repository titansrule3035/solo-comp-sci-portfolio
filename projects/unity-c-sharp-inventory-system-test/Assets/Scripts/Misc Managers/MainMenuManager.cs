using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public class MainMenuManager : MonoBehaviour
{
    public int menuIndex;
    public GameObject selector;
    public int selectorMinValue;
    public int selectorMaxValue;
    public int selectorValue;
    public int? selectedValue;
    public GameObject mainMenu;
    public GameObject[] mainMenuOptions;
    public GameObject saveMenu;
    public GameObject[] saveMenuOptions;
    public bool omitLoad;
    public Vector3 spoofPlayerPos = new(-2.647947f, -1.028628f, 0);
    public int[] spoofPlayerInv;
    private void Start()
    {
        menuIndex = 0;
        if (SaveManager.HasSaveData())
        {
            omitLoad = false;
        }
        else
        {
            omitLoad = true;
        }
        SaveManager.NormalizeDir();
        selectorMinValue = 0;
        selectorMaxValue = 2;
        selectorValue = selectorMinValue;
        spoofPlayerInv = new int[19];
        for (int i = 0; i < spoofPlayerInv.Length; i++)
        {
            spoofPlayerInv[i] = 0;
        }
        Time.timeScale = 1;
    }
    private void Update()
    {
        GameManager._inventoryOpened = false;
        if (menuIndex == 0)
        {
            mainMenu.SetActive(true);
            saveMenu.SetActive(false);
            selectorMinValue = 0;
            selectorMaxValue = 2;
            if (selectorValue == 0)
            {
                selector.transform.position = mainMenuOptions[0].transform.position;
            }
            if (selectorValue == 1)
            {
                selector.transform.position = mainMenuOptions[1].transform.position;
            }
            if (selectorValue == 2)
            {
                selector.transform.position = mainMenuOptions[2].transform.position;
            }
        }
        if (menuIndex == 1)
        {
            mainMenu.SetActive(false);
            saveMenu.SetActive(true);
            if (omitLoad)
            {
                saveMenuOptions[1].GetComponent<UnityEngine.UI.Image>().color = new Color32(210, 210, 210, 100);
                selectorMinValue = 0;
                selectorMaxValue = 1;
                if (selectorValue == 0)
                {
                    selector.transform.position = saveMenuOptions[0].transform.position;
                }
                if (selectorValue == 1)
                {
                    selector.transform.position = saveMenuOptions[2].transform.position;
                }
            }
            else
            {

                saveMenuOptions[1].GetComponent<UnityEngine.UI.Image>().color = new Color32(243, 243, 243, 255);
                selectorMinValue = 0;
                selectorMaxValue = 2;
                if (selectorValue == 0)
                {
                    selector.transform.position = saveMenuOptions[0].transform.position;
                }
                if (selectorValue == 1)
                {
                    selector.transform.position = saveMenuOptions[1].transform.position;
                }
                if (selectorValue == 2)
                {
                    selector.transform.position = saveMenuOptions[2].transform.position;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectorValue--;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectorValue++;
        }
        if (selectorValue < selectorMinValue)
        {
            selectorValue = selectorMaxValue;
        }
        if (selectorValue > selectorMaxValue)
        {
            selectorValue = selectorMinValue;
        }
        if (DebugManager.debug)
        {
            mainMenuOptions[1].GetComponent<UnityEngine.UI.Image>().color = new Color32(0, 255, 0, 255);
        }
        else
        {
            mainMenuOptions[1].GetComponent<UnityEngine.UI.Image>().color = new Color32(255, 0, 0, 255);
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            selectedValue = selectorValue;
            if (menuIndex == 0)
            {
                if (selectedValue == 0)
                {
                    menuIndex = 1;
                    selectedValue = 0;
                    selectorValue = selectorMinValue;
                }
                else if (selectedValue == 1)
                {
                    if (DebugManager.debug)
                    {
                        DebugManager.debug = false;
                    }
                    else
                    {
                        DebugManager.debug = true;
                    }
                }
                else if (selectedValue == 2)
                {
                    Application.Quit();
                    selectedValue = 0;
                    selectorValue = selectorMinValue;
                }
            }
            else if (menuIndex == 1)
            {
                if (omitLoad)
                {
                    if (selectedValue == 0)
                    {
                        GameManager.newGame = true;
                        SceneManager.LoadScene(1);
                        selectedValue = 0;
                        selectorValue = selectorMinValue;
                    }
                    else if (selectedValue == 1)
                    {
                        menuIndex = 0;
                        selectedValue = 0;
                        selectorValue = selectorMinValue;
                    }
                }
                else
                {
                    if (selectedValue == 0)
                    {
                        GameManager.newGame = true;
                        SceneManager.LoadScene(1);
                        selectedValue = 0;
                        selectorValue = selectorMinValue;
                    }
                    else if (selectedValue == 1)
                    {
                        SceneManager.LoadScene(1);
                        selectedValue = 0;
                        selectorValue = selectorMinValue;
                    }
                    else if (selectedValue == 2)
                    {
                        menuIndex = 0;
                        selectedValue = 0;
                        selectorValue = selectorMinValue;
                    }
                }
            }
        }
    }
}