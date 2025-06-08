namespace UniGame.GoogleSpreadsheets.Editor
{
    using global::UniGame.Core.Runtime;

    public interface ISpreadsheetTriggerAssetsHandler : IResetable
    {
        void                                   Initialize(IGoogleSpreadsheetClient client);
    }
}