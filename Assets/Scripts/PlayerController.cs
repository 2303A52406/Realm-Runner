using UnityEngine;

namespace TokyoRunner
{
    /// <summary>The runner: lane switching, jumping, sliding and a procedural run animation.</summary>
    public class PlayerController : MonoBehaviour
    {
        public const float LaneW = 2.5f;
        const float Gravity = 28f;
        const float JumpV = 9f;
        const float SlideTime = 0.7f;

        public int Lane = 1;
        public float X, Y, VY, SlideT;
        public bool Grounded = true;
        public float Height { get { return SlideT > 0f ? 0.9f : 1.8f; } }

        bool slideQueued;
        float cycle;
        Transform visual, hipL, hipR, shL, shR;

        public static PlayerController Create()
        {
            GameObject go = new GameObject("Player");
            PlayerController p = go.AddComponent<PlayerController>();
            p.BuildModel();
            return p;
        }

        Transform Pivot(string n, Vector3 pos)
        {
            Transform t = new GameObject(n).transform;
            t.SetParent(visual, false);
            t.localPosition = pos;
            return t;
        }

        void BuildModel()
        {
            visual = new GameObject("Visual").transform;
            visual.SetParent(transform, false);

            // torso, jacket stripe, backpack with neon strips
            Gfx.Box(visual, new Vector3(0f, 1.15f, 0f), new Vector3(0.56f, 0.72f, 0.32f), Mt.Jacket);
            Gfx.Box(visual, new Vector3(0f, 1.3f, 0.165f), new Vector3(0.58f, 0.07f, 0.02f), Mt.NeonCyan);
            Gfx.Box(visual, new Vector3(0f, 1.2f, -0.27f), new Vector3(0.42f, 0.5f, 0.22f), Mt.Pack);
            Gfx.Box(visual, new Vector3(0f, 1.2f, -0.385f), new Vector3(0.06f, 0.42f, 0.02f), Mt.NeonPink);
            Gfx.Box(visual, new Vector3(0f, 1.3f, -0.385f), new Vector3(0.3f, 0.05f, 0.02f), Mt.NeonPink);

            // head, hair, cap, visor
            Gfx.Sphere(visual, new Vector3(0f, 1.72f, 0f), new Vector3(0.34f, 0.36f, 0.34f), Mt.Skin);
            Gfx.Box(visual, new Vector3(0f, 1.72f, -0.12f), new Vector3(0.3f, 0.28f, 0.12f), Mt.Hair);
            Gfx.Box(visual, new Vector3(0f, 1.88f, -0.02f), new Vector3(0.38f, 0.12f, 0.4f), Mt.Cap);
            Gfx.Box(visual, new Vector3(0f, 1.71f, 0.165f), new Vector3(0.3f, 0.07f, 0.03f), Mt.NeonCyan);

            // legs
            hipL = Pivot("HipL", new Vector3(-0.14f, 0.85f, 0f));
            hipR = Pivot("HipR", new Vector3(0.14f, 0.85f, 0f));
            Transform[] hips = { hipL, hipR };
            for (int i = 0; i < 2; i++)
            {
                Gfx.Box(hips[i], new Vector3(0f, -0.38f, 0f), new Vector3(0.2f, 0.72f, 0.22f), Mt.Jeans);
                Gfx.Box(hips[i], new Vector3(0f, -0.74f, 0.04f), new Vector3(0.22f, 0.14f, 0.32f), Mt.White);
                Gfx.Box(hips[i], new Vector3(0f, -0.81f, 0.04f), new Vector3(0.23f, 0.04f, 0.33f), Mt.NeonCyan);
            }

            // arms
            shL = Pivot("ShoulderL", new Vector3(-0.37f, 1.42f, 0f));
            shR = Pivot("ShoulderR", new Vector3(0.37f, 1.42f, 0f));
            Transform[] sh = { shL, shR };
            for (int i = 0; i < 2; i++)
            {
                Gfx.Box(sh[i], new Vector3(0f, -0.27f, 0f), new Vector3(0.15f, 0.56f, 0.16f), Mt.Jacket);
                Gfx.Box(sh[i], new Vector3(0f, -0.6f, 0f), new Vector3(0.14f, 0.14f, 0.14f), Mt.Skin);
            }

            // lights that make the runner glow against the dark street
            Gfx.PointLight(transform, new Vector3(0f, 3f, -1.5f), new Color(0.85f, 0.9f, 1f), 2.4f, 15f);
            Gfx.PointLight(transform, new Vector3(0f, 0.6f, 1.8f), Pal.Pink, 1.6f, 7f);
        }

