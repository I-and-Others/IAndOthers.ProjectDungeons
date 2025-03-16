using System;
using UnityEngine;

namespace Scripts.Entities.Class
{
    public class OnAIAttackEventArgs : EventArgs
    {
        public Character Attacker { get; set; }
        public Character Target { get; set; }
        public int SkillIndex { get; set; }
        public int Damage { get; set; }
        public SkillType SkillType { get; set; }
    }
} 