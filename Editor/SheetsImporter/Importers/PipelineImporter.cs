namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UniCore.Runtime.ProfilerTools;
    using UnityEditor;
    using UnityEngine;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
    using Sirenix.Utilities.Editor;
    
#endif
    
    [CreateAssetMenu(menuName = "UniGame/Google/PipelineImporter",fileName = nameof(PipelineImporter))]
    public class PipelineImporter : BaseSpreadsheetProcessor
    {
        private static Color _oddColor = new Color(0.2f, 0.4f, 0.3f);
        
#if ODIN_INSPECTOR
        [Tooltip("If true, the asset database will not be refreshed after importing. This is useful if you want to import assets without having them show up in the project view.")]
#endif
        public bool disableDBOnImport = false;

#if ODIN_INSPECTOR
        [Tooltip("If true, the asset database will not be refreshed after importing. This is useful if you want to import assets without having them show up in the project view.")]
#endif
        public bool disableDBOnExport = false;
        
#if ODIN_INSPECTOR
        [Tooltip("if true, reimport all assets on each step")]
#endif
        public bool reimportOnStep = false;
        
#if ODIN_INSPECTOR
        [ListDrawerSettings(ElementColor = nameof(GetElementColor),ListElementLabelName = "Name")]//OnEndListElementGUI = "BeginDrawImporterElement"
#endif
        public List<SpreadsheetProcessorValue> importers = new List<SpreadsheetProcessorValue>();

        public override bool CanImport => importers
            .Where(x => x.HasValue)
            .Any(x => x.Value.CanImport);
        
        public override bool CanExport => importers
            .Where(x => x.HasValue)
            .Any(x => x.Value.CanExport);

        public bool IsValidData =>  Client!= null && 
                                    Client.Status !=null && 
                                    Client.Status.HasConnectedSheets;
        
        public override ISpreadsheetData ExportObjects(ISpreadsheetData data)
        {
            if (disableDBOnExport)
                AssetDatabase.StartAssetEditing();
            try
            {
                var items = importers
                    .Where(x => x.HasValue && x.Value.CanExport);

                foreach (var exporter in items)
                {
                    exporter.Value.Export(data);
                    if (reimportOnStep)
                    {
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
            }
            catch (Exception e)
            {
                GameLog.LogError(e);
                return data;
            }
            finally
            {
                if (disableDBOnExport)
                {
                    AssetDatabase.StopAssetEditing();
                }
            }

            return data;
        }

        public sealed override ISpreadsheetData ImportObjects(ISpreadsheetData spreadsheetData)
        {
            var stage = new List<object>();

            if (disableDBOnImport)
                AssetDatabase.StartAssetEditing();
            
            try
            {
                foreach (var importer in importers
                             .Where(x => x.HasValue))
                {
                    var value = importer.Value;
                    value.Import(spreadsheetData);
                    
                    if (reimportOnStep)
                    {
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }

                stage = OnPostImport(stage).ToList();
            }
            catch (Exception e)
            {
                GameLog.LogError(e);
            }
            finally
            {
                if (disableDBOnImport)
                {
                    //By adding a call to StopAssetEditing inside
                    //a "finally" block, we ensure the AssetDatabase
                    //state will be reset when leaving this function
                    AssetDatabase.StopAssetEditing();
                }
            }
            
            return spreadsheetData;
        }

        
        protected override void OnInitialize(IGoogleSpreadsheetClient client)
        {
            base.OnInitialize(client);

            foreach (var importer in importers.Where(x => x.HasValue))
            {
                var value = importer.Value;
                value.Initialize(client);
            }
        }

        
        protected virtual IEnumerable<object> OnPreImport(IEnumerable<object> sourceImporters)
        {
            return sourceImporters;
        }
        
        protected virtual IEnumerable<object> OnPostImport(IEnumerable<object> importedAssets)
        {
            return importedAssets;
        }
        
        private Color GetElementColor(int index, Color defaultColor)
        {
            var result = index % 2 == 0 
                ? _oddColor : defaultColor;
            return result;
        }
        
        private void BeginDrawImporterElement(int index)
        {
            var importer = importers[index];
            var handler = importer.Value;
            if (handler == null) return;

#if ODIN_INSPECTOR
            if (handler.CanImport && IsValidData && SirenixEditorGUI.SDFIconButton("import",14,SdfIconType.Download))
            {
                handler.Initialize(Client);
                handler.Import(Client.SpreadsheetData);
            }

            if (handler.CanExport && IsValidData && SirenixEditorGUI.SDFIconButton("export",14,SdfIconType.Upload))
            {
                handler.Initialize(Client);
                handler.Export(Client.SpreadsheetData);
            }
#endif  
        }

    }
}