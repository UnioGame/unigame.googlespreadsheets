namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Serializable]
    public class SheetLineData 
    {
        public string        id   = string.Empty;
        public List<SheetValue>  data = new();
        public int index = -1;
        
        public SheetValue this[string fieldName] {
            get {
                var value = data.FirstOrDefault(x =>
                    string.Equals(x.fieldName, fieldName, StringComparison.OrdinalIgnoreCase));
                return value;
            }
        }
        
    }
}