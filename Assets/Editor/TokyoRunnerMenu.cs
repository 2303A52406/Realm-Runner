using UnityEditor;
using UnityEditor.SceneManagement;

namespace TokyoRunner.EditorTools
{
    public static class TokyoRunnerMenu
    {
        [MenuItem("Tokyo Runner/Create Scene and Play")]
        public static void CreateSceneAndPlay()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            const string path = "Assets/Scenes/TokyoNight.unity";
            EditorSceneManager.SaveScene(scene, path);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
            EditorApplication.isPlaying = true;
        }
    }
}
