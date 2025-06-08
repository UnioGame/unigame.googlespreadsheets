namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using Object = UnityEngine.Object;

    [Serializable]
    public class SheetSyncItem
    {
        public string sheetId;
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.InlineEditor]
#endif
        public Object asset;

        public object target;
    }
}