using UnityEngine;

namespace MageTest.Core.Configs
{
    [CreateAssetMenu(fileName = "Player", menuName = "Configs/Player")]
    public class PlayerConfig : ScriptableObject
    {
        public int _health = 100;
        [Range(0, 100)]
        public int _defense = 30;
        public float _moveSpeed = 10f;
        public float _spellDelay = 0.3f;
    }
}