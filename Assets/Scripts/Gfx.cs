using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TokyoRunner
{
    public static class Gfx
    {
        static readonly Dictionary<PrimitiveType, Mesh> meshes = new Dictionary<PrimitiveType, Mesh>();

        static Mesh GetMesh(PrimitiveType t)
        {
            Mesh m;
            if (!meshes.TryGetValue(t, out m) || m == null)
            {
                GameObject g = GameObject.CreatePrimitive(t);
                m = g.GetComponent<MeshFilter>().sharedMesh;
                Object.Destroy(g);
                meshes[t] = m;
            }
            return m;
        }

        public static GameObject Prim(PrimitiveType t, Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject g = new GameObject(t.ToString());
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            g.transform.localScale = scale;
            g.AddComponent<MeshFilter>().sharedMesh = GetMesh(t);
            MeshRenderer r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            return g;
        }

        public static GameObject Box(Transform p, Vector3 pos, Vector3 scale, Material m) { return Prim(PrimitiveType.Cube, p, pos, scale, m); }
        public static GameObject Sphere(Transform p, Vector3 pos, Vector3 scale, Material m) { return Prim(PrimitiveType.Sphere, p, pos, scale, m); }
        public static GameObject Cyl(Transform p, Vector3 pos, Vector3 scale, Material m) { return Prim(PrimitiveType.Cylinder, p, pos, scale, m); }

        public static Light PointLight(Transform parent, Vector3 pos, Color c, float intensity, float range)
        {
            GameObject g = new GameObject("Light");
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            Light l = g.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = c;
            l.intensity = intensity * Mt.LightK;
            l.range = range;
            l.shadows = LightShadows.None;
            return l;
        }

        // ---------- neon sign with (Japanese) text, faces the road ----------
        public static void Sign(Transform parent, Vector3 pos, int side, string text, int neonIdx)
        {
            Color col = Mt.NeonCols[neonIdx];
            Material nm = Mt.NeonMats[neonIdx];
            Transform root = new GameObject("Sign").transform;
            root.SetParent(parent, false);
            root.localPosition = pos;
            root.localRotation = Quaternion.Euler(0f, 90f * side, 0f);

            float w = text.Length * (Fonts.Japanese ? 1.7f : 1.15f) + 1.4f;
            float h = 2.4f;
            Box(root, Vector3.zero, new Vector3(w, h, 0.3f), Mt.SignBack);
            Box(root, new Vector3(0f, h / 2f, -0.17f), new Vector3(w, 0.12f, 0.08f), nm);
            Box(root, new Vector3(0f, -h / 2f, -0.17f), new Vector3(w, 0.12f, 0.08f), nm);
            Box(root, new Vector3(-w / 2f, 0f, -0.17f), new Vector3(0.12f, h, 0.08f), nm);
            Box(root, new Vector3(w / 2f, 0f, -0.17f), new Vector3(0.12f, h, 0.08f), nm);

            GameObject go = new GameObject("Text");
            go.transform.SetParent(root, false);
            go.transform.localPosition = new Vector3(0f, 0f, -0.22f);
            TextMesh tm = go.AddComponent<TextMesh>();
            tm.font = Fonts.Face;
            tm.text = text;
            tm.fontSize = 64;
            tm.characterSize = 0.25f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = col;
            go.GetComponent<MeshRenderer>().sharedMaterial = Fonts.Face.material;
        }

        public static void Vending(Transform parent, Vector3 pos, int side)
        {
            Box(parent, pos + new Vector3(0f, 1f, 0f), new Vector3(0.9f, 2f, 1f), Mt.Vend);
            Box(parent, pos + new Vector3(-side * 0.46f, 1.15f, 0f), new Vector3(0.05f, 1.3f, 0.8f), Mt.NeonWhite);
            Box(parent, pos + new Vector3(-side * 0.46f, 0.25f, 0f), new Vector3(0.05f, 0.2f, 0.7f), Mt.NeonCyan);
        }

        public static void Lantern(Transform parent, Vector3 pos)
        {
            Box(parent, new Vector3(pos.x, 1.6f, pos.z), new Vector3(0.1f, 3.2f, 0.1f), Mt.Pole);
            Sphere(parent, new Vector3(pos.x, 3.15f, pos.z), new Vector3(0.6f, 0.8f, 0.6f), Mt.NeonRed);
            Box(parent, new Vector3(pos.x, 3.6f, pos.z), new Vector3(0.32f, 0.08f, 0.32f), Mt.Dark);
        }

        public static void Torii(Transform parent, float z)
        {
            Box(parent, new Vector3(-5.2f, 3.25f, z), new Vector3(0.7f, 6.5f, 0.7f), Mt.ToriiRed);
            Box(parent, new Vector3(5.2f, 3.25f, z), new Vector3(0.7f, 6.5f, 0.7f), Mt.ToriiRed);
            Box(parent, new Vector3(0f, 6.75f, z), new Vector3(13.6f, 0.5f, 1.1f), Mt.ToriiBlack);
            Box(parent, new Vector3(0f, 5.3f, z), new Vector3(10.8f, 0.4f, 0.6f), Mt.ToriiRed);
            Box(parent, new Vector3(0f, 6.0f, z), new Vector3(0.5f, 0.7f, 0.5f), Mt.ToriiRed);
            Box(parent, new Vector3(0f, 6.45f, z - 0.58f), new Vector3(13.2f, 0.07f, 0.04f), Mt.NeonOrange);
            Box(parent, new Vector3(0f, 5.05f, z - 0.33f), new Vector3(10.6f, 0.05f, 0.04f), Mt.NeonPink);
        }

        // ---------- Tokyo Tower in the distance ----------
        static void TowerLevel(Transform root, float yBase, float h, float w, Material m)
        {
            float pw = Mathf.Max(0.8f, w * 0.06f);
            float yc = yBase + h / 2f;
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    Box(root, new Vector3(sx * w / 2f, yc, sz * w / 2f), new Vector3(pw, h, pw), m);
            float yt = yBase + h;
            float bw = Mathf.Max(0.6f, w * 0.04f);
            Box(root, new Vector3(0f, yt, w / 2f), new Vector3(w, bw, bw), m);
            Box(root, new Vector3(0f, yt, -w / 2f), new Vector3(w, bw, bw), m);
            Box(root, new Vector3(w / 2f, yt, 0f), new Vector3(bw, bw, w), m);
            Box(root, new Vector3(-w / 2f, yt, 0f), new Vector3(bw, bw, w), m);
        }

        public static Transform Tower()
        {
            Transform root = new GameObject("TokyoTower").transform;
            Material o = Mt.NeonOrange, w = Mt.NeonWhite;
            float[] widths = { 34f, 28f, 22f, 17f, 13f };
            for (int i = 0; i < widths.Length; i++)
                TowerLevel(root, i * 12f, 12f, widths[i], (i % 2 == 0) ? o : w);
            Box(root, new Vector3(0f, 62f, 0f), new Vector3(24f, 4f, 24f), w);          // main deck
            Box(root, new Vector3(0f, 62f, 0f), new Vector3(22f, 1.2f, 22f), Mt.NeonPink);
            for (int i = 0; i < 3; i++)
                TowerLevel(root, 64f + i * 10f, 10f, 9f - i * 1.8f, (i % 2 == 0) ? w : o);
            Box(root, new Vector3(0f, 100f, 0f), new Vector3(12f, 3f, 12f), o);        // top deck
            Box(root, new Vector3(0f, 125f, 0f), new Vector3(2.4f, 46f, 2.4f), w);      // antenna
            Box(root, new Vector3(0f, 140f, 0f), new Vector3(2.6f, 8f, 2.6f), o);
            Box(root, new Vector3(0f, 150f, 0f), new Vector3(1.2f, 1.2f, 1.2f), Mt.NeonRed);
            root.localScale = Vector3.one * 0.8f;
            return root;
        }
    }
}
