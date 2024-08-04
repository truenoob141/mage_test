using System;
using MageTest.Core.CombatSystem;
using MageTest.Core.Configs;
using MageTest.ResourceManagement;
using UnityEngine;

namespace MageTest
{
    [Serializable]
    public class GameSettings
    {
        public int _maxEnemies = 10;
        public int _maxSpells = 3;
        public Vector2 _enemySpawnDelayRange = new Vector2(1f, 3f);
        public ViewAssetReferenceCollection _viewAssetRefCollection;
        public PlayerConfig _playerConfig;
        public Spell[] _availableSpells;
        public EnemyConfig[] _enemyConfigs;
        
    }
}