namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class SheetSyncScheme
    {
        public string sheetId;
        public string keyField;
        public SyncValue keyValue;
        public Dictionary<string,SyncValue> values = new();

        public SheetSyncScheme(string sheetId)
        {
            this.sheetId = sheetId;
        }

        public SyncValue GetFieldBySheetFieldName(string fieldName)
        {
            foreach (var item in values)
            {
                if (SheetData.IsEquals(item.Value.sheetField, fieldName))
                {
                    return item.Value;
                }
            }
            
            return null;
        }
        
        public SyncValue GetFieldByObjectField(string fieldName)
        {
            foreach (var item in values)
            {
                if (SheetData.IsEquals(item.Value.objectField, fieldName))
                {
                    return item.Value;
                }
            }
            return null;
        }
    }
}