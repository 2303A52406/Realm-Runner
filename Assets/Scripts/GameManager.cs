using System.Collections.Generic;
using UnityEngine;

namespace TokyoRunner
{
    public class GameManager : MonoBehaviour
    {
        enum State { Menu, Playing, Dead }

        const float StartSpeed = 15f;
        const float MaxSpeed = 36f;
        const float Accel = 0.3f;
        const float Ahead = 330f;

        State state = State.Menu;
        Camera cam;
        Transform camT;
        PlayerController player;
        Transform tower, rain;
        AudioSource musicSrc;
        readonly List<Segment> segs = new List<Segment>();

        int nextSeg;
        float z, speed, dist;
        int score, coins, best;
        float deadTimer, shake, flash, menuT;

        GUIStyle sTitle, sMid, sSmall, sLeft, sRight;
        bool stylesReady;

        // ------------------------------------------------------------ setup
        void Awake()
        {
            Application.targetFrameRate = 60;
            Mt.Init();
            Sfx.Init(gameObject);
            SetupEnvironment();
            SetupCamera();

            player = PlayerController.Create();
            tower = Gfx.Tower();
            rain = MakeRain();
            best = PlayerPrefs.GetInt("tokyo_best", 0);

            musicSrc = gameObject.AddComponent<AudioSource>();
            musicSrc.clip = Sfx.Music;
            musicSrc.loop = true;
            musicSrc.volume = 0.35f;
            musicSrc.spatialBlend = 0f;
            musicSrc.Play();

            ResetWorld();
        }

        void SetupEnvironment()
        {
            Light[] old = FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < old.Length; i++) Destroy(old[i]);

            RenderSettings.skybox = null;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = Pal.Fog;
            RenderSettings.fogStartDistance = 35f;
            RenderSettings.fogEndDistance = 340f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.2f, 0.17f, 0.34f);

            GameObject moon = new GameObject("Moonlight");
            Light l = moon.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(0.45f, 0.55f, 1f);
            l.intensity = 0.45f;
            l.shadows = LightShadows.None;
            moon.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        void SetupCamera()
        {
            cam = Camera.main;
            if (cam == null)
            {
                GameObject g = new GameObject("Main Camera");
                g.tag = "MainCamera";
                cam = g.AddComponent<Camera>();
                g.AddComponent<AudioListener>();
            }
            camT = cam.transform;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Pal.Fog;
            cam.nearClipPlane = 0.2f;
            cam.farClipPlane = 500f;
            cam.fieldOfView = 62f;
        }

        Transform MakeRain()
        {
            GameObject g = new GameObject("Rain");
            g.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            ParticleSystem ps = g.AddComponent<ParticleSystem>();
            ps.Stop();
            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = 0.7f;
            main.startSpeed = 34f;
            main.startSize = 0.05f;
            main.maxParticles = 5000;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ParticleSystem.EmissionModule em = ps.emission;
            em.rateOverTime = 2200f;
            ParticleSystem.ShapeModule sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Box;
            sh.scale = new Vector3(40f, 80f, 1f);
            ParticleSystemRenderer r = g.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Stretch;
            r.lengthScale = 2f;
            r.velocityScale = 0.03f;
            r.sharedMaterial = Mt.NeonRain;
            ps.Play();
            return g.transform;
        }

        // ------------------------------------------------------------ world
        void ResetWorld()
        {
            for (int i = 0; i < segs.Count; i++)
                if (segs[i] != null) Destroy(segs[i].gameObject);
            segs.Clear();
            nextSeg = 0;
            z = Segment.Length;
            speed = StartSpeed;
            dist = 0f;
            score = 0;
            coins = 0;
            player.ResetState(z);
            FillSegments();
        }

        void FillSegments()
        {
            while (nextSeg * Segment.Length < z + Ahead)
            {
                float diff = Mathf.Clamp01((speed - StartSpeed) / (MaxSpeed - StartSpeed));
                segs.Add(Segment.Create(nextSeg, diff));
                nextSeg++;
            }
            while (segs.Count > 0 && segs[0].ZStart + Segment.Length < z - 45f)
            {
                Destroy(segs[0].gameObject);
                segs.RemoveAt(0);
            }
        }

