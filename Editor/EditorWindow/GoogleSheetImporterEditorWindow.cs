using System.Collections.Generic;
using System.Linq;

#if ODIN_INSPECTOR

namespace UniModules.UniGame.GoogleSpreadsheetsImporter.Editor.EditorWindow
{
    using UniModules.Editor;
    using global::UniGame.GoogleSpreadsheets.Editor;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using UnityEditor;
    using UnityEngine;

    public class GoogleSheetImporterEditorWindow : OdinMenuEditorWindow
    {
        #region static data
        
        [MenuItem("UniGame/Google/Spreadsheet ImporterWindow")]
        public static void Open()
        {
            var window = GetWindow<GoogleSheetImporterEditorWindow>();
            window.titleContent = new GUIContent("Google Sheets");
            window.Show();
        }
        
        public static GoogleSpreadsheetImporter GetGoogleSpreadsheetImporter()
        {
            //load importer asset
            var importer = AssetEditorTools.GetAsset<GoogleSpreadsheetImporter>();
            if (importer) return importer;
            
            importer = CreateInstance<GoogleSpreadsheetImporter>();
            importer.SaveAsset(nameof(GoogleSpreadsheetImporter), GoogleSheetImporterEditorConstants.DefaultGoogleSheetImporterPath);

            return importer;
        }


        #endregion

        [SerializeField]
        [HideLabel]
        [HideInInspector]
        public GoogleSpreadsheetImporter _googleSheetImporter;

        private GoogleImportersCommonOperations _operations;
        private List<ISpreadsheetProcessor> _assetsHandlers;
        private OdinMenuTree _menuTree;
        
        #region private methods

        protected override OdinMenuTree BuildMenuTree()
        {
            _googleSheetImporter = GetGoogleSpreadsheetImporter();

            _menuTree = new OdinMenuTree();
            var spreadSheerHandler = _googleSheetImporter.sheetsItemsHandler;
            
            _assetsHandlers = spreadSheerHandler.Importers.ToList();
            
            _operations = new GoogleImportersCommonOperations(_googleSheetImporter);
            
            _menuTree.Add("Commands",_operations);
            
            foreach (var importer in _assetsHandlers)
                _menuTree.Add($"Import & Export/{importer.Name}",importer);
            
            _menuTree.Add("Configuration",_googleSheetImporter);

            if(_googleSheetImporter.AutoConnect)
                _operations.Reconnect();

            OnStartElements();
            
            return _menuTree;
        }

        protected override void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();
            _operations?.DrawEditors();
        }
        
        private void OnStartElements()
        {
            foreach (var item in _assetsHandlers)
                if(item is IStartable startable) startable.Start();
        }
        
        #endregion

    }
}
#endif
