namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using System.Collections.Generic;
    using CoProcessors;
    using UniModules.UniGame.TypeConverters.Editor;
    using UnityEngine;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    [Serializable]
    public class GoogleSpreadsheetSettings
    {
        /// <summary>
        /// credential profile file
        /// https://developers.google.com/sheets/api/quickstart/dotnet
        /// </summary>
#if ODIN_INSPECTOR
        [FilePath(RequireExistingPath = true)]
#endif
        public string credentialsPath;

#if ODIN_INSPECTOR
#endif
        public string user = "user";

        [Tooltip("timeout to google auth in sec")] [Range(10, 100)]
        public float authTimeout = 30f;

#if ODIN_INSPECTOR
#endif
        [Header("try to auto-connect to spreadsheets when window opened")]
        public bool autoConnect = true;

        /// <summary>
        /// list of target sheets
        /// </summary>
        [Space(4)]
#if ODIN_INSPECTOR
        [InfoBox("Add any valid spreadsheet id's")]
        [TableList]
#endif
        public List<SpreadSheetInfo> sheets = new List<SpreadSheetInfo>();

        [Space(8)]
#if ODIN_INSPECTOR
        [TitleGroup("Type Converters")]
        [InlineEditor()]
        [HideLabel]
#endif
        public ObjectTypeConverter typeConverter;

        [Space(8)]
#if ODIN_INSPECTOR
        [TitleGroup("Co Processors")]
        [InlineEditor(InlineEditorObjectFieldModes.Boxed, Expanded = true)]
        [HideLabel]
#endif
        public CoProcessor coProcessors;


#if ODIN_INSPECTOR
        [OnInspectorInit]
#endif
        private void OnInspectorInitialize()
        {
            typeConverter ??= ObjectTypeConverter.TypeConverters;
        }
    }
}