namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using System.Collections.Generic;
    using Google.Apis.Sheets.v4;

    public interface IGoogleSpreadsheetClient : IDisposable
    {
        bool                   IsConnectionRefused { get; }

        bool IsConnected { get; }

        SheetsService          SheetsService       { get; }

        ISpreadsheetData SpreadsheetData { get; }

        IGooglsSpreadsheetClientStatus Status { get; }

        IEnumerable<SheetData> Sheets { get; }

        bool HasSheet(string id);

        void UploadAll(bool force = false);
        
        bool Upload(string sheetId);
        
        bool Upload(SheetData sheet);

        bool Connect();
        
        bool Connect(string user,string credentialsPath);
        
        void ReloadAll();
        void Reload(string id);
        bool ConnectToSpreadsheet(string spreadsheetId);
    }
}