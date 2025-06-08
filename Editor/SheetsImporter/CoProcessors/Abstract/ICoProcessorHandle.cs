using System.Data;

namespace UniGame.GoogleSpreadsheets.Editor.CoProcessors.Abstract
{
    public interface ICoProcessorHandle
    {
        void Apply(SheetValueInfo valueInfo, DataRow row);
    }
}