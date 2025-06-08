namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Runtime;
    using UniGame.Runtime.ReflectionUtils;
    using UniGame.Runtime.Utils;

    public class SheetSyncSchemaProcessor
    {
        
        public static MemorizeItem<Type, Dictionary<string, SyncValue>> CachedSyncValues =
            MemorizeTool.Memorize<Type, Dictionary<string, SyncValue>>(x =>
            {
                return CreateSyncValueMap(x, GoogleSpreadsheetConstants.KeyField);
            });
        
        public static MemorizeItem<SyncSpreadsheetValue,SheetSyncScheme> CachedSyncScheme =
            MemorizeTool.Memorize<SyncSpreadsheetValue,SheetSyncScheme>(CreateSyncScheme);
        
        public SheetSyncScheme CreateSyncScheme(Type type)
        {
            var sheetItemAttribute = CustomAttributeExtensions.GetCustomAttribute<SpreadsheetTargetAttribute>(type);
            var spreadsheetValue = new SyncSpreadsheetValue()
            {
                spreadsheet = sheetItemAttribute,
                type = type,
            };
            return CreateSyncScheme(spreadsheetValue);
        }

        public static SheetSyncScheme CreateSyncScheme(SyncSpreadsheetValue spreadsheetValue)
        {
            var type = spreadsheetValue.type;
            var spreadsheetTarget = spreadsheetValue.spreadsheet;
            var sheetName = type.Name;
            var useAllFields = true;
            var keyField = GoogleSpreadsheetConstants.KeyField;

            var sheetItemAttribute = spreadsheetTarget;
            if (sheetItemAttribute != null)
            {
                useAllFields = sheetItemAttribute.SyncAllFields;
                sheetName = sheetItemAttribute.UseTypeName ? sheetName : sheetItemAttribute.SheetName;
                keyField = string.IsNullOrEmpty(sheetItemAttribute.KeyField) 
                    ? GoogleSpreadsheetConstants.KeyField
                    : sheetItemAttribute.KeyField;
            }

            var result = new SheetSyncScheme(sheetName);
            result.keyField = keyField;
            result.values = CachedSyncValues[type];
            result.keyValue = result.values
                .FirstOrDefault(x => 
                    SheetData.IsEquals(keyField,x.Value.objectField)).Value;
            return result;
        }
        
        private IEnumerable<SyncValue> LoadSyncValuesData(Type sourceType)
        {
            var values = CachedSyncValues[sourceType];
            return values.Values;
        }

        public static Dictionary<string,SyncValue> CreateSyncValueMap(Type sourceType,string keyField)
        {
            var result = new Dictionary<string, SyncValue>();
            FillSyncValuesFromFields(result, sourceType, keyField);
            FillSyncValuesFromProperties(result, sourceType, keyField);
            return result;
        }

        public static void FillSyncValuesFromFields(Dictionary<string, SyncValue> map, Type sourceType, string keyField)
        {
            var fields = sourceType.GetInstanceFields();
            var spreadsheetTargetAttribute = CustomAttributeExtensions.GetCustomAttribute<SpreadsheetTargetAttribute>(sourceType);

            var fieldsAttributes = new List<SpreadsheetValueAttribute>();
            var keyFieldSheetName = GoogleSpreadsheetConstants.KeyField;

            var keyFieldName = spreadsheetTargetAttribute != null ? spreadsheetTargetAttribute.KeyField : keyField;

            foreach (var field in fields)
            {
                var attributeInfo = CustomAttributeExtensions.GetCustomAttribute<SpreadsheetValueAttribute>(field.FieldType);

                fieldsAttributes.Add(attributeInfo);

                if (attributeInfo != null && attributeInfo.isKey)
                    keyFieldName = attributeInfo.useFieldName ? field.Name : attributeInfo.sheetField;
            }

            for (var i = 0; i < fields.Count; i++)
            {
                var fieldInfo = fields[i];
                var customAttribute = fieldsAttributes[i];

                var fieldName = SheetData.FormatKey(fieldInfo.Name);
                var sheetField = customAttribute is { useFieldName: false }
                    ? customAttribute.sheetField
                    : fieldName;

                if(map.ContainsKey(sheetField))
                    continue;
                
                var isKeyField = SheetData.IsEquals(keyFieldName, fieldName);

                var spreadsheetAttribute =
                    fieldInfo.GetCustomAttributes(typeof(SpreadsheetTargetAttribute), true)
                        .FirstOrDefault() as ISpreadsheetDescription;

                var fieldSyncScheme = spreadsheetAttribute == null
                    ? null
                    : CachedSyncScheme[new SyncSpreadsheetValue()
                    {
                        spreadsheet = spreadsheetAttribute,
                        type = fieldInfo.FieldType,
                    }];

                var syncField = CreateSyncValue(fieldInfo, sheetField, isKeyField, fieldSyncScheme);
                var key = SheetData.FormatKey(sheetField);
                map[key] = syncField;
            }
        }
        
        public static void FillSyncValuesFromProperties(Dictionary<string, SyncValue> map, Type sourceType, string keyField)
        {
            var properties = sourceType.GetProperties();
            var spreadsheetTargetAttribute = CustomAttributeExtensions.GetCustomAttribute<SpreadsheetTargetAttribute>(sourceType);

            var fieldsAttributes = new List<SpreadsheetValueAttribute>();
            var keyFieldSheetName = GoogleSpreadsheetConstants.KeyField;

            var keyFieldName = spreadsheetTargetAttribute != null ? spreadsheetTargetAttribute.KeyField : keyField;

            foreach (var propertyInfo in properties)
            {
                var attributeInfo = CustomAttributeExtensions.GetCustomAttribute<SpreadsheetValueAttribute>(propertyInfo.PropertyType);
                fieldsAttributes.Add(attributeInfo);
                
                if (attributeInfo is { isKey: true })
                    keyFieldName = attributeInfo.useFieldName ? propertyInfo.Name : attributeInfo.sheetField;
            }

            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var customAttribute = fieldsAttributes[i];

                var propertyName = SheetData.FormatKey(property.Name);
                var sheetField = customAttribute is { useFieldName: false }
                    ? customAttribute.sheetField
                    : propertyName;

                if(map.ContainsKey(sheetField)) continue;
                
                var isKeyField = SheetData.IsEquals(keyFieldName, propertyName);

                var spreadsheetAttribute =
                    property.GetCustomAttributes(typeof(SpreadsheetTargetAttribute), true)
                        .FirstOrDefault() as ISpreadsheetDescription;

                var fieldSyncScheme = spreadsheetAttribute == null
                    ? null
                    :CachedSyncScheme[new SyncSpreadsheetValue()
                    {
                        spreadsheet = spreadsheetAttribute,
                        type = property.PropertyType,
                    }];

                var syncField = CreateSyncValue(property, sheetField, isKeyField, fieldSyncScheme);
                var key = SheetData.FormatKey(sheetField);
                map[key] = syncField;
            }
        }
        
        public static SyncValue CreateSyncValue(PropertyInfo property, string sheetValueField, bool isKey,
            SheetSyncScheme scheme = null)
        {
            var syncValue = new SyncValue()
            {
                propertyInfo = property,
                fieldInfo = null,
                objectField = property.Name,
                valueType = SyncValueType.Property,
                sheetField = SheetData.FormatKey(sheetValueField),
                isKeyField = isKey,
                targetType = property.PropertyType,
                syncScheme = scheme,
                allowRead = property.CanRead,
                allowWrite = property.CanWrite,
            };

            return syncValue;
        }

        public static SyncValue CreateSyncValue(FieldInfo field, string sheetValueField, bool isKey,
            SheetSyncScheme scheme = null)
        {
            var syncValue = new SyncValue()
            {
                fieldInfo = field,
                objectField = field.Name,
                valueType = SyncValueType.Field,
                sheetField = SheetData.FormatKey(sheetValueField),
                isKeyField = isKey,
                targetType = field.FieldType,
                syncScheme = scheme,
                allowRead = true,
                allowWrite = true,
                propertyInfo = null
            };

            return syncValue;
        }
    }
}