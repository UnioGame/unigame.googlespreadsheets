namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using UniGame.Common;

    [Serializable]
    public class SpreadsheetProcessorValue : VariantValue<
        SerializableSpreadsheetProcessor,
        BaseSpreadsheetProcessor,
        ISpreadsheetProcessor>
    {
        public const string EmptyValue = "EMPTY";
        
        public string Name => HasValue ? Value.Name : EmptyValue;
        
    }
}