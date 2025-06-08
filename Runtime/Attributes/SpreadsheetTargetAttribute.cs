namespace UniGame.GoogleSpreadsheets.Runtime
{
    using System;
    using global::UniGame.GoogleSpreadsheets.Runtime;

    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Field)]
    public class SpreadsheetTargetAttribute : Attribute, ISpreadsheetDescription
    {
        private string _sheetName = string.Empty;
        private bool _syncAllFields = false;
        private string _keyField = GoogleSpreadsheetConstants.KeyField;

        public SpreadsheetTargetAttribute(string sheetName = "",string keyField = "",bool syncAllFields = true)
        {
            _sheetName = sheetName;
            _keyField = string.IsNullOrEmpty( keyField ) ? _keyField : keyField;
            _syncAllFields = syncAllFields;
        }

        public bool UseTypeName => string.IsNullOrEmpty(_sheetName);

        public string SheetName => _sheetName;

        public string KeyField => _keyField;

        public bool SyncAllFields => _syncAllFields;
    }
}
