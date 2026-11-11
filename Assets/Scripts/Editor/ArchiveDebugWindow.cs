using System;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class ArchiveDebugWindow : OdinEditorWindow
{
    private ArchiveData_1_4_0 archiveData;
    private string status;
    private string archivePath;
    [ShowInInspector, InlineProperty, HideLabel, TabGroup("存档", "全局数据"), PropertyOrder(0)]
    private GlobalData_1_4_0 globalData;
    [ShowInInspector, InlineProperty, HideLabel, TabGroup("存档", "临时数据"), PropertyOrder(1)]
    private TemporaryData_1_4_0 temporaryData;
    [ShowInInspector, InlineProperty, HideLabel, TabGroup("存档", "战斗续档"), PropertyOrder(2)]
    private BattleArchiveData_1_4_0 battleData;
    [ShowInInspector, InlineProperty, HideLabel, TabGroup("存档", "统计数据"), PropertyOrder(3)]
    private StatisticsData_1_4_0 statisticsData;
    [ShowInInspector, InlineProperty, HideLabel, TabGroup("存档", "设置数据"), PropertyOrder(4)]
    private SettingArchiveData_1_4_0 settingArchiveData;

    [MenuItem("GameTools/存档调试工具")]
    private static void OpenWindow()
    {
        GetWindow<ArchiveDebugWindow>("存档调试").Show();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        archivePath = Path.Combine(Application.persistentDataPath, ArchiveManager.ArchiveName);
        InitializeDataForPreview();
        LoadArchive();
    }

    [ShowInInspector, ReadOnly, LabelText("存档路径"), PropertyOrder(-20)]
    private string ArchivePath => archivePath;

    [ShowInInspector, ReadOnly, LabelText("状态"), PropertyOrder(-19)]
    private string Status => status;

    [Button("重新读取存档"), PropertyOrder(-18)]
    private void LoadArchive()
    {
        archivePath = Path.Combine(Application.persistentDataPath, ArchiveManager.ArchiveName);
        if (!File.Exists(archivePath))
        {
            archiveData = null;
            status = "未找到存档文件";
            return;
        }

        try
        {
            archiveData = SaveManager.Inst.LoadArchive<ArchiveData_1_4_0>(ArchiveManager.ArchiveName);
            BindArchiveSections();
            status = archiveData == null ? "存档内容为空或无法识别" : "已读取存档，可在下方页面编辑";
        }
        catch (Exception exception)
        {
            archiveData = null;
            status = "读取失败: " + exception.Message;
            Debug.LogException(exception);
        }
    }

    [Button("应用并重新进入游戏"), GUIColor(0.35f, 0.8f, 0.45f), PropertyOrder(10)]
    private void ApplyAndReenterGame()
    {
        if (archiveData == null)
        {
            status = "没有可应用的存档数据";
            return;
        }

        try
        {
            archiveData.GlobalData = globalData;
            archiveData.TemporaryData = temporaryData;
            archiveData.BattleData = battleData;
            archiveData.StatisticsData = statisticsData;
            archiveData.SettingArchiveData = settingArchiveData;
            SaveManager.Inst.SaveArchive(ArchiveManager.ArchiveName, archiveData);
            if (EditorApplication.isPlaying)
            {
                ArchiveManager.Inst.ReloadArchiveForEditor();
                if (ProcedureManager.Inst == null)
                {
                    status = "存档已写入；游戏流程尚未初始化，请稍后再次应用";
                    return;
                }

                ProcedureManager.Inst.RestartProcedureForEditor(new MainSceneProcedure());
                LoadArchive();
                status = "已重新读取存档并重新进入游戏";
                return;
            }

            status = "存档已写入，正在启动游戏";
            EditorApplication.isPlaying = true;
        }
        catch (Exception exception)
        {
            status = "应用失败: " + exception.Message;
            Debug.LogException(exception);
        }
    }

    private void BindArchiveSections()
    {
        globalData = archiveData?.GlobalData ?? new GlobalData_1_4_0();
        temporaryData = archiveData?.TemporaryData ?? new TemporaryData_1_4_0();
        battleData = archiveData?.BattleData ?? new BattleArchiveData_1_4_0();
        statisticsData = archiveData?.StatisticsData ?? new StatisticsData_1_4_0();
        settingArchiveData = archiveData?.SettingArchiveData ?? new SettingArchiveData_1_4_0();
    }

    private void InitializeDataForPreview()
    {
        try
        {
            if (!EditorApplication.isPlaying)
            {
                ResourcesManager.Inst.EditorMode = true;
                ResourcesManager.Inst.Init();
                DataManager.Inst.Init();
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("初始化 ID 预览数据失败: " + exception.Message);
        }
    }

}

internal sealed class ArchiveIdPreviewDrawer : OdinValueDrawer<int>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        ArchiveIdKind kind = GetIdKind(this.Property.Path);
        if (kind == ArchiveIdKind.None)
        {
            this.CallNextDrawer(label);
            return;
        }

        Rect row = EditorGUILayout.GetControlRect();
        float idWidth = Mathf.Min(Mathf.Max(EditorGUIUtility.labelWidth + 55f, row.width * 0.42f), row.width * 0.58f);
        float nameWidth = Mathf.Min(150f, Mathf.Max(85f, row.width * 0.25f));
        Rect idRect = new Rect(row.x, row.y, idWidth, row.height);
        Rect nameRect = new Rect(idRect.xMax + 4f, row.y, nameWidth, row.height);
        Rect objectRect = new Rect(nameRect.xMax + 4f, row.y, Mathf.Max(0f, row.xMax - nameRect.xMax - 4f), row.height);

        EditorGUI.BeginChangeCheck();
        int id = EditorGUI.IntField(idRect, label, this.ValueEntry.SmartValue);
        if (EditorGUI.EndChangeCheck())
            this.ValueEntry.SmartValue = id;

        ResolvePreview(kind, id, out string displayName, out UnityEngine.Object preview);
        GUIContent nameContent = new GUIContent(displayName, "由当前 ID 对应的游戏数据解析，仅供预览。");
        EditorGUI.LabelField(nameRect, nameContent, EditorStyles.miniLabel);
        using (new EditorGUI.DisabledScope(true))
            EditorGUI.ObjectField(objectRect, preview, typeof(UnityEngine.Object), false);
    }

    private static ArchiveIdKind GetIdKind(string path)
    {
        if (string.IsNullOrEmpty(path))
            return ArchiveIdKind.None;

        if (path.Contains("KillEnemys") || path.Contains("ReceiveEnemyIllustrated"))
            return IsDictionaryValue(path) ? ArchiveIdKind.None : ArchiveIdKind.Enemy;
        if (path.Contains("ReceiveHeroSkill"))
            return path.Contains(".Key") && CountOccurrences(path, ".Key") == 1
                ? ArchiveIdKind.Hero
                : ArchiveIdKind.None;
        if (path.Contains("UnLockHeros") || path.Contains("LastSelectHeroID") ||
            path.Contains("HeroUpgradeDatas") || path.Contains("HeroId"))
            return IsDictionaryValue(path) && !path.Contains("HeroId") ? ArchiveIdKind.None : ArchiveIdKind.Hero;
        if (path.Contains("UnLockItems") || path.Contains("UnlockItems") || path.Contains("UnlockedItems") ||
            path.Contains("GetItemList") || path.Contains("ReceiveItemIllustrated") ||
            path.Contains("UnlockRuneItem") || path.Contains("PlayerData.Items"))
            return IsDictionaryValue(path) ? ArchiveIdKind.None : ArchiveIdKind.Item;
        if (path.Contains("ChapterClearanceDatas") || path.Contains("LastSelectChapterId") ||
            path.Contains("ReceiveChapterFreeSoul") || path.Contains("CurrentCPId"))
            return IsDictionaryValue(path) ? ArchiveIdKind.None : ArchiveIdKind.Chapter;
        if (path.Contains("UnlockItemSchool") || path.Contains("ReceiveItemSchoolIllustrated"))
            return ArchiveIdKind.ItemSchool;
        if (path.Contains("MissionCompletedIdList"))
            return ArchiveIdKind.Mission;

        return ArchiveIdKind.None;
    }

    private static bool IsDictionaryValue(string path)
    {
        return path.Contains(".Value") || path.EndsWith("Value", StringComparison.Ordinal);
    }

    private static int CountOccurrences(string value, string match)
    {
        int count = 0;
        int index = 0;
        while ((index = value.IndexOf(match, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += match.Length;
        }
        return count;
    }

    private static void ResolvePreview(ArchiveIdKind kind, int id, out string name, out UnityEngine.Object preview)
    {
        name = id <= 0 ? "" : "未找到配置";
        preview = null;
        if (id <= 0)
            return;

        DataManager data = DataManager.Inst;
        switch (kind)
        {
            case ArchiveIdKind.Hero:
                if (data.HeroDatas != null && data.HeroDatas.TryGetValue(id, out HeroData hero))
                {
                    name = hero.HeroName;
                    preview = AssetDatabase.LoadAssetAtPath<GameObject>(
                        "Assets/AssetsPackage/PlayerPrefabs/" + hero.HeroPrefabName + ".prefab");
                }
                break;
            case ArchiveIdKind.Item:
                if (data.AllItemObj != null && data.AllItemObj.TryGetValue(id, out ItemScriptableObject item))
                {
                    name = item.Name;
                    preview = item;
                }
                break;
            case ArchiveIdKind.Chapter:
                if (data.ChapterTableDatas != null && data.ChapterTableDatas.TryGetValue(id, out ChapterTabData chapter))
                {
                    name = chapter.ChapterName;
                    string chapterPath = "Assets/AssetsPackage/ChapterDatas/" + chapter.ChapterDataPath + ".asset";
                    preview = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(chapterPath);
                }
                break;
            case ArchiveIdKind.Enemy:
                if (data.EnemyDatas != null && data.EnemyDatas.TryGetValue(id, out EnemyInfoData enemy))
                {
                    name = enemy.EnemyName;
                    preview = AssetDatabase.LoadAssetAtPath<GameObject>(
                        "Assets/AssetsPackage/EnemyPrefabs/" + enemy.EnemyPrefabName + ".prefab");
                }
                break;
            case ArchiveIdKind.ItemSchool:
                if (data.ItemSchool != null && data.ItemSchool.TryGetValue(id, out ItemSchoolData school))
                    name = school.Name;
                break;
            case ArchiveIdKind.Mission:
                if (data.MissionTabDatas != null && data.MissionTabDatas.TryGetValue(id, out MissionTabData mission))
                    name = mission.Name;
                break;
        }
    }

    private enum ArchiveIdKind
    {
        None,
        Hero,
        Item,
        Chapter,
        Enemy,
        ItemSchool,
        Mission
    }
}
