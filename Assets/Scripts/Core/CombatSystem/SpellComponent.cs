using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MageTest.Core.CombatSystem
{
    public abstract class SpellComponent : ScriptableObject
    {
        public virtual SpellComponentData Prepare(SpellComponentData data)
        {
            return data;
        }

        public abstract UniTask<SpellComponentData> Create(SpellComponentData data, CancellationToken token);
    }
}