namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using System.Collections.Generic;
    using Core.Runtime.SerializableType;
    using Core.Runtime.SerializableType.Extensions;
    using UniModules.Editor;
    using Runtime;
    using UnityEngine;
    using Object = UnityEngine.Object;
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    //public class AssetsWithAttributesImporterAsset  : BaseSpreadsheetImporter
    
    [Serializable]
#if ODIN_INSPECTOR
    [HideLabel]
    [BoxGroup("Attributes Source")]
#endif
    public class AssetsWithAttributesImporter :  SerializableSpreadsheetProcessor 
    {
#if ODIN_INSPECTOR
        [ValueDropdown(nameof(GetAssetTypeDropdown))]
        [PropertyOrder(-1)]
#endif
        public SType targetAssetType = typeof(ScriptableObject);
        
#if ODIN_INSPECTOR
        [Button]
#endif
        public void LoadAssets()
        {
            foreach (var asset in Load()) { }
        }
        
        public List<Object> Load()
        {
            if(targetAssetType == null) return default;
            
            return AssetEditorTools.GetAssets(targetAssetType);
        }

        public override ISpreadsheetData ImportObjects(ISpreadsheetData spreadsheetData)
        {
            var source =Load();
            if(source == null) return spreadsheetData;
            
            var result = FilterAttributesTargets(source);
            
            foreach (var item in result) {
                if(!spreadsheetData.HasSheet(item.sheetId) || item.asset == null)
                    continue;
                item.target.ApplySpreadsheetData(spreadsheetData,item.sheetId);
                item.asset.MarkDirty();
            }

            return spreadsheetData;
        }
        
        public override ISpreadsheetData ExportObjects(ISpreadsheetData spreadsheetData)
        {
            var source =Load();
            var result = FilterAttributesTargets(source);
            foreach (var item in result) {
                if(item.asset == null) continue;
                item.asset.UpdateSheetValue(spreadsheetData, item.sheetId);
            }
            
            return spreadsheetData;
        }

        public IEnumerable<SheetSyncItem> FilterAttributesTargets(IEnumerable<object> source)
        {
            foreach (var sourceValue in source)
            {
                var attributeValue = sourceValue
                    .GetAttributeOnSelfOrChildren<SpreadsheetTargetAttribute>();
                
                if(!attributeValue.IsFound) continue;
                
                var attribute = attributeValue.Attribute;
                var target = attributeValue.Target;
                
                var sheetName = attribute == null || attribute.UseTypeName
                    ? target.GetType().Name
                    : attribute.SheetName;
                
                var asset = attributeValue.Source as Object;
                    
                var value = new SheetSyncItem
                {
                    asset = asset,
                    target = attributeValue.Target,
                    sheetId = sheetName
                };

                yield return value;
            }
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