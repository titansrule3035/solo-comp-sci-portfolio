using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#region Editor
#if UNITY_EDITOR
[CustomEditor(typeof(InventoryPage)), CanEditMultipleObjects]
public class InventoryPageEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        InventoryPage inventoryPage = (InventoryPage)target;
        SerializedProperty serializedInventoryMenu = null;
        SerializedProperty serializedInventoryMenuOpened = null;
        SerializedProperty serializedSelector = null;
        SerializedProperty serializedstatTextObjects = null;
        if (inventoryPage.pageType != InventoryPage.PageTypes.stats)
        {
            serializedInventoryMenu = serializedObject.FindProperty("inventoryMenu");
            serializedInventoryMenuOpened = serializedObject.FindProperty("inventoryMenuOpened");
            serializedSelector = serializedObject.FindProperty("selector");
            EditorGUILayout.PropertyField(serializedInventoryMenu, new GUIContent("Inventory Menu"));
            serializedInventoryMenuOpened.boolValue = EditorGUILayout.Toggle("Inventory Menu Opened", serializedInventoryMenuOpened.boolValue);
            serializedObject.Update();
            DrawInventory();
        }
        if (inventoryPage.pageType == InventoryPage.PageTypes.weapons)
        {
            DrawEquippedWeaponsGrids();
        }
        if (inventoryPage.pageType == InventoryPage.PageTypes.armor)
        {
            inventoryPage.equippedArmorGrid = EditorGUILayout.IntField("Equipped Armor", inventoryPage.equippedArmorGrid);
        }
        if (inventoryPage.pageType != InventoryPage.PageTypes.stats)
        {
            EditorGUILayout.PropertyField(serializedSelector, new GUIContent("Selector"));
            inventoryPage.maxSelectorXIndex = EditorGUILayout.IntField("Maximum Selector X Value", inventoryPage.maxSelectorXIndex);
            inventoryPage.maxSelectorYIndex = EditorGUILayout.IntField("Maximum Selector Y Value", inventoryPage.maxSelectorYIndex);
            EditorGUILayout.Space();
            SerializedProperty selectorPos = serializedObject.FindProperty("selectorPos");
            serializedObject.Update();
            EditorGUILayout.PropertyField(selectorPos, new GUIContent("Selector Position"), true);
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Space();
            inventoryPage.selectedGrid = EditorGUILayout.IntField("Highlighted Selector Grid", inventoryPage.selectedGrid);
            serializedObject.Update();
        }
        else
        {
            serializedstatTextObjects = serializedObject.FindProperty("statTextObjects");
            DrawStatsTextObjects();
        }
    }
    private void DrawStatsTextObjects()
    {
        SerializedProperty statTextObjects = serializedObject.FindProperty("statTextObjects");
        EditorGUILayout.Space();
        serializedObject.Update();
        EditorGUILayout.PropertyField(statTextObjects, new GUIContent("Stats Text Objects"), true);
        serializedObject.ApplyModifiedProperties();
    }
    private void DrawInventory()
    {
        EditorGUILayout.Space();
        SerializedProperty inventory = serializedObject.FindProperty("inventory");
        serializedObject.Update();
        EditorGUILayout.PropertyField(inventory, new GUIContent("Inventory Contents"), true);
        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.Space();
        SerializedProperty itemAmounts = serializedObject.FindProperty("itemAmounts");
        serializedObject.Update();
        EditorGUILayout.PropertyField(itemAmounts, new GUIContent("Item Amounts"), true);
        serializedObject.ApplyModifiedProperties();
    }
    private void DrawEquippedWeaponsGrids()
    {
        EditorGUILayout.Space();
        SerializedProperty equippedWeaponGrids = serializedObject.FindProperty("equippedWeaponGrids");
        serializedObject.Update();
        EditorGUILayout.PropertyField(equippedWeaponGrids, new GUIContent("Equipped Weapon Grids"), true);
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
#endregion
