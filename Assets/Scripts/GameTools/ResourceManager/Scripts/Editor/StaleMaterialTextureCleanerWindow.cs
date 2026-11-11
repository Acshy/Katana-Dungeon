using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class StaleMaterialTextureCleanerWindow : EditorWindow
{
    private const string TextureEnvironmentsProperty = "m_SavedProperties.m_TexEnvs";

    private readonly List<TextureReference> staleReferences = new List<TextureReference>();
    private readonly List<TextureReference> selectedTextureReferences = new List<TextureReference>();
    private Vector2 staleScroll;
    private Vector2 textureScroll;
    private Texture2D targetTexture;
    private bool staleScanComplete;
    private string status;

    [MenuItem("Tools/AssetBundle/Clean Material Texture References")]
    private static void Open()
    {
        GetWindow<StaleMaterialTextureCleanerWindow>("Material Texture Cleanup");
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "This tool only clears serialized texture references from materials. Changes are applied only after you press a cleanup button and can be undone with Ctrl+Z.",
            MessageType.Info);

        DrawStaleShaderProperties();
        EditorGUILayout.Space(12);
        DrawTextureSearch();

        if (!string.IsNullOrEmpty(status))
            EditorGUILayout.HelpBox(status, MessageType.Info);
    }

    private void DrawStaleShaderProperties()
    {
        EditorGUILayout.LabelField("References to properties missing from the current Shader", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("These are likely obsolete after a Shader change. Review each row before clearing it.", EditorStyles.wordWrappedMiniLabel);

        if (GUILayout.Button("Scan and clean all invalid references", GUILayout.Width(280)))
            ScanAndCleanAll();

        if (GUILayout.Button("Scan all materials for obsolete properties", GUILayout.Width(280)))
            ScanStaleProperties();

        if (!staleScanComplete)
            return;

        if (staleReferences.Count == 0)
        {
            EditorGUILayout.LabelField("No obsolete Shader texture properties with assigned textures were found.");
            return;
        }

        staleScroll = EditorGUILayout.BeginScrollView(staleScroll, GUILayout.MinHeight(110), GUILayout.MaxHeight(240));
        DrawReferenceRows(staleReferences);
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Clear checked obsolete references", GUILayout.Width(270)))
            ClearChecked(staleReferences);
    }

    private void DrawTextureSearch()
    {
        EditorGUILayout.LabelField("References to a selected texture", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Use this when a Shader still exposes a property that the game no longer needs.", EditorStyles.wordWrappedMiniLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            targetTexture = (Texture2D)EditorGUILayout.ObjectField("Texture", targetTexture, typeof(Texture2D), false);
            using (new EditorGUI.DisabledScope(targetTexture == null))
            {
                if (GUILayout.Button("Find", GUILayout.Width(65)))
                    ScanTexture(targetTexture);
            }
        }

        if (targetTexture == null || selectedTextureReferences.Count == 0)
            return;

        EditorGUILayout.LabelField("Found " + selectedTextureReferences.Count + " material reference(s).");
        textureScroll = EditorGUILayout.BeginScrollView(textureScroll, GUILayout.MinHeight(100), GUILayout.MaxHeight(220));
        DrawReferenceRows(selectedTextureReferences);
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Clear checked references to " + targetTexture.name, GUILayout.Width(300)))
            ClearChecked(selectedTextureReferences);
    }

    private void DrawReferenceRows(List<TextureReference> references)
    {
        for (int i = references.Count - 1; i >= 0; i--)
        {
            TextureReference reference = references[i];
            if (reference.Material == null)
            {
                references.RemoveAt(i);
                continue;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                reference.Selected = EditorGUILayout.Toggle(reference.Selected, GUILayout.Width(18));
                if (GUILayout.Button(reference.Material.name, EditorStyles.linkLabel, GUILayout.Width(180)))
                    EditorGUIUtility.PingObject(reference.Material);
                GUILayout.Label(reference.PropertyName, GUILayout.Width(190));
                GUILayout.Label(reference.Texture != null ? reference.Texture.name : "Missing texture", GUILayout.MinWidth(100));
                GUILayout.Label(reference.Material.shader != null ? reference.Material.shader.name : "No Shader", EditorStyles.miniLabel, GUILayout.MinWidth(120));
            }
        }
    }

    private void ScanStaleProperties()
    {
        staleReferences.Clear();
        selectedTextureReferences.Clear();
        staleScanComplete = true;
        status = null;

        foreach (Material material in LoadAllMaterials())
        {
            if (material.shader == null)
                continue;

            HashSet<string> shaderTextureProperties = GetShaderTextureProperties(material.shader);
            foreach (TextureReference reference in GetMaterialTextureReferences(material))
            {
                if (!shaderTextureProperties.Contains(reference.PropertyName))
                    staleReferences.Add(reference);
            }
        }

        status = "Scanned materials. Found " + staleReferences.Count + " assigned references whose property is absent from its current Shader.";
    }

    private void ScanAndCleanAll()
    {
        ScanStaleProperties();
        if (staleReferences.Count == 0)
        {
            EditorUtility.DisplayDialog("Material Cleanup", "No invalid texture references were found.", "OK");
            return;
        }

        bool confirmed = EditorUtility.DisplayDialog(
            "Clean invalid material references",
            "Found " + staleReferences.Count + " assigned texture reference(s) whose properties are not declared by their current Shader. Clear them from all affected materials?",
            "Clean All",
            "Cancel");
        if (confirmed)
            ClearChecked(staleReferences);
    }

    private void ScanTexture(Texture2D texture)
    {
        selectedTextureReferences.Clear();
        status = null;

        foreach (Material material in LoadAllMaterials())
        {
            foreach (TextureReference reference in GetMaterialTextureReferences(material))
            {
                if (reference.Texture == texture)
                    selectedTextureReferences.Add(reference);
            }
        }

        status = "Scanned materials. Found " + selectedTextureReferences.Count + " reference(s) to " + texture.name + ".";
    }

    private static IEnumerable<Material> LoadAllMaterials()
    {
        return AssetDatabase.FindAssets("t:Material", new[] { "Assets" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<Material>)
            .Where(material => material != null);
    }

    private static HashSet<string> GetShaderTextureProperties(Shader shader)
    {
        HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < shader.GetPropertyCount(); i++)
        {
            if (shader.GetPropertyType(i) == ShaderPropertyType.Texture)
                names.Add(shader.GetPropertyName(i));
        }
        return names;
    }

    private static IEnumerable<TextureReference> GetMaterialTextureReferences(Material material)
    {
        SerializedObject serializedMaterial = new SerializedObject(material);
        SerializedProperty textures = serializedMaterial.FindProperty(TextureEnvironmentsProperty);
        if (textures == null || !textures.isArray)
            yield break;

        for (int i = 0; i < textures.arraySize; i++)
        {
            SerializedProperty entry = textures.GetArrayElementAtIndex(i);
            SerializedProperty name = entry.FindPropertyRelative("first");
            SerializedProperty value = entry.FindPropertyRelative("second.m_Texture");
            if (name == null || value == null || value.objectReferenceValue == null)
                continue;

            yield return new TextureReference
            {
                Material = material,
                PropertyName = name.stringValue,
                Texture = value.objectReferenceValue as Texture
            };
        }
    }

    private void ClearChecked(List<TextureReference> references)
    {
        List<TextureReference> selected = references.Where(reference => reference.Selected && reference.Material != null).ToList();
        int cleared = 0;
        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Clean invalid material texture references");
        foreach (TextureReference reference in selected)
        {
            SerializedObject serializedMaterial = new SerializedObject(reference.Material);
            SerializedProperty textures = serializedMaterial.FindProperty(TextureEnvironmentsProperty);
            if (textures == null || !textures.isArray)
                continue;

            bool changed = false;
            for (int i = 0; i < textures.arraySize; i++)
            {
                SerializedProperty entry = textures.GetArrayElementAtIndex(i);
                SerializedProperty name = entry.FindPropertyRelative("first");
                SerializedProperty value = entry.FindPropertyRelative("second.m_Texture");
                if (name == null || value == null || name.stringValue != reference.PropertyName || value.objectReferenceValue != reference.Texture)
                    continue;

                value.objectReferenceValue = null;
                changed = true;
            }

            if (!changed)
                continue;

            Undo.RecordObject(reference.Material, "Clear material texture reference");
            serializedMaterial.ApplyModifiedProperties();
            EditorUtility.SetDirty(reference.Material);
            cleared++;
        }

        Undo.CollapseUndoOperations(undoGroup);
        AssetDatabase.SaveAssets();
        references.RemoveAll(reference => reference.Selected);
        status = "Cleared " + cleared + " material texture reference(s). Changes can be undone with Ctrl+Z.";
    }

    private sealed class TextureReference
    {
        public Material Material;
        public string PropertyName;
        public Texture Texture;
        public bool Selected = true;
    }
}
