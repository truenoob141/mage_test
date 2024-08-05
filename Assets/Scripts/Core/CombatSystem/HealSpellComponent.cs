using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MageTest.Core.Interfaces;
using UnityEngine;

namespace MageTest.Core.CombatSystem
{
    [CreateAssetMenu(fileName = "Heal", menuName = "Spell Components/Heal")]
    public class HealSpellComponent : SpellComponent
    {
        [SerializeField]
        private int _amount = 5;

        public override UniTask<SpellComponentData> Create(SpellComponentData data, CancellationToken token)
        {
            foreach (var target in data.GetTargets().OfType<IAliveEntity>())
            {
                if (Helper.IsValid(target))
                    target.Heal(_amount);
            }
            
            return UniTask.FromResult(data);
        }
    }
}