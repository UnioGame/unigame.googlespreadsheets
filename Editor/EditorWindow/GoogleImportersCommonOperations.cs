namespace UniModules.UniGame.GoogleSpreadsheetsImporter.Editor.EditorWindow
{
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
    using Sirenix.Utilities.Editor;
#endif
    using UnityEngine;
    using System;
    using global::UniGame.GoogleSpreadsheets.Editor;

    [Serializable]
    public class GoogleImportersCommonOperations
    {
        private const string ImportLabel = "Import All";
        private const string ExportLabel = "Export All";
        private readonly GoogleSpreadsheetImporter _importer;

#if ODIN_INSPECTOR
        [ButtonGroup("Importers")]
        [Button(ImportLabel)]
        [EnableIf(nameof(HasConnectedSheets))]
#endif
        public void Import() => _importer.Import();

#if ODIN_INSPECTOR
        [ButtonGroup("Importers")]
        [Button(ExportLabel)]
        [EnableIf(nameof(HasConnectedSheets))]
#endif
        public void Export() => _importer.Export();

#if ODIN_INSPECTOR
        [ButtonGroup()]
        [EnableIf(nameof(IsValidToConnect))]
        [Button("Reload Spreadsheets")]
#endif
        public void Reconnect() => _importer.Reconnect();

        public GoogleImportersCommonOperations(GoogleSpreadsheetImporter importer)
        {
            _importer = importer;
        }

        public void DrawEditors()
        {
#if ODIN_INSPECTOR
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                GUILayout.FlexibleSpace();
                if (SirenixEditorGUI.ToolbarButton(nameof(Reconnect)))
                    Reconnect();
                if (SirenixEditorGUI.ToolbarButton(ImportLabel))
                    Import();
                if (SirenixEditorGUI.ToolbarButton(ExportLabel))
                    Export();
            }
            SirenixEditorGUI.EndHorizontalToolbar();
#endif
        }


        private bool HasConnectedSheets() => _importer && _importer.HasConnectedSheets;

        private bool IsValidToConnect() => _importer && _importer.IsValidToConnect;
    }
}