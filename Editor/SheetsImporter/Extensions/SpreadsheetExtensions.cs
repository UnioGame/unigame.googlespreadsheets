using System.Data;
using System.Linq;

namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using CoProcessors;
    using TypeConverters.Editor;
    using TypeConverters.Editor.Abstract;
    using Object = UnityEngine.Object;

    public static class SpreadsheetExtensions
    {
        public static readonly AssetSheetDataProcessor DefaultProcessor = new(CoProcessor.Processor);

        public static bool UpdateSheetValue(this object source, ISpreadsheetData data, string sheetId, string sheetKeyField)
        {
            return DefaultProcessor.UpdateSheetValue(source, data,sheetId,sheetKeyField);
        }
        
        public static bool UpdateSheetValues(this object source, ISpreadsheetData data, string sheetId, string sheetKeyField, object keyValue)
        {
            return DefaultProcessor.UpdateSheetValue(source, data,sheetId,sheetKeyField, keyValue);
        }
        
        public static bool UpdateSheetValue(this object source, ISpreadsheetData data)
        {
            return DefaultProcessor.UpdateSheetValue(source, data);
        }
        
        public static bool UpdateSheetValue(this object source, ISpreadsheetData data, string sheetId)
        {
            return DefaultProcessor.UpdateSheetValue(source, data,sheetId);
        }
        
        public static bool UpdateValue(this ISpreadsheetData data, object source, 
            string sheetId, string sheetKeyField)
        {
            return DefaultProcessor.UpdateSheetValue(source, data,sheetId,sheetKeyField);
        }
        
        public static bool UpdateValue(this ISpreadsheetData data, object source, string sheetId, 
            string sheetKeyField, object keyValue)
        {
            return DefaultProcessor.UpdateSheetValue(source, data,sheetId,sheetKeyField, keyValue);
        }
        
        public static bool UpdateValue(this ISpreadsheetData data, object source)
        {
            return DefaultProcessor.UpdateSheetValue(source, data);
        }
        
        public static bool UpdateValue(this ISpreadsheetData data,object source, string sheetId)
        {
            return DefaultProcessor.UpdateSheetValue(source, data,sheetId);
        }

        public static bool UpdateValue<T>(this ISpreadsheetData data, List<T> source, 
            string sheetId,
            string sheetKeyField)
            where T : new()
        {
            return UpdateListValue(source,data, sheetId, sheetKeyField);
        }

        public static bool ReadData<T>(this ISpreadsheetData data,List<T> source, 
            string sheetId,
            string sheetKeyField)
            where T : new()
        {
            return ApplySpreadsheetData(source, data, sheetId, sheetKeyField);
        }
        
        public static object ReadData(this ISpreadsheetData data,object asset)
        {
            return DefaultProcessor.ApplyData(asset,data);
        }
        
        public static object ReadData(this ISpreadsheetData spreadsheetData,
            object asset,
            string sheetName,
            string sheetFieldName)
        {
            var valueType = asset.GetType();
            
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                               BindingFlags.IgnoreCase;

            var targetIdName = SheetData.FormatToFieldName(sheetFieldName);
            var idField = valueType.GetField(targetIdName, bindingFlags);
            var idProperty = valueType.GetProperty(targetIdName, bindingFlags);

            var objectIdValue = idField != null 
                ? idField.GetValue(asset) 
                : idProperty?.GetValue(asset);

            if (objectIdValue == null)
            {
                var fields = valueType.GetFields(bindingFlags);
                var targetField = fields.FirstOrDefault(x=>
                    x.Name.Contains(targetIdName, StringComparison.OrdinalIgnoreCase));
                objectIdValue = targetField!=null 
                    ? targetField.GetValue(asset) 
                    : null;
            }
            
            return ApplySpreadsheetData(asset, spreadsheetData, sheetName, objectIdValue, sheetFieldName);
        }
        
        public static object ReadData(
            this ISpreadsheetData spreadsheetData,
            object asset,
            string sheetName,
            object keyValue = null,
            string sheetFieldName = "")
        {
            return ApplySpreadsheetData(asset, spreadsheetData, sheetName, keyValue, sheetFieldName);
        }
        
        public static List<Object> SyncAssets(
            this ISpreadsheetData spreadsheetData,
            Type filterType,
            string folder,
            Object[] assets = null,
            bool createMissing = true,
            int maxItemsCount = -1,
            string overrideSheetId = "")
        {
            return SyncFolderAssets(filterType, folder, spreadsheetData, 
                assets, createMissing, maxItemsCount,
                overrideSheetId);
        }
                
        public static List<Object> SyncAssets(
            this ISpreadsheetData spreadsheetData,
            Type type, 
            string folder,
            bool createMissing)
        {
            return DefaultProcessor.SyncFolderAssets(type, folder, createMissing, spreadsheetData);
        }
        
        public static bool UpdateListValue<T>(this List<T> source, ISpreadsheetData data, string sheetId, string sheetKeyField)
            where T : new()
        {
            source.Clear();
            var sheetData = data[sheetId];
            var rowsCount = sheetData.RowsCount;
            if (string.IsNullOrEmpty(sheetKeyField))
                return false;
            
            for (var i = 0; i < rowsCount; i++)
            {
                var key = sheetData.GetValue(i, sheetKeyField);
                var item = new T();
                item.UpdateSheetValues(data, sheetId, sheetKeyField, key);
                source.Add(item);
            }

            return true;
        }


        public static bool ApplySpreadsheetData<T>(this List<T> source, ISpreadsheetData data, string sheetId, string sheetKeyField)
            where T : new()
        {
            source.Clear();
            var sheetData = data[sheetId];
            var rowsCount = sheetData.RowsCount;
            if (string.IsNullOrEmpty(sheetKeyField))
                return false;
            
            for (var i = 0; i < rowsCount; i++)
            {
                var key = sheetData.GetValue(i, sheetKeyField);
                var item = new T();
                item.ApplySpreadsheetData(data, sheetId, key, sheetKeyField);
                source.Add(item);
            }

            return true;
        }


        public static List<Object> SyncFolderAssets(
            this Type filterType, 
            string folder,
            ISpreadsheetData spreadsheetData,
            Object[] assets = null,
            bool createMissing = true, 
            int maxItemsCount = -1,
            string overrideSheetId = "")
        {
            return DefaultProcessor
                .SyncFolderAssets(filterType, folder, spreadsheetData,assets, createMissing, maxItemsCount, overrideSheetId);
        }

        
        public static List<Object> SyncFolderAssets(
            this Type type, 
            string folder,
            bool createMissing, 
            ISpreadsheetData spreadsheetData)
        {
            return DefaultProcessor.SyncFolderAssets(type, folder, createMissing, spreadsheetData);
        }

        public static object ApplySpreadsheetData(
            this object asset,
            ISpreadsheetData spreadsheetData, 
            string sheetName,
            object keyValue = null,
            string sheetFieldName = "")
        {
            if (spreadsheetData.HasSheet(sheetName) == false)
                return asset;
            
            var syncAsset = asset.CreateSheetScheme();

            var sheetValueIndo = new SheetValueInfo
            {
                Source = asset,
                SheetName = sheetName,
                SpreadsheetData = spreadsheetData,
                SyncScheme = syncAsset,
                SyncFieldName = sheetFieldName,
                SyncFieldValue = keyValue,
                StartColumn = 0
            };
            
            return DefaultProcessor.ApplyData(sheetValueIndo);
        }
        
        public static object ApplySpreadsheetData(
            this object asset, 
            SheetValueInfo sheetValueInfo, 
            string sheetName,
            object keyValue = null,
            string sheetFieldName = "",
            int overrideStartColumn = 0)
        {
            if (!sheetValueInfo.SpreadsheetData.HasSheet(sheetName))
                return asset;
            
            sheetValueInfo.SheetName = sheetName;
            sheetValueInfo.SyncFieldName = sheetFieldName;
            sheetValueInfo.SyncFieldValue = keyValue;
            sheetValueInfo.StartColumn = overrideStartColumn;

            return DefaultProcessor.ApplyData(sheetValueInfo);
        }

        
        public static object ApplySpreadsheetData(this object asset, ISpreadsheetData data)
        {
            return DefaultProcessor.ApplyData(asset,data);
        }
        
        public static object ApplySpreadsheetData(
            this object asset,
            SheetSyncScheme syncAsset, 
            ISpreadsheetData data)
        {
            var sheetValueInfo = new SheetValueInfo
            {
                Source = asset,
                SyncScheme = syncAsset,
                SpreadsheetData = data,
                StartColumn = 0
            };
            return DefaultProcessor.ApplyData(sheetValueInfo);
        }

        public static object ApplySpreadsheetData(
            this object asset,
            Type type,
            ISpreadsheetData sheetData,
            string sheetId,
            object keyValue = null,
            string sheetFieldName = "")
        {
            var syncAsset = type.CreateSheetScheme();
            
            var sheetValue = new SheetValueInfo
            {
                Source = asset,
                SheetName = sheetId,
                SpreadsheetData = sheetData,
                SyncScheme = syncAsset,
                SyncFieldName = sheetFieldName,
                SyncFieldValue = keyValue,
                StartColumn = 0
            };
            
            return DefaultProcessor.ApplyData(sheetValue);
        }

        public static TypeConverterResult ConvertType(this object source, Type target)
        {
            if (source == null)
            {
                var result = new TypeConverterResult()
                {
                    IsComplete = false,
                    Result = null,
                };
                return result;
            }

            if (target.IsInstanceOfType(source))
            {
                var result = new TypeConverterResult()
                {
                    IsComplete = true,
                    Result = source,
                };
                return result;
            }

            return ObjectTypeConverter.TypeConverters.ConvertValue(source, target);
        }

        public static IDictionary<T, V> GetDictionary<T, V>(this SheetData sheet, string keyColumn, string valueColumn)
        {
            var dict = new Dictionary<T, V>();
            
            foreach (var row in sheet.Rows.Cast<DataRow>())
            {
                var key = (T) Convert.ChangeType(row[keyColumn], typeof(T));
                var val = (V) Convert.ChangeType(row[valueColumn], typeof(V));
                dict.Add(key, val);
            }

            return dict;
        }
    }
}