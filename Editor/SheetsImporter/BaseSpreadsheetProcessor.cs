namespace UniGame.GoogleSpreadsheets.Editor
{
    using Core.Runtime;
    using UniGame.Runtime.DataFlow;
    using UnityEngine;
#if ALCHEMY_INSPECTOR
    using Alchemy.Inspector;
#endif
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

    public abstract class BaseSpreadsheetProcessor : ScriptableObject, ISpreadsheetProcessor
    {
        public const string CommandsGroup = "commands";
        
        public string importerName = string.Empty;

        private IGoogleSpreadsheetClient _client;
        private IGooglsSpreadsheetClientStatus _status;
        
        private LifeTimeDefinition _lifeTimeDefinition = new();

        #region public properties

        public virtual string Name => string.IsNullOrEmpty(importerName) ? name : importerName;

        protected ILifeTime LifeTime => _lifeTimeDefinition;

        public IGoogleSpreadsheetClient Client => _client;

        public bool IsValidData =>  _client!=null && 
                                    _status != null && 
                                    _status.HasConnectedSheets;

        public abstract bool CanImport { get; }
        public abstract bool CanExport { get; }

        #endregion

        public virtual void Start()
        {
        }

        public virtual void Initialize(IGoogleSpreadsheetClient client)
        {
            Reset();

            _client = client;
            _status = client.Status;

            OnInitialize(client);
        }

        public void Reset()
        {
            _client = null;
            _status = null;

            _lifeTimeDefinition.Terminate();
            _lifeTimeDefinition = new LifeTimeDefinition();
        }

        public ISpreadsheetData Import(ISpreadsheetData spreadsheetData)
        {
            return ImportObjects(spreadsheetData);
        }

        public ISpreadsheetData Export(ISpreadsheetData data)
        {
            return ExportObjects(data);
        }

#if ODIN_INSPECTOR
        [ButtonGroup]
        [Button(ButtonSizes.Small,Icon = SdfIconType.CloudDownload)]
        [EnableIf(nameof(IsValidData))]
        [ShowIf(nameof(CanImport))]
#endif
#if ALCHEMY_INSPECTOR
        [HorizontalGroup(CommandsGroup)]
        [EnableIf(nameof(IsValidData))]
        [ShowIf(nameof(CanImport))]
        [Button]
#endif
        public void Import()
        {
            if (IsValidData == false) return;
            Import(_client.SpreadsheetData);
        }

#if ODIN_INSPECTOR
        [ButtonGroup]
        [Button(ButtonSizes.Small,Icon = SdfIconType.CloudUpload)]
        [EnableIf(nameof(IsValidData))]
        [ShowIf(nameof(CanExport))]
#endif
#if ALCHEMY_INSPECTOR
        [HorizontalGroup(CommandsGroup)]
        [EnableIf(nameof(IsValidData))]
        [ShowIf(nameof(CanExport))]
        [Button]
#endif
        public void Export()
        {
            if (IsValidData == false) return;
            Export(_client.SpreadsheetData);
            
            if (_client.IsConnected) 
                _client.UploadAll();
        }

        public abstract ISpreadsheetData ImportObjects(ISpreadsheetData spreadsheetData);

        public abstract ISpreadsheetData ExportObjects(ISpreadsheetData spreadsheetData);

        public virtual string FormatName(string assetName) => assetName;
        
        protected virtual void OnReset() {}
        
        protected virtual void OnInitialize(IGoogleSpreadsheetClient client) {}
    }
}