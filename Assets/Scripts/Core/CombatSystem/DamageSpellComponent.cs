using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MageTest.Core.CombatSystem
{
    [CreateAssetMenu(fileName = "Damage", menuName = "Spell Components/Damage")]
    public class DamageSpellComponent : SpellComponent
    {
        [SerializeField]
        private int _damage = 1;

        public override UniTask<SpellComponentData> Create(SpellComponentData data, CancellationToken token)
        {
            data.AddDamage(_damage);
            return UniTask.FromResult(data);
        }
    }
}