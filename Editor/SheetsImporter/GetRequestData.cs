using System;

namespace UniGame.GoogleSpreadsheets.Editor
{
    [Serializable]
    public class GetRequestData<TRequest,TValue>
    {
        public TRequest Request;
        public Action<TValue> Result;
        public TValue Value;
        public bool isReady;
    }
}