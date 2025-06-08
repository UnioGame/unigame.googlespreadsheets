namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Core.Runtime.SerializableType;
    using Core.Runtime.SerializableType.Extensions;
    using UniModules.Editor;
    using UniModules.UniCore.EditorTools.Editor;
    using UniModules;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;    
#endif
    
    [Serializable]
    public class FolderAssetsImporter : SerializableSpreadsheetProcessor
    {
        private const int LabelWidth = 120;

#if ODIN_INSPECTOR
        [VerticalGroup("Filter")]
        [FolderPath(RequireExistingPath = true)]
        [LabelWidth(LabelWidth)]
        [Required]
#endif
        public string folder;

        public bool cleanupOnImport = false;
        
#if ODIN_INSPECTOR
        [ValueDropdown(nameof(GetAssetTypeDropdown))]
        //[HideLabel]
        [PropertyOrder(-1)]
#endif
        public SType targetAssetType = typeof(ScriptableObject);
        
#if ODIN_INSPECTOR
        [VerticalGroup("Filter")]
        [LabelWidth(LabelWidth)]
        [LabelText("RegEx Filter")]
#endif
        public string assetRegexFilter = string.Empty;

#if ODIN_INSPECTOR
        [VerticalGroup("Filter")]
        [LabelWidth(LabelWidth)]
        [LabelText("Create Missing")]
#endif
        public bool createMissingItems;

#if ODIN_INSPECTOR
        [VerticalGroup("Filter")]
        [LabelWidth(LabelWidth)]
#endif
        public bool overrideSheetId;
        
#if ODIN_INSPECTOR
        [VerticalGroup("Filter")]
        [LabelWidth(LabelWidth)]
        [ShowIf("overrideSheetId")]
#endif
        public string sheetId;
        
#if ODIN_INSPECTOR
        [LabelWidth(LabelWidth)]
        [VerticalGroup("Filter")]
#endif
        public int maxItemsCount = -1;

#if ODIN_INSPECTOR
        [LabelWidth(LabelWidth)]
        [VerticalGroup("Filter")]
        [PropertyTooltip("Example MyNewAsset {0} HERE")]
#endif
        public string assetTemplateName;

#if ODIN_INSPECTOR
        [Button]
#endif
        public List<Object> Load()
        {
            var filterType = GetFilteredType();
            var values = AssetEditorTools.GetAssets(filterType, new[] {folder});
            values = ApplyRegExpFilter(values);
            return values;
        }

        public sealed override ISpreadsheetData ExportObjects(ISpreadsheetData data)
        {
            var source = Load();
            
            if (!data.HasSheet(sheetId))
                return data;

            AssetEditorTools.ShowProgress(ExportValues(source,data,sheetId));

            return data;
        }

        public sealed override ISpreadsheetData ImportObjects(ISpreadsheetData spreadsheetData)
        {
            var source = Load();
            
            if (cleanupOnImport)
            {
                FileUtils.DeleteDirectoryFiles(folder.ToAbsoluteProjectPath());
                AssetDatabase.Refresh();
            }

            var result = new List<object>();
            var filterType = GetFilteredType();
            
            if (filterType == null) return spreadsheetData;

            var assets = source
                .Where(x => x!=null)
                .ToArray();
            
            var syncedAsset = filterType.SyncFolderAssets(
                folder,
                spreadsheetData,
                assets,
                createMissingItems, 
                maxItemsCount,
                overrideSheetId ? sheetId : string.Empty);
            
            result.AddRange(OnPostImportAction(syncedAsset));

            return spreadsheetData;
        }

        public override string FormatName(string assetName)
        {
            return string.IsNullOrEmpty(assetTemplateName) 
                ? assetName 
                : string.Format(assetTemplateName, assetName);
        }
        
        protected virtual Type GetFilteredType() => targetAssetType;

        protected virtual IEnumerable<Object> OnPostImportAction(IEnumerable<Object> importedAssets)
        {
            return importedAssets;
        }

        private IEnumerable<ProgressData> ExportValues(IEnumerable<object> source,ISpreadsheetData data,string sheeName)
        {
            var progressData = new ProgressData
            {
                Title = "Export",
                IsDone =  false,
            };
            
            var targetObjects = source.ToList();
            var count         = targetObjects.Count;
            for (var index = 0; index < targetObjects.Count; index++) {
                var targetObject = targetObjects[index];
                
                targetObject.UpdateSheetValue(data,sheeName);
                
                progressData.Progress = index / (float) count;
                progressData.Content  = $"{index} : {count}";
                yield return progressData;
            }

            progressData.IsDone = true;
            yield return progressData;
        }
        
        private List<Object> ApplyRegExpFilter(List<Object> assets)
        {
            if (string.IsNullOrEmpty(assetRegexFilter))
                return assets;
            
            var filteredAssets = new List<Object>();
            var regexpr = new Regex(assetRegexFilter, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            
            foreach (var asset in assets) {
                if (!asset) continue;
                var resource = asset.ToEditorResource();
                if(!regexpr.IsMatch(resource.AssetPath))
                    continue;
                filteredAssets.Add(asset);
            }

            return filteredAssets;
        }
        
#if ODIN_INSPECTOR
        private IEnumerable<ValueDropdownItem<SType>> GetAssetTypeDropdown()
        {
            var baseType = typeof(ScriptableObject);
            var baseSType = (SType)baseType;
            var assetTypes = baseSType.GetAssignableNonAbstractTypes();
            foreach (var sType in assetTypes)
            {
                yield return new ValueDropdownItem<SType>()
                {
                    Text = sType.Name,
                    Value = sType
                };
            }
        }
#endif

    }
}