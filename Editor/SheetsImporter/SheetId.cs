namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using Runtime;

    [Serializable]
    public class SheetId
    {
        public string sheetName = string.Empty;
        public string keyField = GoogleSpreadsheetConstants.KeyField;
    }
}