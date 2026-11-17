using UnityEngine;

namespace OpenFrame
{
    /// <summary>Shared stage coordinates keep targeting, movement and presentation on the same track.</summary>
    public static class DefenseArena
    {
        public static Vector2 StageSize => new Vector2(900,2000);
        public static Vector2 CellPosition(int x, int y) => new Vector2(120 + (x + 0.5f) * 66, 490 + (y + 0.5f) * 66);
        public static Vector2 PartnerCellPosition(int x,int y) => CellPosition(x,y)+new Vector2(0,885);

        public static Vector2 LoopPoint(float progress)
        {
            float arc = Mathf.PI * 35;
            float distance = Mathf.Repeat(progress, 1) * (1500 + arc * 4);
            if (distance < 580) return new Vector2(160 + distance, 975);
            distance -= 580;
            if (distance < arc) return Arc(new Vector2(740, 1045), -90 + distance / arc * 90);
            distance -= arc;
            if (distance < 170) return new Vector2(810, 1045 + distance);
            distance -= 170;
            if (distance < arc) return Arc(new Vector2(740, 1215), distance / arc * 90);
            distance -= arc;
            if (distance < 580) return new Vector2(740 - distance, 1285);
            distance -= 580;
            if (distance < arc) return Arc(new Vector2(160, 1215), 90 + distance / arc * 90);
            distance -= arc;
            if (distance < 170) return new Vector2(90, 1215 - distance);
            distance -= 170;
            return Arc(new Vector2(160, 1045), 180 + distance / arc * 90);
        }

        private static Vector2 Arc(Vector2 center, float degrees) => center + new Vector2(Mathf.Cos(degrees * Mathf.Deg2Rad), Mathf.Sin(degrees * Mathf.Deg2Rad)) * 70;
    }

    public readonly struct DefenseShot
    {
        public readonly Vector2Int Cell;
        public readonly Vector2 Target;
        public readonly int Element;
        public readonly float Damage;
        public DefenseShot(Vector2Int cell, Vector2 target, int element, float damage)
        { Cell = cell; Target = target; Element = element; Damage = damage; }
    }
}

