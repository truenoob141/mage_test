using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

namespace MageTest.ResourceManagement
{
    public abstract class AssetReferenceCollectionBase : ScriptableObject
    {
        [SerializeField]
        protected AssetReferenceGameObject[] _assetRefs;
        [SerializeField]
        protected string[] _types;

#if UNITY_EDITOR
        [SerializeField]
        private AddressableAssetGroup _assetGroup;

        protected abstract Type GetComponentType();
        
        [ContextMenu("Update AssetTypes")]
        private void UpdateAssetTypes()
        {
            _types = new string[_assetRefs.Length];

            var componentType = GetComponentType();
            for (int index = 0; index < _assetRefs.Length; index++)
            {
                var state = _assetRefs[index];
                var component = state.editorAsset.GetComponent(componentType);
                if (component == null)
                {
                    Debug.LogError($"Failed to update asset {state.editorAsset.name}: " +
                        $"Component {componentType.FullName} not found");

                    _types[index] = null;
                    continue;
                }

                _types[index] = component.GetType().FullName;
            }
        }

        [ContextMenu("Refresh Group")]
        private void RefreshGroup()
        {
            if (_assetGroup == null)
                return;

            var list = new List<AssetReference>();
            foreach (var prefab in _assetRefs)
            {
                if (prefab == null)
                    continue;

                var entry = AddressableAssetSettingsDefaultObject.Settings
                    .CreateOrMoveEntry(prefab.AssetGUID, _assetGroup);

                if (entry == null)
                    continue;

                list.Add(prefab);
            }

            foreach (var entry in _assetGroup.entries.ToList()
                         .Where(entry => _assetRefs.FirstOrDefault(x => x.AssetGUID == entry.guid) == null))
                AddressableAssetSettingsDefaultObject.Settings.RemoveAssetEntry(entry.guid);
        }

        [ContextMenu("Sort")]
        private void Sort()
        {
            if (_assetGroup == null)
                return;

            var sorted = _assetRefs.Where(a => a != null)
                .OrderBy(a => a.editorAsset.name)
                .ToArray();

            var serializedObject = new SerializedObject(this);

            serializedObject.Update();

            _assetRefs = sorted;

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(this);
        }

        [CustomEditor(typeof(AssetReferenceCollectionBase), true)]
        public class AssetReferenceCollectionBaseEditor : Editor
        {
            private AssetReferenceCollectionBase _target;

            private void OnEnable()
            {
                _target = (AssetReferenceCollectionBase) target;
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("Update Assets"))
                {
                    _target.UpdateAssetTypes();
                }

                if (GUILayout.Button("Refresh Group"))
                {
                    _target.RefreshGroup();
                }

                if (GUILayout.Button("Sort"))
                {
                    _target.Sort();
                }
            }
        }
#endif
    }
}