        public void ResetState(float z)
        {
            Lane = 1; X = 0f; Y = 0f; VY = 0f; SlideT = 0f; Grounded = true; slideQueued = false; cycle = 0f;
            transform.position = new Vector3(0f, 0f, z);
            visual.localRotation = Quaternion.identity;
            visual.localPosition = Vector3.zero;
        }

        public void Step(float dt, float z, float speed, bool left, bool right, bool jump, bool slide)
        {
            if (left && Lane > 0) Lane--;
            if (right && Lane < 2) Lane++;
            float tx = (Lane - 1) * LaneW;
            float lean = tx - X;
            X = Mathf.Lerp(X, tx, 1f - Mathf.Exp(-20f * dt));

            if (jump && Grounded)
            {
                VY = JumpV; Grounded = false; SlideT = 0f;
                Sfx.Play(Sfx.Jump, 0.7f);
            }
            if (slide)
            {
                if (Grounded)
                {
                    if (SlideT <= 0f) Sfx.Play(Sfx.Slide, 0.6f);
                    SlideT = SlideTime;
                }
                else { VY = -18f; slideQueued = true; }
            }
            if (!Grounded)
            {
                VY -= Gravity * dt;
                Y += VY * dt;
                if (Y <= 0f)
                {
                    Y = 0f; VY = 0f; Grounded = true;
                    if (slideQueued) { SlideT = SlideTime; slideQueued = false; }
                }
            }
            if (SlideT > 0f) SlideT -= dt;

            transform.position = new Vector3(X, Y, z);
            cycle += dt * (7f + speed * 0.28f);
            Animate(dt, lean);
        }

        void SetPose(Transform t, Quaternion q, float k) { t.localRotation = Quaternion.Slerp(t.localRotation, q, k); }

        void Animate(float dt, float lean)
        {
            float k = 1f - Mathf.Exp(-22f * dt);
            Quaternion vis, hl, hr, al, ar;
            Vector3 vpos = Vector3.zero;

            if (SlideT > 0f)
            {
                vis = Quaternion.Euler(-78f, 0f, 0f);
                vpos = new Vector3(0f, 0.28f, 0f);
                hl = Quaternion.Euler(-4f, 0f, 0f); hr = Quaternion.Euler(4f, 0f, 0f);
                al = Quaternion.Euler(-40f, 0f, 0f); ar = Quaternion.Euler(-40f, 0f, 0f);
            }
            else if (!Grounded)
            {
                vis = Quaternion.Euler(8f, 0f, -lean * 8f);
                hl = Quaternion.Euler(-50f, 0f, 0f); hr = Quaternion.Euler(35f, 0f, 0f);
                al = Quaternion.Euler(-130f, 0f, 0f); ar = Quaternion.Euler(-100f, 0f, 0f);
            }
            else
            {
                float a = Mathf.Sin(cycle);
                vis = Quaternion.Euler(10f, 0f, -lean * 10f);
                vpos = new Vector3(0f, Mathf.Abs(Mathf.Cos(cycle)) * 0.05f, 0f);
                hl = Quaternion.Euler(a * 55f, 0f, 0f); hr = Quaternion.Euler(-a * 55f, 0f, 0f);
                al = Quaternion.Euler(-a * 50f, 0f, 0f); ar = Quaternion.Euler(a * 50f, 0f, 0f);
            }
            SetPose(visual, vis, k);
            visual.localPosition = Vector3.Lerp(visual.localPosition, vpos, k);
            SetPose(hipL, hl, k); SetPose(hipR, hr, k);
            SetPose(shL, al, k); SetPose(shR, ar, k);
        }

        // after a crash: face-plant
        public void DeadStep(float dt)
        {
            float k = 1f - Mathf.Exp(-10f * dt);
            SetPose(visual, Quaternion.Euler(80f, 0f, 0f), k);
            visual.localPosition = Vector3.Lerp(visual.localPosition, new Vector3(0f, 0.1f, 0f), k);
            SetPose(hipL, Quaternion.Euler(-20f, 0f, 0f), k);
            SetPose(hipR, Quaternion.Euler(25f, 0f, 0f), k);
            SetPose(shL, Quaternion.Euler(-100f, 0f, 0f), k);
            SetPose(shR, Quaternion.Euler(-60f, 0f, 0f), k);
            transform.position = new Vector3(X, 0f, transform.position.z);
        }
    }
}
