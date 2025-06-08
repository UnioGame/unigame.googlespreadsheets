namespace UniGame.GoogleSpreadsheets.Editor
{
    public interface ISpreadsheetAssetsExporter
    {
        bool CanExport { get; }
        
        ISpreadsheetData Export(ISpreadsheetData data);

    }
}