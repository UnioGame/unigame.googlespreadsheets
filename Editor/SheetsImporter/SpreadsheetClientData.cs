namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;

    [Serializable]
    public class SpreadsheetClientData
    {
        public string user;
        public string credentialsPath;
        public string appName;
        public string[] scope;
        public float timeout = 30f;
    }
}