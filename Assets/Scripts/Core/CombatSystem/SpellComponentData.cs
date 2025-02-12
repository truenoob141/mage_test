﻿using System.Collections.Generic;
using MageTest.Core.Interfaces;
using UnityEngine;

namespace MageTest.Core.CombatSystem
{
    /// <summary>
    /// Pipeline data for spell components
    /// </summary>
    public class SpellComponentData
    {
        public IEntity Source { get; }
        public IEntity Target => _targets.Count > 0 ? _targets[0] : null;
        public Vector3? SourcePosition { get; }
        public Vector3? TargetPosition { get; }
        public int Damage { get; private set; }

        public int TargetNumber => _targets.Count;

        private readonly List<IEntity> _targets = new(1);
        
        public SpellComponentData(IEntity source, Vector3 targetPosition)
        {
            Source = source;
            TargetPosition = targetPosition;
        }

        public SpellComponentData(
            IEntity source,
            IEntity target)
        {
            Source = source;
            
            _targets.Add(target);
        }

        public SpellComponentData(
            Vector3 sourcePosition,
            Vector3 targetPosition)
        {
            SourcePosition = sourcePosition;
            TargetPosition = targetPosition;
        }

        public IEnumerable<IEntity> GetTargets(bool onlyValid = true)
        {
            if (Target != null && (!onlyValid || Target.IsValid))
                yield return Target;
        }
        
        public void AddTarget(IEntity target)
        {
            _targets.Add(target);
        }

        public void ClearTargets()
        {
            _targets.Clear();
        }

        public void AddDamage(int damage)
        {
            Damage += damage;
        }
    }
}