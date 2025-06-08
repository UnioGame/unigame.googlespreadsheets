namespace UniGame.GoogleSpreadsheets.Editor.CoProcessors
{
    using System;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Data;
    using Abstract;
    using global::UniGame.GoogleSpreadsheets.Editor;

    [Serializable]
    public class NestedTableCoProcessor : ICoProcessorHandle
    {
        private const int TableNameGroupId = 1;
        
        [SerializeField]
        private string _filter = @"\w*\[ref:(\w*)\]";
        [SerializeField, Min(0)]
        private int _overrideStartColumn = 1;

        public void Apply(SheetValueInfo sheetValueInfo, DataRow row)
        {
            sheetValueInfo.IgnoreCache = sheetValueInfo.IgnoreCache ?? new HashSet<object>();
            sheetValueInfo.IgnoreCache.Add(sheetValueInfo.SheetName.ToLower());
            
            var validColumns = GetValidColumns(sheetValueInfo, row);

            foreach (var cell in validColumns)
            {
                var nestedTableName = cell.Key;
                var nestedTableKey = cell.Value;
                
                if(string.IsNullOrEmpty(nestedTableName) || string.IsNullOrEmpty(nestedTableKey.ToString()))
                    continue;
                
                sheetValueInfo.Source.ApplySpreadsheetData(sheetValueInfo, nestedTableName, nestedTableKey, overrideStartColumn: _overrideStartColumn);
            }
        }

        private Dictionary<string, object> GetValidColumns(SheetValueInfo sheetValueInfo, DataRow row)
        {
            var result = new Dictionary<string, object>();

            var table = row.Table;
            var rowValues = row.ItemArray;

            for (var i = 0; i < table.Columns.Count; i++)
            {
                var columnName = table.Columns[i].ColumnName;
                if (IsValidColumn(columnName))
                {
                    var tableName = GetTableName(columnName);
                    if (sheetValueInfo.IsInIgnoreCache(tableName))
                        continue;
                    
                    result.Add(tableName, rowValues[i]);
                }
            }

            return result;
        }

        private string GetTableName(string columnName)
        {
            var regex = new Regex(_filter);
            if (!regex.IsMatch(columnName))
                return columnName;

            var groups = regex.Match(columnName).Groups;
            return groups[TableNameGroupId].Value;
        }

        private bool IsValidColumn(string columnName)
        {
            var regex = new Regex(_filter);
            return regex.IsMatch(columnName);
        }
    }
}