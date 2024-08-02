using System.Collections.Generic;
using MageTest.Core.CombatSystem;
using UnityEngine;
using Zenject;

namespace MageTest.Core.Factories
{
    public class SpellFactory
    {
        [Inject]
        private readonly DiContainer _diContainer;
        
        private static readonly List<TypeValuePair> list = new(0);
        
        public Spell Create(Spell spell)
        {
            var instance = Object.Instantiate(spell);
            _diContainer.InjectExplicit(instance, list);
            return instance;
        }
    }
}