using MageTest.Core.CombatSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MageTest.Core.Configs
{
    [CreateAssetMenu(fileName = "Enemy", menuName = "Configs/Enemy")]
    public class EnemyConfig : ScriptableObject
    {
        public AssetReferenceGameObject _prefabRef;
        public int _health = 100;
        [Range(0, 100)]
        public int _defense = 0;
        public float _moveSpeed = 5f;
        public float _spellDelay = 0.3f;
        public Spell _spell;
    }
}