using UnityEngine;

namespace TokyoRunner
{
    /// <summary>Starts the game automatically in any scene - nothing has to be set up by hand.</summary>
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Object.FindFirstObjectByType<GameManager>() != null) return;
            new GameObject("TokyoRunner").AddComponent<GameManager>();
        }
    }
}
