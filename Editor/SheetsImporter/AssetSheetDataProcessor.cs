namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using CoProcessors.Abstract;
    using Core.Runtime.Extension;
    using UniModules.Editor;
    using UniModules.UniCore.EditorTools.Editor;
    using UniModules.UniGame.TypeConverters.Editor;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class AssetSheetDataProcessor : IAssetSheetDataProcessor
    {
        private readonly ICoProcessorHandle _coProcessorHandle;
        
        private static readonly SheetSyncScheme _dummyItem = new(string.Empty);

        public AssetSheetDataProcessor(ICoProcessorHandle coProcessorHandle)
        {
            _coProcessorHandle = coProcessorHandle;
        }

        public SheetSyncScheme CreateSyncItem(object source)
        {
            return source == null ? _dummyItem : CreateSyncItem(source.GetType());
        }

        /// <summary>
        /// Sync folder assets by spreadsheet data
        /// </summary>
        /// <param name="filterType"></param>
        /// <param name="folder"></param>
        /// <param name="createMissing">if true - create missing assets</param>
        /// <param name="spreadsheetData"></param>
        /// <param name="maxItems"></param>
        /// <param name="overrideSheetId"></param>
        /// <returns></returns>
        public List<Object> SyncFolderAssets(
            Type filterType,
            string folder,
            bool createMissing,
            ISpreadsheetData spreadsheetData,
            int maxItems = -1,
            string overrideSheetId = "",
            Func<string,string> assetNameFormatter = null)
        {
            if (!filterType.IsScriptableObject() && !filterType.IsComponent()) {
                Debug.LogError($"SyncFolderAssets: BAD target type {filterType}");
                return null;
            }

            var assets = AssetEditorTools.GetAssets<Object>(filterType, folder);
            var result = SyncFolderAssets(
                filterType,
                folder,
                spreadsheetData,
                assets.ToArray(),
                createMissing, maxItems,
                overrideSheetId,
                assetNameFormatter);
            return result;
        }

        /// <summary>
        /// Sync folder assets by spreadsheet data
        /// </summary>
        /// <param name="filterType"></param>
        /// <param name="assets"></param>
        /// <param name="folder"></param>
        /// <param name="createMissing">if true - create missing assets</param>
        /// <param name="spreadsheetData"></param>
        /// <param name="maxItemsCount"></param>
        /// <param name="overrideSheetId">force override target sheet id</param>
        /// <param name="assetNameFormatter"></param>
        /// <returns></returns>
        public List<Object> SyncFolderAssets(
            Type filterType,
            string folder,
            ISpreadsheetData spreadsheetData,
            Object[] assets = null,
            bool createMissing = true,
            int maxItemsCount = -1,
            string overrideSheetId = "",
            Func<string,string> assetNameFormatter = null)
        {
            var result = assets != null ? new List<Object>(assets) : new List<Object>();

            if (!filterType.IsScriptableObject() && !filterType.IsComponent()) {
                Debug.LogError($"SyncFolderAssets: BAD target type {filterType}");
                return result;
            }

            var syncScheme = filterType.CreateSheetScheme();

            var sheetId = string.IsNullOrEmpty(overrideSheetId) ? syncScheme.sheetId : overrideSheetId;

            var sheet = spreadsheetData[sheetId];
            if (sheet == null) {
                Debug.LogWarning($"{nameof(AssetSheetDataProcessor)} Missing Sheet with name {sheetId}");
                return result;
            }

            var keyField = syncScheme.keyValue;

            if (keyField == null) {
                Debug.LogWarning($"{nameof(AssetSheetDataProcessor)} Key field missing sheet = {sheetId}");
                return result;
            }

            var keysId = keyField.sheetField;
            var column = sheet.GetColumn(keysId);
            if (column == null) {
                Debug.LogWarning($"{nameof(AssetSheetDataProcessor)} Keys line missing with id = {keysId}");
                return result;
            }

            var values = sheet.GetColumnValues(keysId)
                .Where(x => !string.IsNullOrEmpty(x.ToString()))
                .ToArray();
            
            var updatedItems = ApplyAssets(
                filterType,
                sheetId,
                folder,
                syncScheme,
                spreadsheetData,
                values,
                assets, maxItemsCount,
                createMissing,
                string.Empty,assetNameFormatter);

            foreach (var item in updatedItems)
            {
                if(result.Contains(item)) continue;
                result.Add(item);
            }
            
            return result;
        }

        public IEnumerable<Object> ApplyAssets(
            Type filterType,
            string sheetId,
            string folder,
            SheetSyncScheme syncScheme,
            ISpreadsheetData spreadsheetData,
            object[] keys,
            Object[] assets = null,
            int count = -1,
            bool createMissing = true,
            string keyFieldName = "",
            Func<string,string> assetNameFormatter = null)
        {
            assets ??= Array.Empty<Object>();
            
            count = count < 0 ? keys.Length : count;
            count = Math.Min(keys.Length, count);

            var keyField = string.IsNullOrEmpty(keyFieldName) 
                ? syncScheme.keyValue 
                : syncScheme.GetFieldBySheetFieldName(keyFieldName);

            try {
                for (var i = 0; i < count; i++) {
                    var keyValue = keys[i];
                    var key      = keyValue.TryConvert<string>();

                    Object targetAsset = null;
                    var createAsset = false;
                    var assetName = $"{filterType.Name}_{i + 1}";
                    
                    foreach (var asset in assets)
                    {
                        if(asset == null) continue;
                        var keyFieldValue = keyField.GetValue(asset).TryConvert<string>();
                        var found = string.Equals(keyFieldValue,key, StringComparison.OrdinalIgnoreCase);
                        if(!found) continue;
                        targetAsset = asset;
                        break;
                    }
                    
                    //create asset if missing
                    if (targetAsset == null) {
                        //skip asset creation step
                        if (createMissing == false)
                            continue;

                        createAsset = true;
                        targetAsset = filterType.CreateAsset();
                        targetAsset.name = assetName;
                        
                        assetName = assetNameFormatter == null
                            ? assetName
                            : assetNameFormatter?.Invoke(assetName);
                    }

                    //show assets progression
                    AssetEditorTools.ShowProgress(new ProgressData
                    {
                        IsDone   = false,
                        Progress = i / (float) count,
                        Content  = $"{i}:{count}  {targetAsset.name}",
                        Title    = "Spreadsheet Importing"
                    });

                    var spreadsheetValueInfo = new SheetValueInfo
                    {
                        Source          = targetAsset,
                        SheetName         = sheetId,
                        SpreadsheetData = spreadsheetData,
                        SyncScheme      = syncScheme,
                        SyncFieldName   = keyField.sheetField,
                        SyncFieldValue  = keyValue,
                    };
                    
                    ApplyData(spreadsheetValueInfo);

                    if (createAsset)
                    {
                        targetAsset.SaveAsset(assetName, folder, false);
                        Debug.Log($"Create Asset [{targetAsset}] for path {folder}", targetAsset);
                    }
                    
                    targetAsset.MarkDirty();
                    
                    yield return targetAsset;
                }
            }
            finally {
                AssetEditorTools.ShowProgress(new ProgressData {IsDone = true,});
                AssetDatabase.SaveAssets();
            }
        }

        private object ApplyData(SheetValueInfo valueInfo, DataRow row)
        {
            var syncScheme      = valueInfo.SyncScheme;
            var spreadsheetData = valueInfo.SpreadsheetData;
            var source          = valueInfo.Source;
            var rowValues       = row.ItemArray;
            var table           = row.Table;

            if (valueInfo.IsInIgnoreCache()) {
                return source;
            }

            var values = syncScheme.values;
            
            for (var i = valueInfo.StartColumn; i < rowValues.Length; i++) 
            {
                var columnName = table.Columns[i].ColumnName;
                var key = SheetData.FormatKey(columnName);
                var found = values.TryGetValue(key, out var itemField);
                if(!found) continue;

                //check for recurvice call
                var currentValue = itemField.GetValue(source);
                if(valueInfo.IsInIgnoreCache(currentValue))
                    continue;
                
                var rowValue     = rowValues[i];
                var result  = rowValue.ConvertType(itemField.targetType);
                
                var resultValue = result.Result;

                itemField.ApplyValue(source, resultValue);

                if (itemField.IsSheetTarget) {
                    //initialize cache
                    valueInfo.IgnoreCache = valueInfo.IgnoreCache ?? new HashSet<object>();
                    valueInfo.IgnoreCache.Add(source);

                    ApplyData(new SheetValueInfo
                    {
                        Source          = resultValue,
                        SpreadsheetData = spreadsheetData,
                        SyncScheme      = itemField.targetType.CreateSheetScheme(),
                        IgnoreCache     = valueInfo.IgnoreCache
                    });
                }
            }

            if (_coProcessorHandle != null)
            {
                _coProcessorHandle.Apply(valueInfo, row);
            }

            if(source is Object asset) asset.MarkDirty();
            
            return source;
        }

        public object ApplyData(object source, ISpreadsheetData spreadsheetData)
        {
            var syncScheme = source.CreateSheetScheme();

            var syncValue = new SheetValueInfo
            {
                Source          = source,
                SpreadsheetData = spreadsheetData,
                SheetName         = syncScheme.sheetId,
                SyncScheme      = syncScheme,
                SyncFieldName   = syncScheme.keyValue.sheetField,
            };

            var result = ApplyData(syncValue);
            return result;
        }

        public bool ValidateSheetInfo(ref SheetValueInfo sheetValueInfo)
        {
            var source          = sheetValueInfo.Source;
            var syncScheme      = sheetValueInfo.SyncScheme;
            var spreadsheetData = sheetValueInfo.SpreadsheetData;
            var sheetId         = sheetValueInfo.SheetName;

            syncScheme = syncScheme ?? source?.GetType().CreateSheetScheme();

            if (source == null || syncScheme == null)
                return false;

            sheetId                = string.IsNullOrEmpty(sheetValueInfo.SheetName) ? syncScheme.sheetId : sheetId;
            sheetValueInfo.SheetName = sheetId;

            if (!spreadsheetData.HasSheet(sheetId))
                return false;

            var keyField = string.IsNullOrEmpty(sheetValueInfo.SyncFieldName) ? syncScheme.keyValue?.sheetField : sheetValueInfo.SyncFieldName;

            if (string.IsNullOrEmpty(keyField) || !spreadsheetData.HasSheet(sheetId))
                return false;

            sheetValueInfo.SyncFieldName = keyField;

            var syncKeyField = syncScheme.GetFieldBySheetFieldName(keyField);
            var keyValue     = sheetValueInfo.SyncFieldValue ?? syncKeyField?.GetValue(source);
            if (keyValue == null)
                return false;

            sheetValueInfo.SyncFieldValue = keyValue;

            return true;
        }

        public object ApplyData(SheetValueInfo sheetValueInfo)
        {
            if (!ValidateSheetInfo(ref sheetValueInfo))
                return sheetValueInfo.Source;

            var spreadsheetData = sheetValueInfo.SpreadsheetData;
            var sheetName         = sheetValueInfo.SheetName;

            var sheet = spreadsheetData[sheetName];
            var row   = sheet.GetRow(sheetValueInfo.SyncFieldName, sheetValueInfo.SyncFieldValue);

            if (row == null)
            {
                Debug.LogError($"Can't find in Sheet {sheetName} row with ID {sheetValueInfo.SyncFieldName} == {sheetValueInfo.SyncFieldValue}");
                return sheetValueInfo.Source;
            }
            
            var result = ApplyData(sheetValueInfo, row);
            return result;
        }

        public bool UpdateSheetValue(object source, 
            ISpreadsheetData data, 
            string sheetId = "", 
            string sheetKeyField = "")
        {
            return UpdateSheetValue(source, data, sheetId, sheetKeyField,null);
        }

        public bool UpdateSheetValue(object source, ISpreadsheetData data, 
            string sheetId , 
            string sheetKeyField, 
            object keyValue)
        {
            if (source == null) return false;
            
            var type       = source.GetType();
            var syncScheme = type.CreateSheetScheme();
            var keyFieldName = string.IsNullOrEmpty(sheetKeyField) 
                ? syncScheme.keyValue.sheetField 
                : sheetKeyField;
            
            var keyField = string.IsNullOrEmpty(sheetKeyField) 
                ? syncScheme.keyValue 
                : syncScheme.GetFieldBySheetFieldName(sheetKeyField);
            
            syncScheme.keyField = keyFieldName;
            syncScheme.keyValue = keyField;
            
            if (keyField == null)
                return false;

            var sheetValueInfo = new SheetValueInfo
            {
                Source          = source,
                SpreadsheetData = data,
                SyncScheme      = syncScheme,
                SyncFieldName   = sheetKeyField,
                SyncFieldValue = keyValue,
                SheetName         = sheetId
            };

            return UpdateSheetValue(sheetValueInfo);
        }

        public bool UpdateSheetValue(SheetValueInfo sheetValueInfo)
        {
            if (!ValidateSheetInfo(ref sheetValueInfo))
                return false;

            var source          = sheetValueInfo.Source;
            var schemeValue     = sheetValueInfo.SyncScheme;
            var spreadsheetData = sheetValueInfo.SpreadsheetData;
            var sheetId         = sheetValueInfo.SheetName;

            var sheet = spreadsheetData[sheetId];
            var row   = sheet.GetRow(sheetValueInfo.SyncFieldName, sheetValueInfo.SyncFieldValue) ?? sheet.CreateRow();

            //var sheetFields = SelectSheetFields(schemaValue, data);
            var values = schemeValue.values;
            foreach (var item in values)
            {
                var field       = item.Value;
                var sourceValue = field.GetValue(source);
                sourceValue = sourceValue ?? string.Empty;
                sheet.UpdateValue(row, field.sheetField, sourceValue);

                if (field.IsSheetTarget) {}
            }

            return true;
        }

        public IEnumerable<SyncValue> SelectSheetFields(SheetSyncScheme schemaValue, SheetData data)
        {
            var columns = data.Columns;
            
            for (var i = 0; i < columns.Count; i++) {
                var column = columns[i];
                var syncValue = schemaValue.GetFieldBySheetFieldName(column.ColumnName);
                if (syncValue == null) yield return null;
                yield return syncValue;
            }
        }
    }
}