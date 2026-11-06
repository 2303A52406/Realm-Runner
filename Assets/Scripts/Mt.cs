using UnityEngine;

namespace TokyoRunner
{
    public static class Pal
    {
        public static readonly Color Pink = new Color(1f, 0.12f, 0.55f);
        public static readonly Color Cyan = new Color(0.1f, 0.9f, 1f);
        public static readonly Color Purple = new Color(0.65f, 0.3f, 1f);
        public static readonly Color Orange = new Color(1f, 0.5f, 0.1f);
        public static readonly Color Yellow = new Color(1f, 0.85f, 0.2f);
        public static readonly Color Green = new Color(0.2f, 1f, 0.55f);
        public static readonly Color Red = new Color(1f, 0.1f, 0.1f);
        public static readonly Color Fog = new Color(0.05f, 0.03f, 0.12f);
    }

    public static class Fonts
    {
        public static Font Face;
        public static bool Japanese;

        public static readonly string[] SignsJP = { "ラーメン", "居酒屋", "東京", "カラオケ", "寿司", "ネオン", "電気街", "ホテル", "喫茶", "焼肉", "ゲーム" };
        public static readonly string[] SignsEN = { "RAMEN", "IZAKAYA", "TOKYO", "KARAOKE", "SUSHI", "NEON", "AKIBA", "HOTEL", "KISSA", "YAKINIKU", "GAME" };

        public static string[] Signs { get { return Japanese ? SignsJP : SignsEN; } }

        public static void Init()
        {
            Face = null;
            Japanese = false;
            string[] wanted = { "Yu Gothic", "Yu Gothic UI", "Meiryo", "MS Gothic", "Hiragino Sans", "Hiragino Kaku Gothic ProN", "Noto Sans CJK JP", "Noto Sans JP", "IPAGothic", "TakaoGothic" };
            try
            {
                string[] installed = Font.GetOSInstalledFontNames();
                for (int i = 0; i < wanted.Length && Face == null; i++)
                {
                    if (System.Array.IndexOf(installed, wanted[i]) >= 0)
                    {
                        Face = Font.CreateDynamicFontFromOSFont(wanted[i], 64);
                        Japanese = true;
                    }
                }
            }
            catch { Face = null; Japanese = false; }

            if (Face == null)
            {
                try { Face = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
                catch { Face = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
            }

            // pre-load glyphs so signs do not hitch when they first appear
            string all = "";
            string[] list = Signs;
            for (int i = 0; i < list.Length; i++) all += list[i];
            Face.RequestCharactersInTexture(all, 64, FontStyle.Normal);
        }
    }

    public static class Mt
    {
        static Shader litSh, unlitSh, unlitTexSh;
        public static float LightK = 1f;

        public static Material Road, Side, RedPaint, White, Taxi, Glass, Dark, Pole, Vend, Van, Skin, Jacket, Jeans, Pack, Hair, Cap, ToriiRed, ToriiBlack, SignBack, Roof, Wheel;
        public static Material NeonPink, NeonCyan, NeonPurple, NeonOrange, NeonYellow, NeonGreen, NeonWhite, NeonRed, NeonRain, NeonDash;
        public static Color[] NeonCols;
        public static Material[] NeonMats;
        public static Material[,] Facade;
        public static readonly float[] FacadeH = { 18f, 26f, 36f, 46f, 58f };

        static Shader Pick(params string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                Shader s = Shader.Find(names[i]);
                if (s != null && s.isSupported) return s;
            }
            return Shader.Find("Sprites/Default");
        }

        public static Material NewLit(Color c, float smooth, float metal)
        {
            if (litSh == null) litSh = Pick("Universal Render Pipeline/Lit", "Standard");
            Material m = new Material(litSh);
            m.color = c;
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smooth);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metal);
            return m;
        }

        public static Material NewNeon(Color c)
        {
            if (unlitSh == null) unlitSh = Pick("Universal Render Pipeline/Unlit", "Unlit/Color");
            Material m = new Material(unlitSh);
            m.color = c;
            return m;
        }

        static Material NewTex(Texture2D t, Vector2 tiling)
        {
            if (unlitTexSh == null) unlitTexSh = Pick("Universal Render Pipeline/Unlit", "Unlit/Texture");
            Material m = new Material(unlitTexSh);
            m.color = Color.white;
            m.mainTexture = t;
            m.mainTextureScale = tiling;
            return m;
        }

