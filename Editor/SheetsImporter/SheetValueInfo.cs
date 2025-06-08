namespace UniGame.GoogleSpreadsheets.Editor
{
    using System.Collections.Generic;

    public class SheetValueInfo
    {
        public object           Source;
        public SheetSyncScheme  SyncScheme;
        public ISpreadsheetData SpreadsheetData;
        public string           SheetName;
        public string           SyncFieldName;
        public object           SyncFieldValue;
        public HashSet<object>  IgnoreCache;
        public int StartColumn;
    }
}