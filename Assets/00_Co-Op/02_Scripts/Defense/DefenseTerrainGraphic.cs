using UnityEngine;

namespace OpenFrame
{
    /// <summary>Quiet illustrated terrain behind the playable board, independent of unit artwork.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class DefenseTerrainGraphic : UnityEngine.UI.MaskableGraphic
    {
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear();
            Polygon(mesh, new[] { new Vector2(0,0), new Vector2(900,0), new Vector2(900,1800), new Vector2(0,1800) }, new Color(0.28f, 0.39f, 0.28f));
            Polygon(mesh, new[] { new Vector2(42,280), new Vector2(865,280), new Vector2(874,460), new Vector2(832,640), new Vector2(867,870), new Vector2(851,1100), new Vector2(860,1350), new Vector2(811,1660), new Vector2(730,1750), new Vector2(210,1730), new Vector2(70,1660), new Vector2(37,1460), new Vector2(65,1230), new Vector2(32,970), new Vector2(53,760), new Vector2(29,540) }, new Color(0.65f, 0.49f, 0.34f));
            Polygon(mesh, new[] { new Vector2(52,285), new Vector2(851,285), new Vector2(859,457), new Vector2(820,640), new Vector2(853,870), new Vector2(837,1100), new Vector2(846,1350), new Vector2(800,1652), new Vector2(725,1736), new Vector2(215,1717), new Vector2(82,1650), new Vector2(51,1460), new Vector2(79,1230), new Vector2(46,970), new Vector2(67,760), new Vector2(43,540) }, new Color(0.91f, 0.74f, 0.55f));
            var random = new System.Random(204);
            for (int i = 0; i < 52; i++)
            {
                float x = i % 2 == 0 ? -20 + (float)random.NextDouble() * 53 : 867 + (float)random.NextDouble() * 53;
                float y = 320 + (float)random.NextDouble() * 1490;
                float radius = 25 + (float)random.NextDouble() * 43;
                Ellipse(mesh, x + 4, y - 9, radius, radius * 0.75f, new Color(0.19f, 0.26f, 0.20f));
                Ellipse(mesh, x, y, radius, radius * 0.75f, new Color(0.23f, 0.34f, 0.24f));
                Ellipse(mesh, x - 5, y + 5, radius * 0.88f, radius * 0.61f, i % 3 == 0 ? new Color(0.39f, 0.50f, 0.30f) : new Color(0.32f, 0.44f, 0.29f));
            }
            for (int i = 0; i < 45; i++)
            {
                float x = 86 + (float)random.NextDouble() * 728;
                float y = 330 + (float)random.NextDouble() * 1350;
                Ellipse(mesh, x, y, 8 + (float)random.NextDouble() * 12, 4 + (float)random.NextDouble() * 6, new Color(0.79f, 0.62f, 0.44f, 0.22f));
            }
        }

        private void Ellipse(UnityEngine.UI.VertexHelper mesh, float x, float y, float w, float h, Color tint)
        {
            var points = new Vector2[12];
            for (int i = 0; i < points.Length; i++)
            {
                float a = i * Mathf.PI * 2 / points.Length;
                points[i] = new Vector2(x + Mathf.Cos(a) * w, y + Mathf.Sin(a) * h);
            }
            Polygon(mesh, points, tint);
        }

        private void Polygon(UnityEngine.UI.VertexHelper mesh, Vector2[] points, Color tint)
        {
            int start = mesh.currentVertCount;
            Vector2 center = Vector2.zero;
            foreach (var point in points) center += point;
            center /= points.Length;
            Add(mesh, center, tint);
            foreach (var point in points) Add(mesh, point, tint);
            for (int i = 0; i < points.Length; i++) mesh.AddTriangle(start, start + i + 1, start + (i + 1) % points.Length + 1);
        }

        private void Add(UnityEngine.UI.VertexHelper mesh, Vector2 logical, Color tint)
        {
            var r = rectTransform.rect;
            mesh.AddVert(new Vector3(r.xMin + logical.x / 900 * r.width, r.yMin + logical.y / 1800 * r.height), tint, Vector2.zero);
        }
    }
}

