using System.Collections.Generic;
using UnityEngine;

namespace TokyoRunner
{
    public class Hazard { public float x, z, hw, hl, yMin, yMax; }
    public class CoinItem { public Transform t; public float x, y, z; public bool taken; }

    /// <summary>One 40 m slice of the Tokyo street: road, buildings, signs, props and obstacles.</summary>
    public class Segment : MonoBehaviour
    {
        public const float Length = 40f;
        public const float LaneW = 2.5f;

        public int Index;
        public float ZStart { get { return Index * Length; } }
        public readonly List<Hazard> Hazards = new List<Hazard>();
        public readonly List<CoinItem> Coins = new List<CoinItem>();

        public static Segment Create(int index, float diff)
        {
            GameObject go = new GameObject("Segment " + index);
            go.transform.position = new Vector3(0f, 0f, index * Length);
            Segment s = go.AddComponent<Segment>();
            s.Index = index;
            s.Generate(diff);
            return s;
        }

        void Update()
        {
            float r = 220f * Time.deltaTime;
            for (int i = 0; i < Coins.Count; i++)
                if (!Coins[i].taken && Coins[i].t != null) Coins[i].t.Rotate(0f, r, 0f);
        }

        static float LaneX(int lane) { return (lane - 1) * LaneW; }

        void Generate(float diff)
        {
            Transform T = transform;

            // --- road, edges, lane dashes, sidewalks ---
            Gfx.Box(T, new Vector3(0f, -0.1f, Length / 2f), new Vector3(9f, 0.2f, Length), Mt.Road);
            Gfx.Box(T, new Vector3(-4.3f, 0.03f, Length / 2f), new Vector3(0.12f, 0.06f, Length), Mt.NeonPink);
            Gfx.Box(T, new Vector3(4.3f, 0.03f, Length / 2f), new Vector3(0.12f, 0.06f, Length), Mt.NeonCyan);
            for (int i = 0; i < 6; i++)
            {
                float zz = 3f + i * 6.6f;
                Gfx.Box(T, new Vector3(-1.25f, 0.012f, zz), new Vector3(0.12f, 0.02f, 3f), Mt.NeonDash);
                Gfx.Box(T, new Vector3(1.25f, 0.012f, zz), new Vector3(0.12f, 0.02f, 3f), Mt.NeonDash);
            }
            Gfx.Box(T, new Vector3(-6f, 0.15f, Length / 2f), new Vector3(3f, 0.3f, Length), Mt.Side);
            Gfx.Box(T, new Vector3(6f, 0.15f, Length / 2f), new Vector3(3f, 0.3f, Length), Mt.Side);

            // --- buildings + neon signs on both sides ---
            string[] signs = Fonts.Signs;
            for (int side = -1; side <= 1; side += 2)
            {
                for (int b = 0; b < 3; b++)
                {
                    float w = 12.6f;
                    float zc = (b + 0.5f) * (Length / 3f);
                    float depth = Random.Range(9f, 16f);
                    float gap = Random.Range(0f, 1.2f);
                    float faceX = side * (7.5f + gap);
                    float xc = side * (7.5f + gap + depth / 2f);
                    int hIdx = Random.Range(0, Mt.FacadeH.Length);
                    float h = Mt.FacadeH[hIdx];
                    int variant = Random.Range(0, 4);

                    Gfx.Box(T, new Vector3(xc, h / 2f, zc), new Vector3(depth, h, w), Mt.Facade[variant, hIdx]);
                    Gfx.Box(T, new Vector3(xc, h + 1f, zc), new Vector3(depth * 0.45f, 2f, w * 0.4f), Mt.Roof);
                    if (h > 30f) Gfx.Box(T, new Vector3(xc, h + 2.6f, zc), new Vector3(0.5f, 0.5f, 0.5f), Mt.NeonRed);

                    if (Random.value < 0.7f)
                    {
                        float sy = Random.Range(5f, Mathf.Min(h - 3f, 17f));
                        Gfx.Sign(T, new Vector3(faceX - side * 0.15f, sy, zc + Random.Range(-2f, 2f)), side,
                                 signs[Random.Range(0, signs.Length)], Random.Range(0, Mt.NeonMats.Length));
                    }
                    if (Random.value < 0.35f)
                    {
                        float sy2 = Random.Range(3f, 9f);
                        Gfx.Sign(T, new Vector3(faceX - side * 0.15f, sy2, zc + Random.Range(-3f, 3f)), side,
                                 signs[Random.Range(0, signs.Length)], Random.Range(0, Mt.NeonMats.Length));
                    }
                }

                // street props
                Gfx.Lantern(T, new Vector3(side * 5.1f, 0f, 8f));
                Gfx.Lantern(T, new Vector3(side * 5.1f, 0f, 28f));
                if (Random.value < 0.55f) Gfx.Vending(T, new Vector3(side * 7.0f, 0.3f, Random.Range(3f, 37f)), side);
            }

            if (Index % 4 == 3) Gfx.Torii(T, Length / 2f);

            // colored street lighting for the lit objects
            Gfx.PointLight(T, new Vector3(-5f, 6f, Length / 2f), Pal.Pink, 2.5f, 26f);
            Gfx.PointLight(T, new Vector3(5f, 6f, Length / 2f), Pal.Cyan, 2.5f, 26f);

            // --- obstacles and coins (a grace period at the start) ---
            if (Index >= 2)
            {
                PlaceRow(14f, diff);
                PlaceRow(33f, diff);
                if (Random.value < 0.85f)
                {
                    int lane = Random.Range(0, 3);
                    for (int i = 0; i < 5; i++) AddCoin(LaneX(lane), 1.1f, 20f + i * 1.5f);
                }
            }
        }

