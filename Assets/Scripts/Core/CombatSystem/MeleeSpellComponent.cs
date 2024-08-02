using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MageTest.Core.Interfaces;
using UnityEngine;

namespace MageTest.Core.CombatSystem
{
    [CreateAssetMenu(fileName = "Melee", menuName = "Spell Components/Melee")]
    public class MeleeSpellComponent : SpellComponent
    {
        public override UniTask<SpellComponentData> Create(SpellComponentData data, CancellationToken token)
        {
            int damage = data.Damage;
            foreach (var target in data.GetTargets().OfType<IAliveEntity>())
                target.TakeDamage(damage);

            if (data.TargetPosition.HasValue)
            {
                // TODO Implement melee attack by position
            }
            
            return UniTask.FromResult(data);
        }
    }
}