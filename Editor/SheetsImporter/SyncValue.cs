namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using System.Reflection;

    [Serializable]
    public class SyncValue
    {
        public static readonly SyncValue Empty = new();
        
        public FieldInfo fieldInfo;
        public PropertyInfo propertyInfo;

        public SyncValueType valueType = SyncValueType.Field;
        public Type targetType;
        
        public string objectField = string.Empty;
        public string sheetField = string.Empty;
        public bool isKeyField = false;
        public bool allowRead = true;
        public bool allowWrite = true;
        public SheetSyncScheme syncScheme;

        public bool IsSheetTarget => syncScheme != null;

        #region public methods

        public object GetValue(object source)
        {
            if(!allowRead) return null;
            
            switch (valueType)
            {
                case SyncValueType.Field:
                    return fieldInfo.GetValue(source);
                case SyncValueType.Property:
                    return propertyInfo.GetValue(source);
            }

            return null;
        }

        public SyncValue ApplyValue(object source, object value)
        {
            switch (valueType)
            {
                case SyncValueType.Field:
                    fieldInfo.SetValue(source,value);
                    break;
                case SyncValueType.Property:
                    propertyInfo.SetValue(source,value);
                    break;
            }
            fieldInfo.SetValue(source, value);
            return this;
        }

        public override string ToString()
        {
            return
                $"Object Field: {objectField} IsKey: {isKeyField} Sheet Field: {sheetField} TargetType: {targetType.Name}";
        }

        #endregion
    }
}