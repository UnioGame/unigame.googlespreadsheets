namespace UniGame.GoogleSpreadsheets.Editor
{
    public interface ISpreadsheetAssetsImporter
    {
        bool CanImport { get; }

        ISpreadsheetData Import(ISpreadsheetData spreadsheetData);
    }
}