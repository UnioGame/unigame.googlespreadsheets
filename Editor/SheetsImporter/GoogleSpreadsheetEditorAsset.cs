namespace UniGame.GoogleSpreadsheets.Editor
{
    using CoProcessors;
    using UniModules;
    using UniModules.Editor;
    using TypeConverters.Editor;
    using UnityEditor;
    using UnityEngine;

    [CreateAssetMenu(menuName = "UniGame/Google/GoogleSpreadSheet Settings",
        fileName = "GoogleSpreadSheet Settings")]
    public class GoogleSpreadsheetEditorAsset : ScriptableObject
    {
        #region inspector
        
        public CoProcessor processor;
        
        public GoogleSpreadsheetImporter importerAsset;

        #endregion
        
        [MenuItem("Assets/UniGame/Google Spreadsheet/Google Spreadsheet Asset")]
        [MenuItem("Assets/Create/UniGame/Google/Google Spreadsheet Asset")]
        public static void CreateAsset()
        {
            var activeObject = Selection.activeObject;
            if (!activeObject)
                return;
            
            var path = AssetDatabase.GetAssetPath(activeObject);
            path = path.GetDirectoryPath();

            Debug.Log($"ASSET PATH SELECTION :  {path}");

            CreateAsset(path);
        }
        
        private static GoogleSpreadsheetEditorAsset _defaultAsset;
        public static GoogleSpreadsheetEditorAsset EditorSettings
        {
            get
            {
                if (_defaultAsset) return _defaultAsset;
                _defaultAsset = AssetEditorTools.GetAsset<GoogleSpreadsheetEditorAsset>();
                return _defaultAsset;
            }
        }

        public static void CreateAsset(string path)
        {
            var coProcessorAsset = EditorSettings.processor;
            var importerAsset = EditorSettings.importerAsset;

            var result = importerAsset.CopyAsset<GoogleSpreadsheetImporter>(importerAsset.name,path);
            var coProcessor = coProcessorAsset.CopyAsset<CoProcessor>(coProcessorAsset.name, path);
            coProcessor.ResetToDefault();

            result.settings.typeConverter = ObjectTypeConverter.TypeConverters;
            result.settings.coProcessors = coProcessor;
            
            result.MarkDirty();
            coProcessor.MarkDirty();
            
            AssetDatabase.Refresh();
        }
        
    }
}