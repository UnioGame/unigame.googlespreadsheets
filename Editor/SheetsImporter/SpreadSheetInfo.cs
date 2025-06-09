using System;
using UnityEngine;

namespace UniGame.GoogleSpreadsheets.Editor
{
#if ODIN_INSPECTOR
        using Sirenix.OdinInspector;
#endif
        
#if ALCHEMY_INSPECTOR
        using Alchemy.Inspector;
#endif

    [Serializable]
    public class SpreadSheetInfo
    {
        public const string SheetKey = "sheet";
        private const string spreadsheetUrl = "https://docs.google.com/spreadsheets/d/{0}";
        
#if ODIN_INSPECTOR
        [TableColumnWidth(160,Resizable = false)]
#endif
        public string name = string.Empty;

        public string id = string.Empty;

#if ODIN_INSPECTOR
        [Button]        
        [TableColumnWidth(160,Resizable = false)]
        [VerticalGroup("Actions")]
#endif
#if ALCHEMY_INSPECTOR
        [Button]
#endif
        public void Open()
        {
            if (string.IsNullOrEmpty(id)) return;
            Application.OpenURL(string.Format(spreadsheetUrl,id));
        }

    }
}