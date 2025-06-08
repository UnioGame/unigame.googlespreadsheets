using System;
using UnityEngine;

namespace UniGame.GoogleSpreadsheets.Editor
{
    [Serializable]
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.HideLabel]
    [Sirenix.OdinInspector.BoxGroup("Attributes Source")]
#endif
    [CreateAssetMenu(menuName = "UniGame/Google/Importers/SpreadsheetFolderImporter",fileName = nameof(SpreadsheetAssetsFolderImporter))]
    public class SpreadsheetAssetsFolderImporter : SpreadsheetImporter<FolderAssetsImporter>
    {
    }
}
