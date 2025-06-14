namespace UniGame.GoogleSpreadsheets.Runtime
{
    using System;

    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Field|AttributeTargets.Property)]
    public class SpreadsheetIgnoreAttribute: Attribute
    {
        
    }
}