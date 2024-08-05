using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MageTest.Core.Interfaces;
using UnityEngine;

namespace MageTest.Core.CombatSystem
{
    [CreateAssetMenu(fileName = "AddTargetSelf", menuName = "Spell Components/Add Target Self")]
    public class AddTargetSelfSpellComponent : SpellComponent
    {
        [SerializeField]
        private bool _clearTargets;
        
        public override UniTask<SpellComponentData> Create(SpellComponentData data, CancellationToken token)
        {
            if (_clearTargets)
                data.ClearTargets();

            if (Helper.IsValid(data.Source))
                data.AddTarget(data.Source);
            
            return UniTask.FromResult(data);
        }
    }
}