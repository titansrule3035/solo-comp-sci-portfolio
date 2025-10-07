using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#region Editor
#if UNITY_EDITOR
[CustomEditor(typeof(Item)), CanEditMultipleObjects]
public class ItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Item item = (Item)target;
        item.defaultTextAssets = new TextAsset[2];
        item.defaultTextAssets[0] = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/Dialogue/Test/DummyDialogue.json", typeof(TextAsset));
        item.defaultTextAssets[1] = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/Dialogue/Test/DummyDialogue.json", typeof(TextAsset));
        if (item.textAssets.Length == 0)
        {
            item.textAssets = item.defaultTextAssets;
        }
        SerializedProperty serializedTextAssets = null;
        SerializedProperty serializedAudioClip = null;
        if (item.destination != InventoryPage.PageTypes.None)
        {
            serializedTextAssets = serializedObject.FindProperty("textAssets");
            serializedAudioClip = serializedObject.FindProperty("itemSpeechClip");
        }
        item.itemName = EditorGUILayout.TextField("Item Name", item.itemName);
        item.itemID = EditorGUILayout.IntField("Item ID Number", item.itemID);
        item.stackAmount = EditorGUILayout.IntField("Maximum Stack Amount", item.stackAmount);
        if (item.destination != InventoryPage.PageTypes.None)
        {
            EditorGUILayout.PropertyField(serializedAudioClip, new GUIContent("Audio Clip"));
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedTextAssets.GetArrayElementAtIndex(0), new GUIContent("Lore Text Asset"));
            serializedObject.ApplyModifiedProperties();
            if (item.destination == InventoryPage.PageTypes.consumables)
            {
                EditorGUILayout.PropertyField(serializedTextAssets.GetArrayElementAtIndex(1), new GUIContent("Use Item Text Asset"));
            }
            if (item.destination != InventoryPage.PageTypes.keyItems && item.destination != InventoryPage.PageTypes.None)
            {
                DrawStats();
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
    private void DrawStats()
    {
        EditorGUILayout.Space();
        SerializedProperty intTags = serializedObject.FindProperty("intTags");
        serializedObject.Update();
        EditorGUILayout.PropertyField(intTags, new GUIContent("Item Stats"), true);
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
#endregion