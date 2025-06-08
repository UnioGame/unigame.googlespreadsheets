namespace UniGame.GoogleSpreadsheets.Runtime
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SpreadsheetValueAttribute : Attribute
    {
        public string sheetField = String.Empty;
        public bool isKey = false;
        public bool useFieldName = false;

        /// <summary>
        /// Mark class field as Sheet sync value
        /// </summary>
        /// <param name="sheetField">Name of Table Filed</param>
        /// <param name="isKey">If true - use this field for sync items with data</param>
        public SpreadsheetValueAttribute(string sheetField = "", bool isKey = false)
        {
            this.useFieldName = string.IsNullOrEmpty(sheetField);
            this.isKey = isKey;
            this.sheetField = sheetField;
        }
        
    }
}