        void PlaceRow(float zl, float diff)
        {
            int maxType = diff < 0.25f ? 5 : 8;
            int type = Random.Range(0, maxType);
            int a = Random.Range(0, 3);
            int b = (a + Random.Range(1, 3)) % 3;

            switch (type)
            {
                case 0: Barrier(a, zl); break;
                case 1: Taxi(a, zl); break;
                case 2:
                    Barrier(0, zl); Barrier(1, zl); Barrier(2, zl);
                    float[] arcY = { 1.2f, 2.0f, 2.4f, 2.0f, 1.2f };
                    for (int i = 0; i < 5; i++) AddCoin(0f, arcY[i], zl - 2.4f + i * 1.2f);
                    break;
                case 3: Gantry(zl); break;
                case 4: Taxi(a, zl); Taxi(b, zl); break;
                case 5: Truck(a, zl); break;
                case 6: Taxi(a, zl); Barrier(b, zl); break;
                default: Truck(a, zl); Taxi(b, zl); break;
            }
        }

        void AddHazard(float x, float zl, float hw, float hl, float yMin, float yMax)
        {
            Hazards.Add(new Hazard { x = x, z = ZStart + zl, hw = hw, hl = hl, yMin = yMin, yMax = yMax });
        }

        void AddCoin(float x, float y, float zl)
        {
            Transform root = new GameObject("Coin").transform;
            root.SetParent(transform, false);
            root.localPosition = new Vector3(x, y, zl);
            GameObject disc = Gfx.Cyl(root, Vector3.zero, new Vector3(0.7f, 0.06f, 0.7f), Mt.NeonYellow);
            disc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Coins.Add(new CoinItem { t = root, x = x, y = y, z = ZStart + zl });
        }

        // low striped barrier: jump over it
        void Barrier(int lane, float zl)
        {
            float x = LaneX(lane);
            for (int k = 0; k < 5; k++)
                Gfx.Box(transform, new Vector3(x - 0.8f + k * 0.4f, 0.45f, zl), new Vector3(0.4f, 0.9f, 0.3f), (k % 2 == 0) ? Mt.RedPaint : Mt.White);
            Gfx.Box(transform, new Vector3(x, 0.97f, zl), new Vector3(2f, 0.07f, 0.3f), Mt.NeonOrange);
            AddHazard(x, zl, 1.0f, 0.2f, 0f, 0.95f);
        }

