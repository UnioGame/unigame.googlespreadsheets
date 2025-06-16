namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    using Core.Runtime;
    using UniGame.Runtime.DataFlow;
    using UnityEngine;
    using UnityEngine.UIElements;

#if ALCHEMY_INSPECTOR
    using Alchemy.Inspector;
#endif
    
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    [Serializable]
    public class SpreadsheetHandler : ISpreadsheetAssetsProcessor, IResettable
    {
        private static Color _oddColor = new(0.2f, 0.4f, 0.3f);
        
        #region inspector

        public string importerName = string.Empty;

#if ODIN_INSPECTOR
        [InfoBox("Reload spreadsheet on each reimport action")]
#endif
        [Tooltip("Reload spreadsheet on each reimport action")]
        public bool autoReloadSpreadsheetOnImport = true;

#if ODIN_INSPECTOR
        [ListDrawerSettings(ElementColor = nameof(GetElementColor),ListElementLabelName = "Name")]
#endif
#if ALCHEMY_INSPECTOR
        [ListViewSettings(ShowAlternatingRowBackgrounds = AlternatingRowBackground.All, 
            ShowFoldoutHeader = false,Reorderable = true)]
#endif
        public List<SpreadsheetProcessorValue> importers = new();

        #endregion

        #region private data

        private ISpreadsheetData _spreadsheetData;
        private IGoogleSpreadsheetClient _client;
        private LifeTimeDefinition _lifeTime;

        #endregion

        public string Name => string.IsNullOrEmpty(importerName) ? GetType().Name : importerName;

        public ILifeTime LifeTime => _lifeTime ??= new LifeTimeDefinition();

        public IEnumerable<ISpreadsheetProcessor> Importers => importers
            .Where(x => x.HasValue)
            .Select(x => x.Value);

        public bool CanImport => true;
        public bool CanExport => true;

        public void Reset() => _lifeTime?.Release();

        public void Initialize(IGoogleSpreadsheetClient client)
        {
            Reset();

            _client = client;
            _spreadsheetData = client.SpreadsheetData;

            foreach (var importer in Importers)
            {
                importer.Initialize(client);
            }

            LifeTime.AddCleanUpAction(() => _client = null);
            LifeTime.AddCleanUpAction(() => _spreadsheetData = null);
        }

        public ISpreadsheetData Import()
        {
            return Import(_spreadsheetData);
        }

        public ISpreadsheetData Export()
        {
            var data = Export(_spreadsheetData);
            return ExportSheets(data);
        }

        public ISpreadsheetData ExportSheets(ISpreadsheetData data)
        {
            var sheets = data.Sheets.ToList();
            foreach (var sheetData in sheets.Where(sheetData => sheetData.IsChanged))
            {
                _client.Upload(sheetData);
            }

            return data;
        }

        public ISpreadsheetData Import(ISpreadsheetData spreadsheetData)
        {
            if (autoReloadSpreadsheetOnImport)
                _client.ReloadAll();

            var importItems = Importers
                .Where(x => x.CanImport)
                .ToList();
            
            foreach (var importer in importItems)
            {
                var importerResult = importer.Import(spreadsheetData);
            }

            return spreadsheetData;
        }

        public ISpreadsheetData ImportObjects(ISpreadsheetData spreadsheetData)
        {
            foreach (var importer in Importers.Where(x => x.CanImport))
            {
                importer.Import(spreadsheetData);
            }

            return spreadsheetData;
        }

        public ISpreadsheetData ExportObjects(ISpreadsheetData data)
        {
            var stage = data;
            
            foreach (var importer in Importers.Where(x => x.CanImport))
            {
                stage = importer.Export(data);
            }

            return stage;
        }

        public ISpreadsheetData Export(ISpreadsheetData data)
        {
            foreach (var importer in Importers.Where(x => x.CanExport))
            {
                Export(data, importer);
            }

            return data;
        }
        
        public virtual string FormatName(string assetName) => assetName;

        private ISpreadsheetData Export(ISpreadsheetData data, ISpreadsheetAssetsProcessor importer)
        {
            return importer.Export(data);
        }

        private ISpreadsheetData Import(ISpreadsheetData data, ISpreadsheetAssetsProcessor importer)
        {
            return importer.Import(data);
        }

        public virtual void Start()
        {
        }

        
        private Color GetElementColor(int index, Color defaultColor)
        {
            var result = index % 2 == 0 
                ? _oddColor : defaultColor;
            return result;
        }
        
        private string GetImporterName(SpreadsheetProcessorValue importer)
        {
            return importer.HasValue == false ? "EMPTY" : importer.Value.Name;
        }
    }
}