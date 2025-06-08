namespace UniGame.GoogleSpreadsheets.Editor
{
    using System.Collections.Generic;
    using UnityEngine;

    public interface IAssetsProvider
    {
        List<Object> GetAssets();
    }
}