namespace UniGame.GoogleSpreadsheets.Editor
{
    using System;
    using System.IO;
    using System.Linq;
    using Core.Runtime;
    using Runtime;
    using UniGame.Runtime.DataFlow;
    
    using UnityEditor;
    using UnityEngine;

#if ALCHEMY_INSPECTOR
    using Alchemy.Inspector;
#endif
    
#if ODIN_INSPECTOR
    using UniModules.UniGame.GoogleSpreadsheetsImporter.Editor.EditorWindow;
    using Sirenix.OdinInspector;
#endif
    
    //[CreateAssetMenu(menuName = "UniGame/Google Spreadsheets/GoogleSpreadSheetImporter", fileName = nameof(GoogleSpreadsheetImporter))]
    public class GoogleSpreadsheetImporter : ScriptableObject, ILifeTimeContext
    {
        public const int DefaultButtonsWidth = 100;
        public const string ButtonsGroup = "сommands";
        public const string ButtonsActionsGroup = "actions";
        public const string SettingsTab = "settings";
        public const string ImporterTab = "importers";

        #region inspector

        /// <summary>
        /// list of assets linked by attributes
        /// </summary>
        [Space(20)]
#if ODIN_INSPECTOR
        [TabGroup(ImporterTab, ImporterTab)]
        [HorizontalGroup("importers/importers/handlers")]
        [VerticalGroup("importers/importers/handlers/sources")]
        [InlineProperty]
        [HideLabel]
        //[BoxGroup(ImporterTab + "/Assets Handlers")]
#endif
#if ALCHEMY_INSPECTOR
        [TabGroup(ImporterTab, ImporterTab)]
        [InlineEditor]
#endif
        public SpreadsheetHandler sheetsItemsHandler = new();

        [Space(20)]
#if ODIN_INSPECTOR
        [TabGroup(ImporterTab, SettingsTab)] 
        [InlineProperty] 
        [HideLabel]
#endif
#if ALCHEMY_INSPECTOR
        [TabGroup(ImporterTab, SettingsTab)]
        [InlineEditor]
        [Order(0)]
#endif
        public GoogleSpreadsheetSettings settings = new();

        #endregion

        #region private data

        private LifeTime _lifeTime = new();

        private GoogleSpreadsheetClient _sheetClient;

        #endregion

        #region public properties

        public bool IsValidToConnect => settings.sheets.Any(x => !string.IsNullOrEmpty(x.id));

        public bool AutoConnect => settings.autoConnect;
        
        public bool HasConnectedSheets => Client.IsConnected && 
                                          Client.Status!=null &&
                                          Client.Status.HasConnectedSheets;

        public ILifeTime LifeTime => (_lifeTime ??= new LifeTime());

        public IGoogleSpreadsheetClient Client => (_sheetClient ??= CreateClient());

        public IGooglsSpreadsheetClientStatus Status => Client.Status;

        #endregion

        #region public methods

#if ODIN_INSPECTOR
        [PropertyOrder(-1)]
        [ResponsiveButtonGroup("Commands")]
        [Button("Reconnect", ButtonSizes.Large, Icon = SdfIconType.SendCheck)]
        [EnableIf(nameof(IsValidToConnect))]
#endif
#if ALCHEMY_INSPECTOR
        [HorizontalGroup(ButtonsGroup)]
        [Button]
        [Order(0)]
#endif
        public void Reconnect()
        {
            _lifeTime?.Restart();

            Client.Connect(settings.user, settings.credentialsPath);

            LifeTime.AddDispose(Client);

            ReloadSpreadsheetsData();

            sheetsItemsHandler.Initialize(Client);
        }

#if ODIN_INSPECTOR
        [PropertyOrder(-1)]
        [ResponsiveButtonGroup("Commands")]
        [Button("Reset", ButtonSizes.Large, Icon = SdfIconType.Eraser)]
#endif
#if ALCHEMY_INSPECTOR
        [HorizontalGroup(ButtonsGroup)]
        [Button]
#endif
        public void ResetCredentials()
        {
            if (Directory.Exists(GoogleSpreadsheetConstants.TokenKey))
                Directory.Delete(GoogleSpreadsheetConstants.TokenKey, true);

            _lifeTime?.Restart();
        }

#if ODIN_INSPECTOR
        [PropertyOrder(-1)]
        [Button("Import All", ButtonSizes.Small, Icon = SdfIconType.CloudDownloadFill)]
        [ResponsiveButtonGroup("importers/importers/commands",DefaultButtonSize = ButtonSizes.Small)]
        [EnableIf(nameof(HasConnectedSheets))]
#endif
#if ALCHEMY_INSPECTOR
        [HorizontalGroup(ButtonsGroup)]
        //[EnableIf(nameof(HasConnectedSheets))]
        [Button]
#endif
        public void Import()
        {
            if (!HasConnectedSheets) return;
            
            //AssetDatabase.StartAssetEditing();
            try
            {
                sheetsItemsHandler.Import();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                //AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

#if ODIN_INSPECTOR
        [Button("Export All", ButtonSizes.Small, Icon = SdfIconType.CloudUploadFill)]
        [ResponsiveButtonGroup("importers/importers/commands")]
        [EnableIf(nameof(HasConnectedSheets))]
#endif
#if ALCHEMY_INSPECTOR
        [HorizontalGroup(ButtonsGroup)]
        //[EnableIf(nameof(HasConnectedSheets))]
        [Button]
#endif
        public void Export()
        {
            if (!HasConnectedSheets) return;
            
            AssetDatabase.StartAssetEditing();
            try
            {
                sheetsItemsHandler.Export();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

#if ODIN_INSPECTOR
        [Button("Show Sheets", ButtonSizes.Small, Icon = SdfIconType.Folder2Open)]
        [ResponsiveButtonGroup("importers/importers/commands")]
        [EnableIf(nameof(HasConnectedSheets))]
#endif
        public void ShowSpreadSheets()
        {
#if ODIN_INSPECTOR
            GoogleSpreadSheetViewWindow.Open(Client.Sheets);
#endif
        }

#if ODIN_INSPECTOR
        //[ButtonGroup()]
        //[Button("Reload Spreadsheets")]
        //[EnableIf(nameof(HasConnectedSheets))]
#endif
        public void ReloadSpreadsheetsData()
        {
            ReconnectToSpreadsheets();
            //Client.ReloadAll();
        }

        #endregion

        #region private methods

        private void ReconnectToSpreadsheets()
        {
            foreach (var sheet in settings.sheets)
            {
                if (string.IsNullOrEmpty(sheet.id))
                    continue;
                Client.ConnectToSpreadsheet(sheet.id);
            }
        }


        private GoogleSpreadsheetClient CreateClient()
        {
            var clientData = new SpreadsheetClientData()
            {
                user = settings.user,
                credentialsPath = settings.credentialsPath,
                appName = GoogleSpreadsheetConstants.ApplicationName,
                scope = GoogleSpreadsheetConnection.WriteScope,
                timeout = settings.authTimeout
            };
            
            var client = new GoogleSpreadsheetClient(clientData)
                .AddTo(LifeTime);

            return client;
        }

        #endregion
    }
}