        // yellow taxi: change lane
        void Taxi(int lane, float zl)
        {
            float x = LaneX(lane);
            Transform T = transform;
            Gfx.Box(T, new Vector3(x, 0.5f, zl), new Vector3(1.8f, 0.8f, 4.2f), Mt.Taxi);
            Gfx.Box(T, new Vector3(x, 1.25f, zl - 0.1f), new Vector3(1.55f, 0.7f, 2.2f), Mt.Taxi);
            Gfx.Box(T, new Vector3(x, 1.28f, zl - 0.1f), new Vector3(1.58f, 0.4f, 2.0f), Mt.Glass);
            Gfx.Box(T, new Vector3(x, 1.72f, zl - 0.1f), new Vector3(0.5f, 0.18f, 0.25f), Mt.NeonWhite);
            Gfx.Box(T, new Vector3(x - 0.6f, 0.65f, zl - 2.12f), new Vector3(0.4f, 0.18f, 0.06f), Mt.NeonRed);
            Gfx.Box(T, new Vector3(x + 0.6f, 0.65f, zl - 2.12f), new Vector3(0.4f, 0.18f, 0.06f), Mt.NeonRed);
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    Gfx.Box(T, new Vector3(x + sx * 0.88f, 0.2f, zl + sz * 1.4f), new Vector3(0.2f, 0.4f, 0.6f), Mt.Wheel);
            AddHazard(x, zl, 0.95f, 2.1f, 0f, 1.6f);
        }

        // delivery van: tall, change lane
        void Truck(int lane, float zl)
        {
            float x = LaneX(lane);
            Transform T = transform;
            Gfx.Box(T, new Vector3(x, 1.3f, zl), new Vector3(1.95f, 2.2f, 5.2f), Mt.Van);
            Gfx.Box(T, new Vector3(x, 2.45f, zl), new Vector3(1.97f, 0.08f, 5.0f), Mt.NeonGreen);
            Gfx.Box(T, new Vector3(x - 0.65f, 0.85f, zl - 2.62f), new Vector3(0.4f, 0.2f, 0.06f), Mt.NeonRed);
            Gfx.Box(T, new Vector3(x + 0.65f, 0.85f, zl - 2.62f), new Vector3(0.4f, 0.2f, 0.06f), Mt.NeonRed);
            Gfx.Box(T, new Vector3(x + 0.98f, 1.5f, zl), new Vector3(0.04f, 0.9f, 3.2f), Mt.NeonPink);
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    Gfx.Box(T, new Vector3(x + sx * 0.95f, 0.25f, zl + sz * 1.8f), new Vector3(0.2f, 0.5f, 0.7f), Mt.Wheel);
            AddHazard(x, zl, 1.0f, 2.6f, 0f, 2.5f);
        }

        // overhead gantry across the whole road: slide under it
        void Gantry(float zl)
        {
            Transform T = transform;
            Gfx.Box(T, new Vector3(-4.3f, 1.7f, zl), new Vector3(0.5f, 3.4f, 0.5f), Mt.Pole);
            Gfx.Box(T, new Vector3(4.3f, 1.7f, zl), new Vector3(0.5f, 3.4f, 0.5f), Mt.Pole);
            Gfx.Box(T, new Vector3(0f, 2.1f, zl), new Vector3(9f, 1.5f, 0.8f), Mt.Dark);
            Gfx.Box(T, new Vector3(0f, 1.33f, zl - 0.2f), new Vector3(9f, 0.08f, 0.3f), Mt.NeonPink);
            Gfx.Box(T, new Vector3(0f, 2.1f, zl - 0.42f), new Vector3(8.4f, 0.2f, 0.04f), Mt.NeonYellow);
            AddHazard(0f, zl, 4.5f, 0.5f, 1.3f, 2.9f);
        }
    }
}
