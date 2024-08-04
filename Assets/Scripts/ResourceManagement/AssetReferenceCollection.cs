using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MageTest.ResourceManagement
{
    public abstract class AssetReferenceCollection<TComponentType> : AssetReferenceCollectionBase
    {
        public AssetReferenceGameObject GetReference<TState>() where TState : MonoBehaviour
        {
            return GetReference(typeof(TState));
        }

        public AssetReferenceGameObject GetReference(Type type)
        {
            string fullName = type.FullName;
            int index = Array.FindIndex(_types, t => t == fullName);
            return index != -1 ? _assetRefs[index] : null;
        }

        protected override Type GetComponentType()
        {
            return typeof(TComponentType);
        }
    }
}