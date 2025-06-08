namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using UniGame.Runtime.Utils;
    using UniModules.UniGame.TypeConverters.Editor;

    [Serializable]
    public class SheetData
    {
        #region static data

        private static Func<string, string> _fieldKeyFactory = MemorizeTool
            .Create<string, string>(x => x.TrimStart('_').ToLower());

        private const string _spaceString = " ";

        public static string FormatKey(string key) => _fieldKeyFactory(key);

        public static bool IsEquals(string key1, string key2) => FormatKey(key1) == FormatKey(key2);
        
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

        public object this[int x, string y] => x == 0 ? _table.Columns[y].ColumnName : _table.Rows[x][y];

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
            return _table.Columns.Contains(_fieldKeyFactory(key));
        }

        public DataColumn GetColumn(string key)
        {
            var fieldKey = _fieldKeyFactory(key);
            return _table.Columns.Contains(fieldKey) ? _table.Columns[fieldKey] : null;
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
            var key = _fieldKeyFactory(fieldName);
            if (!HasColumn(key)) return false;
            
            var currentValue = row[key];
            var newValue     = value.TryConvert(typeof(string));
            if(currentValue.Equals(newValue)) return false;
            
            row[key] = newValue;
            
            AcceptChanges();
            
            return true;
        }

        public object GetValue(string key, object keyValue, string fieldName)
        {
            fieldName = _fieldKeyFactory(fieldName);
            var row = GetRow(key, keyValue);
            return row?[fieldName];
        }
        
        public object GetValue(int rowIndex, string columnName)
        {
            var fieldName = _fieldKeyFactory(columnName);
            var row = _table.Rows[rowIndex];
            return row?[fieldName];
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
            return _table.Columns.Contains(_fieldKeyFactory(key));
        }

        public bool AddValue(string key, object value)
        {
            var columnKey = _fieldKeyFactory(key);
            if (!_table.Columns.Contains(columnKey))
                return false;
            var row = _table.NewRow();
            foreach (DataColumn column in _table.Columns) {
                row[column.ColumnName] = string.Empty;
            }

            row[columnKey] = value;
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
        
        public bool AddHeader(string title)
        {
            if (string.IsNullOrEmpty(title))
                return false;
            var titleKey = _fieldKeyFactory(title);
            if (_table.Columns.Contains(titleKey))
                return false;
 
            _headers.Add(title);
            _table.Columns.Add(titleKey);
            
            AcceptChanges();

            return true;
        }
        
        public DataRow GetRow(string fieldName, object value)
        {
            value = value ?? string.Empty;
            var key = _fieldKeyFactory(fieldName);
            
            for (var i = 0; i < _table.Rows.Count; i++) {
                var row      = _table.Rows[i];
                var rowValue = row[key];
                if (Equals(rowValue, value.TryConvert<string>()))
                    return row;
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