namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using System.Collections.Generic;
    using Object = UnityEngine.Object;

    public interface IAssetSheetDataProcessor
    {
        /// <summary>
        /// Sync folder assets by spreadsheet data
        /// </summary>
        /// <param name="filterType"></param>
        /// <param name="folder"></param>
        /// <param name="createMissing">if true - create missing assets</param>
        /// <param name="spreadsheetData"></param>
        /// <param name="maxItems"></param>
        /// <param name="overrideSheetId"></param>
        /// <returns></returns>
        List<Object> SyncFolderAssets(
            Type filterType, 
            string folder,
            bool createMissing, 
            ISpreadsheetData spreadsheetData,
            int maxItems = -1,
            string overrideSheetId = "",
            Func<string,string> assetNameFormatter = null);

        /// <summary>
        /// Sync folder assets by spreadsheet data
        /// </summary>
        /// <param name="filterType"></param>
        /// <param name="assets"></param>
        /// <param name="folder"></param>
        /// <param name="createMissing">if true - create missing assets</param>
        /// <param name="spreadsheetData"></param>
        /// <param name="maxItemsCount"></param>
        /// <param name="overrideSheetId">force override target sheet id</param>
        /// <returns></returns>
        List<Object> SyncFolderAssets(
            Type filterType, 
            string folder,
            ISpreadsheetData spreadsheetData,
            Object[] assets = null,
            bool createMissing = true, 
            int maxItemsCount = -1,
            string overrideSheetId = "",
            Func<string,string> assetNameFormatter = null);

        IEnumerable<Object> ApplyAssets(
            Type filterType,
            string sheetId,
            string folder,
            SheetSyncScheme syncScheme,
            ISpreadsheetData spreadsheetData,
            object[] keys,
            Object[] assets = null,
            int count = -1,
            bool createMissing = true,
            string keyFieldName = "",
            Func<string,string> assetNameFormatter = null);

        object ApplyData(object source, ISpreadsheetData spreadsheetData);

        object ApplyData(SheetValueInfo syncValueInfo);

        bool              UpdateSheetValue(object source, ISpreadsheetData data,string sheetId = "", string sheetKeyField = "");

        bool UpdateSheetValue(SheetValueInfo sheetValueInfo);
        
        IEnumerable<SyncValue> SelectSheetFields(SheetSyncScheme schemaValue,SheetData data);

    }
}