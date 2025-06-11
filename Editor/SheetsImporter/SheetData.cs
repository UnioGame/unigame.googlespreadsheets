namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using UniGame.Runtime.Utils;
    using TypeConverters.Editor;

    [Serializable]
    public class SheetData
    {
        #region static data

        private static MemorizeItem<string, string> _fieldKeyFactory = MemorizeTool
            .Memorize<string, string>(x =>
            {
                var key = x.TrimStart('_').ToLower();
                key = key.Replace(" ", string.Empty);
                return key;
            });
        
        private static MemorizeItem<string, string> _fieldToColumnFactory = MemorizeTool
            .Memorize<string, string>(FieldToColumnName);
        
        private static MemorizeItem<string, string> _columnToFieldFactory = MemorizeTool
            .Memorize<string, string>(ColumnNameToFieldCamelCase);

        private const string _spaceString = " ";

        public static string FieldToColumnName(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName)) return string.Empty;
            if(fieldName.Contains(" ")) return fieldName;
            
            // Убираем подчёркивания в начале
            fieldName = fieldName.TrimStart('_');
            // Разбиваем по заглавным буквам (camelCase, PascalCase)
            var spaced = Regex.Replace(fieldName, "([A-Z])", " $1").Trim();
            // Если snake_case — заменяем _ на пробелы
            spaced = spaced.Replace("_", " ");
            // Делаем первую букву каждой части заглавной
            return CultureInfo.InstalledUICulture.TextInfo.ToTitleCase(spaced.ToLower());
        }
        
        public static string ColumnNameToFieldCamelCase(string columnName)
        {
            var parts = columnName.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return string.Empty;
            var result = parts[0].ToLower();
            for (var i = 1; i < parts.Length; i++)
                result += CultureInfo.InstalledUICulture.TextInfo.ToTitleCase(parts[i].ToLower());
            return result;
        }
        
        public static string FormatKey(string key) => _fieldKeyFactory[key];
        
        public static string FormatToColumnName(string fieldName) => _fieldToColumnFactory[fieldName];
        
        public static string FormatToFieldName(string columnName) => _columnToFieldFactory[columnName];

        public static bool IsEquals(string key1, string key2) => IsEqualsNames(key1,key2);
        
        public static bool IsEqualsNames(string key,string columnName)
        {
            if(key.Equals(columnName,StringComparison.InvariantCultureIgnoreCase)) 
                return true;
            
            var value = FormatKey(key);
            
            if(value.Equals(columnName,StringComparison.InvariantCultureIgnoreCase)) 
                return true;
            
            var columnKey = FormatKey(columnName);
            if(value.Equals(columnKey,StringComparison.InvariantCultureIgnoreCase)) 
                return true;
            
            value = FormatToColumnName(key);
            if(value.Equals(columnName,StringComparison.InvariantCultureIgnoreCase)) 
                return true;

            return false;
        }
        
        #endregion
        
        
        private readonly MajorDimension _dimension;
        private List<object> _headers = new();
        private          StringBuilder  _stringBuilder = new(300);
        private          DataTable      _table;
        private          bool           _isChanged = false;

        public SheetData(string sheetId, string spreadsheetId, MajorDimension dimension)
        {
            _dimension = dimension;
            
            var fixedId = sheetId.TrimStart('\'');
            fixedId = fixedId.TrimEnd('\'');
            
            _table     = new DataTable(fixedId, spreadsheetId);
        }

        #region public properties

        public bool IsChanged => _isChanged;

        public string SpreadsheetId => _table.Namespace;

        public string Name => _table.TableName;

        public DataTable Table => _table;

        public DataColumnCollection Columns => _table.Columns;

        public DataRowCollection Rows => _table.Rows;
        
        public int RowsCount => _table.Rows.Count;

        public int ColumnsCount => _table.Columns.Count;

        public object this[int x, string y]
        {
            get
            {
                var columnName = GetColumnName(y);
                if (string.IsNullOrEmpty(columnName)) return null;
                var result = x == 0 ? columnName 
                    : _table.Rows[x][columnName];
                return result;
            }
        }

        #endregion

        public IList<IList<object>> CreateSource()
        {
            var items = new List<IList<object>>();

            items.Add(_headers);
            
            foreach (DataRow row in _table.Rows) {
                items.Add(row.ItemArray.ToList());
            }

            return items;
        }

        public IEnumerable<object> GetColumnValues(string key)
        {
            var column = GetColumn(key);
            if (column == null)
                yield break;
            var columnName = column.ColumnName;
            foreach (DataRow row in _table.Rows) {
                yield return row[columnName];
            }
        }
        
        public bool HasColumn(string key)
        {
            var columns = _table.Columns;
            if(columns.Contains(key)) return true;
            
            var columnName = FormatKey(key);
            if (columns.Contains(columnName)) return true;
            
            columnName = FormatToColumnName(key);
            if (columns.Contains(columnName)) return true;

            return false;
        }
        
        public DataColumn GetColumn(string key)
        {
            foreach (DataColumn column in _table.Columns)
            {
                if(IsEqualsNames(key,column.ColumnName))
                    return column;
            }
            return null;
        }
        
        public string GetColumnName(string key)
        {
            foreach (DataColumn column in _table.Columns)
            {
                var columnName = column.ColumnName;
                if(IsEqualsNames(key,columnName))
                    return columnName;
            }
            return string.Empty;
        }

        public void Commit()
        {
            _isChanged = false;
        }

        public DataRow CreateRow()
        {
            _isChanged = true;
            return AddRow(_table);    
        }
        
        public bool UpdateValue(DataRow row, int column,object value)
        {
            _isChanged = true;
            if (column >= ColumnsCount)
                return false;
            row[column] = value;
            return true;
        }
        
        public bool UpdateValue(DataRow row, string fieldName,object value)
        {
            var key = GetColumnName(fieldName);
            if (string.IsNullOrEmpty(key)) return false;
            
            var currentValue = row[key];
            var newValue     = value.TryConvert(typeof(string));
            if(currentValue.Equals(newValue)) return false;
            
            row[key] = newValue;
            
            AcceptChanges();
            
            return true;
        }

        public object GetValue(string key, object keyValue, string fieldName)
        {
            var columnName = GetColumnName(fieldName);
            var row = GetRow(key, keyValue);
            return row?[columnName];
        }
        
        public object GetValue(int rowIndex, string columnName)
        {
            columnName = GetColumnName(columnName);
            var row = _table.Rows[rowIndex];
            return row?[columnName];
        }

        public SheetData Update(IList<IList<object>> source)
        {
            _isChanged = true;
            _table.Clear();
            _headers.Clear();

            ParseSourceData(source);

            return this;
        }

        public bool HasData(string key)
        {
            return HasColumn(key);
        }

        public bool AddValue(string key, object value)
        {
            var columnName = GetColumnName(key);
            if (string.IsNullOrEmpty(columnName)) return false;
            
            var row = _table.NewRow();
            foreach (DataColumn column in _table.Columns) {
                row[column.ColumnName] = string.Empty;
            }
            row[columnName] = value;
            return true;
        }

        public override string ToString()
        {
            _stringBuilder.Clear();
            var columns = _table.Columns;
            foreach (DataColumn column in columns) {
                _stringBuilder.Append(column.ColumnName);
                _stringBuilder.Append(_spaceString);
            }

            _stringBuilder.AppendLine();

            foreach (DataRow row in _table.Rows) {
                _stringBuilder.Append(string.Join(_spaceString, row.ItemArray));
                _stringBuilder.AppendLine();
            }

            return _stringBuilder.ToString();
        }

        public void MarkDirty()
        {
            _isChanged = true;
        }

        public void AcceptChanges()
        {
            _table.AcceptChanges();
            MarkDirty();
        }
        
        public bool RemoveRow(string fieldName, object keyValue)
        {
            var row = GetRow(fieldName, keyValue);
            if (row == null) return false;
            
            row.Delete();
            
            AcceptChanges();
            return true;
        }

        public void AddHeaders(IList<object> data)
        {
            AddHeaders(_table, data);
        }
        
        public DataRow AddRow(IList<object> data = null)
        {
            return AddRow(_table, data);
        }
        
        public DataRow WriteRow(string fieldName, object value,List<object> values)
        {
            var row = GetRow(fieldName, value);
            if (row == null)
            {
                return AddRow(values);
            }
            
            var columns = _table.Columns;
            var valuesCount = values.Count;
            
            row.BeginEdit();
            
            for (var i = 0; i < columns.Count; i++) {
                var valueData =  i < valuesCount ? values[i] : string.Empty;
                row[columns[i].ColumnName] = valueData.ToString();
            }
            
            row.EndEdit();
            row.AcceptChanges();
            AcceptChanges();
            
            return row;
        }
        
        public bool AddHeader(string title, bool formatTitle = true)
        {
            if (string.IsNullOrEmpty(title)) return false;
            
            var titleKey = formatTitle 
                ? FormatToColumnName(title)
                : FormatKey(title);
            
            if (HasColumn(titleKey)) return false;
 
            _headers.Add(titleKey);
            _table.Columns.Add(titleKey);
            
            AcceptChanges();

            return true;
        }
        
        public DataRow GetRow(string fieldName, object value)
        {
            value = value ?? string.Empty;
            var key = GetColumnName(fieldName);
            if (string.IsNullOrEmpty(key)) return null;
            var targetValue = value.TryConvert<string>();
            
            for (var i = 0; i < _table.Rows.Count; i++) {
                var row      = _table.Rows[i];
                var rowValue = row[key];
                if (Equals(rowValue, targetValue)) return row;
            }

            return null;
        }

        private void ParseSourceData(IList<IList<object>> source)
        {
            var rows = source.Count;
            for (var i = 0; i < rows; i++) {
                var line = source[i];

                if (i == 0) {
                    AddHeaders(_table, line);
                }
                else {
                    AddRow(_table, line);
                }
            }
        }

        private DataRow AddRow(DataTable table, IList<object> line = null)
        {
            var row     = table.NewRow();
            var columns = table.Columns;
            var lineLen = line?.Count ?? 0;
            if (columns.Count <= 0) return row;
            
            for (var i = 0; i < columns.Count; i++) {
                row[columns[i].ColumnName] = i < lineLen ? line[i] : string.Empty;
            }

            table.Rows.Add(row);
            AcceptChanges();
            return row;
        }

        private void AddHeaders(DataTable table, IList<object> headers)
        {
            foreach (var header in headers) {
                var title = header == null ? string.Empty : header.ToString();
                AddHeader(title);
            }
            
            AcceptChanges();
        }
    }
}