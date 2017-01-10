#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.SceneManagement;
using System.IO;

class LaunchFromMainScene : EditorWindow
{

    [MenuItem("Play/PlayMe _%h")]
    public static void RunMainScene()
    {
        
        string currentSceneName = "Assets/Scenes/" + EditorSceneManager.GetActiveScene().name + ".unity";
        File.WriteAllText(".lastScene", currentSceneName);
        EditorSceneManager.OpenScene("Assets/Scenes/loading.unity");
        EditorApplication.isPlaying = true;
        
    }

    [MenuItem("Play/Reload editing scene _%g")]
    public static void ReturnToLastScene()
    {
        string lastScene = File.ReadAllText(".lastScene");
        EditorSceneManager.OpenScene(lastScene);
    }
    
  
}
#endif

