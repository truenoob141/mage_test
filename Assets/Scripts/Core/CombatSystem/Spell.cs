using System.Threading;
using Cysharp.Threading.Tasks;
using MageTest.Core.Factories;
using MageTest.Core.Interfaces;
using UnityEngine;
using Zenject;

namespace MageTest.Core.CombatSystem
{
    [CreateAssetMenu(fileName = "Spell", menuName = "Spell")]
    public sealed class Spell : ScriptableObject
    {
        [Inject]
        private readonly SpellComponentFactory _spellComponentFactory;

        [SerializeField]
        private Sprite _icon;
        [SerializeField]
        private SpellComponent[] _spellComponents;

        public UniTask Cast(IEntity source, Vector3 targetPos, CancellationToken token)
        {
            var data = new SpellComponentData(source, targetPos);
            return Cast(data, token);
        }
        
        public UniTask Cast(IEntity source, IEntity victim, CancellationToken token)
        {
            var data = new SpellComponentData(source, victim);
            return Cast(data, token);
        }
        
        public Sprite GetIcon()
        {
            return _icon;
        }
        
        private async UniTask Cast(SpellComponentData data, CancellationToken token)
        {
            var components = _spellComponentFactory.Create(_spellComponents);
            foreach (var comp in components)
                data = comp.Prepare(data);

            foreach (var comp in components)
                data = await comp.Create(data, token);
        }
    }
}