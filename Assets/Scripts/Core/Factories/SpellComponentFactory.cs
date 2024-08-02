using System.Collections.Generic;
using MageTest.Core.CombatSystem;
using UnityEngine;
using Zenject;

namespace MageTest.Core.Factories
{
    public class SpellComponentFactory
    {
        [Inject]
        private readonly DiContainer _diContainer;
        
        public SpellComponent[] Create(SpellComponent[] source)
        {
            var list = new List<TypeValuePair>(0);
            
            var instances = new SpellComponent[source.Length];
            for (int i = 0; i < instances.Length; ++i)
            {
                var instance = Object.Instantiate(source[i]);
                _diContainer.InjectExplicit(instance, list);
                instances[i] = instance;
            }

            return instances;
        }
    }
}