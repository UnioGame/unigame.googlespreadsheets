namespace UniGame.GoogleSpreadsheets.Editor.CoProcessors
{
    using System;
    using System.Collections.Generic;
    using UniModules.Editor;
    using Abstract;
    using UnityEngine;
    using System.Data;
    using UniModules;

    [CreateAssetMenu(menuName = "UniGame/Google/CoProcessors/CoProcessor", fileName = nameof(CoProcessor))]
    public class CoProcessor : ScriptableObject, ICoProcessorHandle
    {
        #region static data

        private static string _defaultCoProcessorPath;

        private static string DefaultCoProcessorPath => _defaultCoProcessorPath =
            string.IsNullOrEmpty(_defaultCoProcessorPath)
                ? FileUtils.Combine(EditorPathConstants.GeneratedContentPath,
                    "GoogleSheetImporter/Editor/CoProcessors/")
                : _defaultCoProcessorPath;

        private static CoProcessor _processor;

        public static CoProcessor Processor
        {
            get
            {
                if (_processor)
                    return _processor;

                _processor = AssetEditorTools.GetAsset<CoProcessor>();
                if (!_processor)
                {
                    _processor = CreateInstance<CoProcessor>();
                    _processor.ResetToDefault();
                    _processor.SaveAsset(nameof(CoProcessor), DefaultCoProcessorPath);
                }

                return _processor;
            }
        }

        #endregion
        
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ListDrawerSettings(Expanded = true)]
#endif
        [SerializeReference]
        private List<ICoProcessorHandle> _processors = new List<ICoProcessorHandle>();

        public void Apply(SheetValueInfo valueInfo, DataRow row)
        {
            if(valueInfo == null)
                throw new ArgumentNullException(nameof(valueInfo));

            foreach (var coProcessor in _processors)
            {
                coProcessor.Apply(valueInfo, row);
            }
        }

        [ContextMenu(nameof(ResetToDefault))]
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        public void ResetToDefault()
        {
            _processors.Clear();
            _processors.Add(new NestedTableCoProcessor());
        }
    }
}