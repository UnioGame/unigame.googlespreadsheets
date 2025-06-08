namespace UniGame.GoogleSpreadsheets.Editor
{
    using System.Collections.Generic;
    using System.Linq;

    public class SpreadsheetImporter<T> : BaseSpreadsheetProcessor
        where T : SerializableSpreadsheetProcessor
    {
        public List<T> importers = new List<T>();

        public override bool CanImport => importers.Any(x => x.CanImport);
        public override bool CanExport => importers.Any(x => x.CanExport);
        

        public override ISpreadsheetData ExportObjects(ISpreadsheetData data)
        {
            var items = importers
                .Where(x => x.CanExport);
            
            foreach (var importer in items) 
                importer.Export(data);

            return data;
        }

        public sealed override ISpreadsheetData ImportObjects(ISpreadsheetData spreadsheetData)
        {
            foreach (var importer in importers.Where(x=>x.CanImport))
            {
                importer.Import(spreadsheetData);
            }
            
            return spreadsheetData;
        }

        
        protected override void OnInitialize(IGoogleSpreadsheetClient client)
        {
            base.OnInitialize(client);

            foreach (var importer in importers)
            {
                importer.Initialize(client);
            }
        }

        
        protected virtual IEnumerable<T> OnPreImport(IEnumerable<T> sourceImporters)
        {
            return sourceImporters;
        }
        
        protected virtual IEnumerable<object> OnPostImport(IEnumerable<object> importedAssets)
        {
            return importedAssets;
        }
    }
}