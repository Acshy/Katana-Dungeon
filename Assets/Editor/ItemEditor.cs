using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class ItemEditor : OdinMenuEditorWindow
{
    [MenuItem("Tools/Item Data")]
    private static void OpenWindows()
    {
        GetWindow<ItemEditor>().Show();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_createNewItemData != null)
        {
            DestroyImmediate(_createNewItemData.itemData);
        }
    }

    private CreateNewItemData _createNewItemData;
    //private UpdateItemID _updateItemID;
    private OdinMenuTree tree;
    int index;
    protected override OdinMenuTree BuildMenuTree()
    {
        string path = "Assets/AssetsPackage/ScriptObject_Item/ItemObject";
        tree = new OdinMenuTree();
        _createNewItemData = new CreateNewItemData();
        tree.Add("Create New", _createNewItemData);

        tree.AddAllAssetsAtPath("ALL", path, typeof(ItemScriptableObject), true);
        tree.SortMenuItemsByName();

        //_updateItemID = new UpdateItemID();
        //tree.Add("Update ID", _updateItemID);


        index = 0;
        //tree.MenuItems.ForEach(UpdateID);

        return tree;
    }

    /*
    void UpdateID(OdinMenuItem menuItem)
    {
        if (menuItem.Value is ItemScriptableObject)
        {
            ItemScriptableObject item = menuItem.Value as ItemScriptableObject;
            //Debug.Log((menuItem.Value as ItemScriptableObject).Name);
            // item.ID = index;
            EditorUtility.SetDirty(item);
            index++;
        }
        else
        {
            foreach (var childMenuItem in menuItem.ChildMenuItems)
            {
                UpdateID(childMenuItem);
            }
        }

    }
    */
    protected override void OnBeginDrawEditors()
    {
        OdinMenuTreeSelection selected = this.MenuTree.Selection;

        SirenixEditorGUI.BeginHorizontalToolbar();
        {
            GUILayout.FlexibleSpace();
            if (SirenixEditorGUI.ToolbarButton("Delete Current"))
            {
                ItemScriptableObject item = selected.SelectedValue as ItemScriptableObject;
                string path = AssetDatabase.GetAssetPath(item);
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.SaveAssets();
            }
        }
        SirenixEditorGUI.EndHorizontalToolbar();
    }


    public class CreateNewItemData
    {
        public CreateNewItemData()
        {
            NewData();
        }

        void NewData()
        {
            itemData = ScriptableObject.CreateInstance<ItemScriptableObject>();
            itemData.Name = "New Item";
        }

        [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        public ItemScriptableObject itemData;

        [Button("Add New Item")]
        private void CreateNewItem()
        {
            string path = "Assets/BundleAssets/ScriptObject_Item/ItemObject/New/" + itemData.Name + ".asset";
            AssetDatabase.CreateAsset(itemData, path);
            AssetDatabase.SaveAssets();

            NewData();
        }
    }

    /*
        public class UpdateItemID
        {
            public UpdateItemID()
            {

            }
            [Button("Update!")]
            private void UpdateID()
            {
                Debug.LogError("??");
            }
        }
        */
}
