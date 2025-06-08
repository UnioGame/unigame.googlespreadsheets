namespace UniGame.GoogleSpreadsheets.Editor
{
    using System.Collections.Generic;
    using System.Linq;

    public class GoogleSpreadsheetClientStatus : IGooglsSpreadsheetClientStatus
    {
        private IEnumerable<SheetData> _connectedSheets;

        public GoogleSpreadsheetClientStatus(IEnumerable<SheetData> connectedSheets)
        {
            _connectedSheets = connectedSheets;
        }

        #region public properties

        public bool HasConnectedSheets => _connectedSheets.Any(x => x!=null);

        #endregion
    }
}