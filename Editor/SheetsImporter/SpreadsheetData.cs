namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Serializable]
    public class SpreadsheetData : ISpreadsheetData
    {
        private IEnumerable<SheetData> _sheets;

        public SpreadsheetData(IEnumerable<SheetData> sheets)
        {
            _sheets = sheets;
        }

        public bool HasSheet(string sheetName)
        {
            return _sheets.Any(x => x.Name.Trim('\'')
                .ToLower()
                .Equals(sheetName.ToLower()));
        }

        public IEnumerable<SheetData> Sheets => _sheets;

        public SheetData this[string sheetName] => _sheets
            .FirstOrDefault(x => x.Name.Equals(sheetName,StringComparison.OrdinalIgnoreCase));
    }
}