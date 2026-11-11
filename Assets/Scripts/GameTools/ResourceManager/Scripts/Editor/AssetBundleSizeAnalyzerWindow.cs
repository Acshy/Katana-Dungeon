using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

public sealed class AssetBundleSizeAnalyzerWindow : EditorWindow
{
    private const string BundleDirectory = "Assets/StreamingAssets";
    private const string ConfigPath = "Assets/AssetsPackage/AssetbundleConfigXML.xml";

    private readonly List<BundleInfo> bundles = new List<BundleInfo>();
    private Vector2 bundleScroll;
    private Vector2 assetScroll;
    private string search = string.Empty;
    private string selectedBundle;
    private AssetInfo selectedAsset;
    private string selectedAssetReferences;
    private int view;
    private string status;

    [MenuItem("Tools/AssetBundle/Size Analyzer")]
    private static void Open()
    {
        GetWindow<AssetBundleSizeAnalyzerWindow>("AssetBundle Sizes");
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnGUI()
    {
        DrawToolbar();
        EditorGUILayout.HelpBox(
            "Bundle size is the actual on-disk file size. Source size is the original project file size and is not the packed contribution. Runtime memory must be measured in the Unity Profiler on the target device.",
            MessageType.Info);

        if (!string.IsNullOrEmpty(status))
            EditorGUILayout.HelpBox(status, MessageType.Warning);

        view = GUILayout.Toolbar(view, new[] { "Bundles", "Largest assets" });
        if (view == 0)
        {
            DrawBundleList();
            DrawSelectedBundle();
        }
        else
        {
            DrawAssetList();
            DrawSelectedAsset();
        }
    }

    private void DrawToolbar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
                Refresh();

            GUILayout.Label("Search", GUILayout.Width(45));
            search = GUILayout.TextField(search, GUI.skin.FindStyle("ToolbarSeachTextField") ?? EditorStyles.toolbarTextField);

            if (GUILayout.Button("Export CSV", EditorStyles.toolbarButton, GUILayout.Width(80)))
                ExportCsv();
        }
    }

    private void DrawBundleList()
    {
        EditorGUILayout.LabelField("Bundles", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Label("Bundle", GUILayout.MinWidth(180));
            GUILayout.Label("File", GUILayout.Width(90));
            GUILayout.Label("With direct dependencies", GUILayout.Width(160));
            GUILayout.Label("Assets", GUILayout.Width(55));
            GUILayout.Label("Source files", GUILayout.Width(100));
        }

        bundleScroll = EditorGUILayout.BeginScrollView(bundleScroll, GUILayout.MinHeight(170));
        foreach (BundleInfo bundle in bundles.Where(MatchesSearch))
        {
            GUIStyle style = selectedBundle == bundle.Name ? EditorStyles.toolbarButton : EditorStyles.label;
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(bundle.Name, style, GUILayout.MinWidth(180)))
                    selectedBundle = bundle.Name;
                GUILayout.Label(FormatBytes(bundle.FileBytes), GUILayout.Width(90));
                GUILayout.Label(FormatBytes(bundle.FileBytes + bundle.DirectDependencyBytes), GUILayout.Width(160));
                GUILayout.Label(bundle.Assets.Count.ToString(), GUILayout.Width(55));
                GUILayout.Label(FormatBytes(bundle.SourceBytes), GUILayout.Width(100));
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawSelectedBundle()
    {
        BundleInfo bundle = bundles.FirstOrDefault(item => item.Name == selectedBundle);
        if (bundle == null)
            return;

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Bundle: " + bundle.Name, EditorStyles.boldLabel);
        EditorGUILayout.LabelField("File size: " + FormatBytes(bundle.FileBytes) +
                                   "    Direct dependencies: " + FormatBytes(bundle.DirectDependencyBytes) +
                                   "    Asset source total: " + FormatBytes(bundle.SourceBytes));

        if (bundle.Dependencies.Count > 0)
            EditorGUILayout.LabelField("Dependencies: " + string.Join(", ", bundle.Dependencies.ToArray()));
        else
            EditorGUILayout.LabelField("Dependencies: none in the generated config");

        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Label("Asset path", GUILayout.MinWidth(300));
            GUILayout.Label("Type", GUILayout.Width(90));
            GUILayout.Label("Source size", GUILayout.Width(100));
        }

        assetScroll = EditorGUILayout.BeginScrollView(assetScroll, GUILayout.MinHeight(120));
        foreach (AssetInfo asset in bundle.Assets.OrderByDescending(item => item.SourceBytes))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(asset.Path, EditorStyles.linkLabel, GUILayout.MinWidth(300)))
                    EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(asset.Path));
                GUILayout.Label(asset.Extension, GUILayout.Width(90));
                GUILayout.Label(FormatBytes(asset.SourceBytes), GUILayout.Width(100));
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private bool MatchesSearch(BundleInfo bundle)
    {
        return string.IsNullOrEmpty(search) || bundle.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void DrawAssetList()
    {
        EditorGUILayout.LabelField("Assets sorted by source file size", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Label("Asset", GUILayout.MinWidth(280));
            GUILayout.Label("Source", GUILayout.Width(85));
            GUILayout.Label("Bundle", GUILayout.Width(190));
            GUILayout.Label("Bundle file", GUILayout.Width(90));
        }

        assetScroll = EditorGUILayout.BeginScrollView(assetScroll, GUILayout.MinHeight(220));
        IEnumerable<AssetInfo> assets = bundles.SelectMany(bundle => bundle.Assets)
            .Where(asset => string.IsNullOrEmpty(search) ||
                            asset.Path.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                            asset.BundleName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderByDescending(asset => asset.SourceBytes);
        foreach (AssetInfo asset in assets)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(asset.Path, selectedAsset == asset ? EditorStyles.toolbarButton : EditorStyles.linkLabel, GUILayout.MinWidth(280)))
                {
                    selectedAsset = asset;
                    selectedAssetReferences = null;
                }
                GUILayout.Label(FormatBytes(asset.SourceBytes), GUILayout.Width(85));
                GUILayout.Label(asset.BundleName, GUILayout.Width(190));
                GUILayout.Label(FormatBytes(asset.BundleBytes), GUILayout.Width(90));
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawSelectedAsset()
    {
        if (selectedAsset == null)
            return;

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Asset details", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Bundle: " + selectedAsset.BundleName + "    Bundle file: " + FormatBytes(selectedAsset.BundleBytes));
        EditorGUILayout.LabelField("Source path: " + selectedAsset.Path);
        EditorGUILayout.LabelField("Source file size: " + FormatBytes(selectedAsset.SourceBytes) +
                                   "    Note: this is not the asset's individual compressed size inside the Bundle.");

        if (GUILayout.Button("Find references in bundled assets", GUILayout.Width(240)))
            FindReferences(selectedAsset);

        if (selectedAssetReferences != null)
            EditorGUILayout.HelpBox(selectedAssetReferences, MessageType.None);
    }

    private void FindReferences(AssetInfo target)
    {
        List<string> references = new List<string>();
        foreach (BundleInfo bundle in bundles)
        {
            foreach (AssetInfo candidate in bundle.Assets)
            {
                if (candidate == target || !File.Exists(candidate.Path))
                    continue;

                string[] dependencies = AssetDatabase.GetDependencies(candidate.Path, false);
                if (dependencies.Contains(target.Path, StringComparer.OrdinalIgnoreCase))
                    references.Add(candidate.Path + "  [" + bundle.Name + "]");
            }
        }

        selectedAssetReferences = references.Count == 0
            ? "No direct references were found among the assets listed in the generated bundle config."
            : "Referenced by " + references.Count + " bundled asset(s):\n" + string.Join("\n", references.ToArray());
    }

    private void Refresh()
    {
        bundles.Clear();
        selectedBundle = null;
        status = null;

        if (!Directory.Exists(BundleDirectory))
        {
            status = "Bundle directory not found: " + BundleDirectory;
            return;
        }

        Dictionary<string, BundleInfo> byName = new Dictionary<string, BundleInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (string file in Directory.GetFiles(BundleDirectory, "*", SearchOption.TopDirectoryOnly))
        {
            string name = Path.GetFileName(file);
            if (name.EndsWith(".manifest", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                continue;

            byName[name] = new BundleInfo { Name = name, FileBytes = new FileInfo(file).Length };
        }

        if (File.Exists(ConfigPath))
            ReadConfig(byName);
        else
            status = "Bundle files were found, but the mapping file is missing: " + ConfigPath;

        foreach (BundleInfo bundle in byName.Values)
        {
            bundle.DirectDependencyBytes = bundle.Dependencies
                .Where(byName.ContainsKey)
                .Select(name => byName[name].FileBytes)
                .Distinct()
                .Sum();
            bundles.Add(bundle);
        }

        bundles.Sort((left, right) => right.FileBytes.CompareTo(left.FileBytes));
        if (bundles.Count > 0)
            selectedBundle = bundles[0].Name;
    }

    private void ReadConfig(Dictionary<string, BundleInfo> byName)
    {
        try
        {
            XDocument document = XDocument.Load(ConfigPath);
            foreach (XElement element in document.Descendants("ABList"))
            {
                string bundleName = (string)element.Attribute("ABName");
                string assetPath = (string)element.Attribute("Path");
                if (string.IsNullOrEmpty(bundleName) || string.IsNullOrEmpty(assetPath) || !byName.TryGetValue(bundleName, out BundleInfo bundle))
                    continue;

                long sourceBytes = 0;
                string absolutePath = Path.GetFullPath(assetPath);
                if (File.Exists(absolutePath))
                    sourceBytes = new FileInfo(absolutePath).Length;

                bundle.Assets.Add(new AssetInfo
                {
                    Path = assetPath,
                    Extension = Path.GetExtension(assetPath),
                    SourceBytes = sourceBytes,
                    BundleName = bundleName,
                    BundleBytes = bundle.FileBytes
                });
                bundle.SourceBytes += sourceBytes;

                string dependencies = (string)element.Attribute("ABDependce");
                if (!string.IsNullOrWhiteSpace(dependencies))
                {
                    foreach (string dependency in dependencies.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (!bundle.Dependencies.Contains(dependency))
                            bundle.Dependencies.Add(dependency);
                    }
                }
            }
        }
        catch (Exception exception)
        {
            status = "Could not read bundle mapping: " + exception.Message;
        }
    }

    private void ExportCsv()
    {
        string path = EditorUtility.SaveFilePanel("Export AssetBundle report", "", "AssetBundleSizes.csv", "csv");
        if (string.IsNullOrEmpty(path))
            return;

        StringBuilder csv = new StringBuilder();
        csv.AppendLine("Bundle,BundleBytes,DirectDependencyBytes,AssetCount,BundleSourceBytes,AssetPath,AssetSourceBytes,AssetType");
        foreach (BundleInfo bundle in bundles)
        {
            if (bundle.Assets.Count == 0)
            {
                csv.AppendLine(string.Join(",", Csv(bundle.Name), bundle.FileBytes, bundle.DirectDependencyBytes, 0, bundle.SourceBytes, "", 0, ""));
                continue;
            }

            foreach (AssetInfo asset in bundle.Assets)
                csv.AppendLine(string.Join(",", Csv(bundle.Name), bundle.FileBytes, bundle.DirectDependencyBytes,
                    bundle.Assets.Count, bundle.SourceBytes, Csv(asset.Path), asset.SourceBytes, Csv(asset.Extension)));
        }

        File.WriteAllText(path, csv.ToString(), new UTF8Encoding(true));
        EditorUtility.RevealInFinder(path);
    }

    private static string Csv(string value)
    {
        return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
    }

    private static string FormatBytes(long bytes)
    {
        double value = bytes;
        string[] units = { "B", "KiB", "MiB", "GiB" };
        int unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }
        return value.ToString("0.##") + " " + units[unit];
    }

    private sealed class BundleInfo
    {
        public string Name;
        public long FileBytes;
        public long DirectDependencyBytes;
        public long SourceBytes;
        public readonly List<string> Dependencies = new List<string>();
        public readonly List<AssetInfo> Assets = new List<AssetInfo>();
    }

    private sealed class AssetInfo
    {
        public string Path;
        public string Extension;
        public long SourceBytes;
        public string BundleName;
        public long BundleBytes;
    }
}
