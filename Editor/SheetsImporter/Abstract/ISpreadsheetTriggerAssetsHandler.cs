namespace UniGame.GoogleSpreadsheets.Editor
{
    using global::UniGame.Core.Runtime;

    public interface ISpreadsheetTriggerAssetsHandler : IResettable
    {
        void                                   Initialize(IGoogleSpreadsheetClient client);
    }
}