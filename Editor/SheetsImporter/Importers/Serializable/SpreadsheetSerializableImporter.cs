namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public abstract class SpreadsheetSerializableImporter : ISpreadsheetAssetsProcessor
    {
        public string importerName = string.Empty;
        
        public string Name => string.IsNullOrEmpty(importerName) ? GetType().Name : importerName;
        
        public abstract bool CanImport { get; }
        public abstract bool CanExport { get; }

        public abstract IEnumerable<object> Load();
        public abstract ISpreadsheetData Import(ISpreadsheetData spreadsheetData);
        public virtual ISpreadsheetData Export(ISpreadsheetData data) => data;

        public abstract ISpreadsheetData ExportObjects(ISpreadsheetData data);
        
        public abstract ISpreadsheetData ImportObjects(ISpreadsheetData spreadsheetData);

        public virtual void Start() { }

        public virtual string FormatName(string assetName) => assetName;
    }
}