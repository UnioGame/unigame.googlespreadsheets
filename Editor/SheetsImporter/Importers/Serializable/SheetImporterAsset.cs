namespace UniGame.GoogleSpreadsheets.Editor
{
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    public class SheetImporterAsset<TImporter> : BaseSpreadsheetProcessor
        where TImporter : SerializableSpreadsheetProcessor
    {
#if ODIN_INSPECTOR
        [InlineProperty]
        [HideLabel]
#endif
        public TImporter importer;

        public override bool CanImport => importer.CanImport;
        public override bool CanExport => importer.CanExport;
        public override string Name => importer.Name;
   
        public override ISpreadsheetData ImportObjects(ISpreadsheetData spreadsheetData)
        {
            return importer.ImportObjects(spreadsheetData);
        }

        public override ISpreadsheetData ExportObjects( ISpreadsheetData spreadsheetData)
        {
            return importer.ExportObjects(spreadsheetData);
        }

        public override string FormatName(string assetName) => importer.Name;
        
        protected override void OnReset()
        {
            importer.Reset();
        }
    }
}