        public static void Init()
        {
            Fonts.Init();
            LightK = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null ? 3f : 1f;

            Road = NewLit(new Color(0.045f, 0.045f, 0.07f), 0.85f, 0.2f);
            Side = NewLit(new Color(0.13f, 0.12f, 0.17f), 0.3f, 0f);
            RedPaint = NewLit(new Color(0.8f, 0.05f, 0.06f), 0.4f, 0f);
            White = NewLit(new Color(0.85f, 0.85f, 0.92f), 0.4f, 0f);
            Taxi = NewLit(new Color(1f, 0.75f, 0.05f), 0.6f, 0.2f);
            Glass = NewLit(new Color(0.04f, 0.06f, 0.1f), 0.95f, 0.5f);
            Dark = NewLit(new Color(0.06f, 0.06f, 0.08f), 0.5f, 0.2f);
            Pole = NewLit(new Color(0.1f, 0.1f, 0.13f), 0.4f, 0.4f);
            Vend = NewLit(new Color(0.12f, 0.12f, 0.2f), 0.5f, 0.2f);
            Van = NewLit(new Color(0.1f, 0.45f, 0.3f), 0.5f, 0.2f);
            Skin = NewLit(new Color(0.9f, 0.7f, 0.55f), 0.2f, 0f);
            Jacket = NewLit(new Color(0.12f, 0.12f, 0.2f), 0.4f, 0.2f);
            Jeans = NewLit(new Color(0.1f, 0.15f, 0.35f), 0.2f, 0f);
            Pack = NewLit(new Color(0.15f, 0.1f, 0.2f), 0.3f, 0.1f);
            Hair = NewLit(new Color(0.05f, 0.04f, 0.06f), 0.2f, 0f);
            Cap = NewLit(new Color(0.9f, 0.1f, 0.5f), 0.4f, 0f);
            ToriiRed = NewLit(new Color(0.75f, 0.06f, 0.05f), 0.4f, 0f);
            ToriiBlack = NewLit(new Color(0.05f, 0.04f, 0.05f), 0.4f, 0f);
            SignBack = NewLit(new Color(0.03f, 0.03f, 0.05f), 0.3f, 0f);
            Roof = NewLit(new Color(0.08f, 0.08f, 0.1f), 0.3f, 0f);
            Wheel = NewLit(new Color(0.02f, 0.02f, 0.02f), 0.2f, 0f);

            NeonPink = NewNeon(Pal.Pink);
            NeonCyan = NewNeon(Pal.Cyan);
            NeonPurple = NewNeon(Pal.Purple);
            NeonOrange = NewNeon(Pal.Orange);
            NeonYellow = NewNeon(Pal.Yellow);
            NeonGreen = NewNeon(Pal.Green);
            NeonWhite = NewNeon(new Color(0.9f, 0.95f, 1f));
            NeonRed = NewNeon(Pal.Red);
            NeonRain = NewNeon(new Color(0.55f, 0.75f, 1f));
            NeonDash = NewNeon(new Color(0.7f, 0.85f, 1f));

            NeonCols = new Color[] { Pal.Pink, Pal.Cyan, Pal.Purple, Pal.Orange, Pal.Green, Pal.Yellow };
            NeonMats = new Material[] { NeonPink, NeonCyan, NeonPurple, NeonOrange, NeonGreen, NeonYellow };

            Facade = new Material[4, FacadeH.Length];
            for (int v = 0; v < 4; v++)
            {
                Texture2D tex = MakeFacade(11 + v * 7);
                for (int h = 0; h < FacadeH.Length; h++)
                    Facade[v, h] = NewTex(tex, new Vector2(1f, FacadeH[h] / 46f));
            }
        }

        static Texture2D MakeFacade(int seed)
        {
            const int cols = 8, rows = 16, cell = 8;
            const int W = cols * cell, H = rows * cell;
            System.Random rng = new System.Random(seed);
            Color[] px = new Color[W * H];
            Color wall = new Color(0.07f, 0.07f, 0.11f, 1f);
            for (int i = 0; i < px.Length; i++) px[i] = wall;

            Color[] lights = {
                new Color(1f, 0.82f, 0.45f), new Color(0.6f, 0.95f, 1f), new Color(1f, 0.45f, 0.75f),
                new Color(0.95f, 0.95f, 1f), new Color(0.7f, 0.5f, 1f)
            };

            for (int r = 0; r < rows; r++)
            {
                bool floorOn = rng.NextDouble() < 0.35;
                for (int c = 0; c < cols; c++)
                {
                    bool lit = rng.NextDouble() < (floorOn ? 0.85 : 0.28);
                    Color col;
                    if (lit)
                    {
                        col = lights[rng.Next(lights.Length)] * (0.6f + 0.4f * (float)rng.NextDouble());
                        col.a = 1f;
                    }
                    else col = new Color(0.03f, 0.04f, 0.08f, 1f);

                    for (int y = 2; y < 6; y++)
                        for (int x = 1; x < 6; x++)
                            px[(r * cell + y) * W + c * cell + x] = col;
                }
            }

            Texture2D t = new Texture2D(W, H, TextureFormat.RGBA32, true);
            t.SetPixels(px);
            t.wrapMode = TextureWrapMode.Repeat;
            t.filterMode = FilterMode.Bilinear;
            t.anisoLevel = 4;
            t.Apply(true);
            return t;
        }
    }
}