        void StartRun()
        {
            ResetWorld();
            state = State.Playing;
        }

        // ------------------------------------------------------------ loop
        void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            Inp.Poll();
            if (flash > 0f) flash = Mathf.MoveTowards(flash, 0f, dt * 1.5f);

            switch (state)
            {
                case State.Menu:
                    player.Step(dt, z, 12f, false, false, false, false);
                    if (Inp.Confirm) StartRun();
                    break;

                case State.Playing:
                    Tick(dt);
                    break;

                case State.Dead:
                    deadTimer += dt;
                    player.DeadStep(dt);
                    if (deadTimer > 0.6f && Inp.Confirm) StartRun();
                    break;
            }

            float targetPitch = state == State.Playing ? 1f + (speed - StartSpeed) * 0.008f : 1f;
            musicSrc.pitch = Mathf.Lerp(musicSrc.pitch, targetPitch, 1f - Mathf.Exp(-2f * dt));
        }

        void Tick(float dt)
        {
            speed = Mathf.Min(MaxSpeed, speed + Accel * dt);
            z += speed * dt;
            dist += speed * dt;
            player.Step(dt, z, speed, Inp.Left, Inp.Right, Inp.Jump, Inp.Slide);
            score = (int)(dist * 0.5f) + coins * 25;
            FillSegments();
            Collide();
        }

        void Collide()
        {
            float px = player.X, py = player.Y, ph = player.Height;
            for (int s = 0; s < segs.Count; s++)
            {
                Segment seg = segs[s];
                if (z < seg.ZStart - 6f || z > seg.ZStart + Segment.Length + 6f) continue;

                for (int i = 0; i < seg.Hazards.Count; i++)
                {
                    Hazard h = seg.Hazards[i];
                    if (Mathf.Abs(px - h.x) < h.hw + 0.32f &&
                        Mathf.Abs(z - h.z) < h.hl + 0.3f &&
                        py < h.yMax && py + ph > h.yMin)
                    {
                        Die();
                        return;
                    }
                }
                for (int i = 0; i < seg.Coins.Count; i++)
                {
                    CoinItem c = seg.Coins[i];
                    if (c.taken) continue;
                    if (Mathf.Abs(px - c.x) < 1.0f && Mathf.Abs(z - c.z) < 1.0f && Mathf.Abs(py + ph * 0.5f - c.y) < 1.1f)
                    {
                        c.taken = true;
                        c.t.gameObject.SetActive(false);
                        coins++;
                        Sfx.Play(Sfx.Coin, 0.8f);
                    }
                }
            }
        }

        void Die()
        {
            state = State.Dead;
            deadTimer = 0f;
            shake = 1f;
            flash = 1f;
            if (score > best)
            {
                best = score;
                PlayerPrefs.SetInt("tokyo_best", best);
                PlayerPrefs.Save();
            }
            Sfx.Play(Sfx.Crash, 1f);
        }

        void LateUpdate()
        {
            float dt = Time.deltaTime;
            Vector3 pp = player.transform.position;

            if (state == State.Menu)
            {
                menuT += dt;
                Vector3 target = pp + new Vector3(4.2f + Mathf.Sin(menuT * 0.4f) * 1.5f, 2.2f, -5.5f);
                camT.position = Vector3.Lerp(camT.position, target, 1f - Mathf.Exp(-4f * dt));
                camT.rotation = Quaternion.LookRotation(pp + Vector3.up * 1.2f - camT.position);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 55f, 1f - Mathf.Exp(-3f * dt));
            }
            else
            {
                Vector3 target = new Vector3(player.X * 0.55f, 4.3f + Mathf.Max(0f, player.Y) * 0.4f, pp.z - 7.2f);
                camT.position = Vector3.Lerp(camT.position, target, 1f - Mathf.Exp(-12f * dt));
                Vector3 look = new Vector3(player.X * 0.8f, 1.4f, pp.z + 9f);
                camT.rotation = Quaternion.Slerp(camT.rotation, Quaternion.LookRotation(look - camT.position), 1f - Mathf.Exp(-14f * dt));
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 62f + (speed - StartSpeed) * 0.8f, 1f - Mathf.Exp(-3f * dt));
            }

            if (shake > 0f)
            {
                camT.position += Random.insideUnitSphere * shake * 0.4f;
                shake = Mathf.MoveTowards(shake, 0f, dt * 2.5f);
            }

            tower.position = new Vector3(-75f, 0f, pp.z + 230f);
            rain.position = new Vector3(pp.x, 22f, pp.z + 25f);
        }

        // ------------------------------------------------------------ UI
        void MakeStyles()
        {
            sTitle = new GUIStyle(GUI.skin.label) { fontSize = 86, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            sMid = new GUIStyle(GUI.skin.label) { fontSize = 40, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            sSmall = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            sLeft = new GUIStyle(GUI.skin.label) { fontSize = 32, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
            sRight = new GUIStyle(GUI.skin.label) { fontSize = 32, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleRight };
            stylesReady = true;
        }

        void T(Rect r, string txt, GUIStyle st, Color c)
        {
            st.normal.textColor = new Color(0f, 0f, 0f, 0.65f);
            GUI.Label(new Rect(r.x + 2f, r.y + 2f, r.width, r.height), txt, st);
            st.normal.textColor = c;
            GUI.Label(r, txt, st);
        }

        void OnGUI()
        {
            if (!stylesReady) MakeStyles();
            float s = Screen.height / 720f;
            GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));
            float W = Screen.width / s;
            const float H = 720f;
            GUI.color = Color.white;

            if (flash > 0f)
            {
                GUI.color = new Color(1f, 0.05f, 0.1f, flash * 0.45f);
                GUI.DrawTexture(new Rect(0f, 0f, W, H), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            if (state == State.Menu)
            {
                T(new Rect(0f, 70f, W, 110f), "REALM RUNNER", sTitle, Pal.Pink);
                T(new Rect(0f, 160f, W, 70f), "NIGHT CITY THEME", sMid, Pal.Cyan);
                if (Mathf.Sin(Time.time * 4f) > -0.3f)
                    T(new Rect(0f, 520f, W, 50f), "PRESS SPACE OR TAP TO START", sSmall, Color.white);
                T(new Rect(0f, 580f, W, 40f), "A / D or LEFT / RIGHT: change lane     W / UP / SPACE: jump     S / DOWN: slide", sSmall, Pal.Yellow);
                T(new Rect(0f, 615f, W, 40f), "Touch: swipe left / right / up / down", sSmall, Pal.Yellow);
                T(new Rect(0f, 655f, W, 40f), "BEST  " + best, sSmall, Pal.Green);
                return;
            }

            T(new Rect(30f, 20f, 500f, 50f), "SCORE  " + score, sLeft, Pal.Pink);
            T(new Rect(30f, 62f, 500f, 40f), "BEST  " + Mathf.Max(best, score), sLeft, new Color(0.7f, 0.7f, 0.8f));
            T(new Rect(W - 530f, 20f, 500f, 50f), "COINS  " + coins, sRight, Pal.Yellow);
            T(new Rect(0f, 20f, W, 40f), (int)(speed * 3.6f) + " km/h", sSmall, Pal.Cyan);

            if (state == State.Dead)
            {
                GUI.color = new Color(0f, 0f, 0f, 0.55f);
                GUI.DrawTexture(new Rect(0f, 0f, W, H), Texture2D.whiteTexture);
                GUI.color = Color.white;
                T(new Rect(0f, 150f, W, 110f), "BUSTED!", sTitle, Pal.Red);
                T(new Rect(0f, 280f, W, 60f), "SCORE  " + score + "     COINS  " + coins, sMid, Color.white);
                T(new Rect(0f, 340f, W, 60f), score >= best && score > 0 ? "NEW BEST!" : "BEST  " + best, sMid, Pal.Yellow);
                if (deadTimer > 0.6f && Mathf.Sin(Time.time * 4f) > -0.3f)

                    T(new Rect(0f, 460f, W, 50f), "PRESS SPACE OR TAP TO RUN AGAIN", sSmall, Pal.Cyan);
            }
        }
    }
}