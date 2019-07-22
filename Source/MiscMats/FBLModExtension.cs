using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace MiscMats
{
    class FBLModExtension : DefModExtension
    {
        public int maxCapacity, baseDuration;
        public Color barColorEmpty = new Color(0.4f, 0.27f, 0.22f);
        public Color barColor = new Color(0.9f, 0.85f, 0.2f);
        public Color barColorBg = new Color(0.3f, 0.3f, 0.3f);
        public bool canBeRuinedByTemperature = false;
    }
}
