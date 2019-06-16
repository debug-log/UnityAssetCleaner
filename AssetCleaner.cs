using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.Linq;
using Object = UnityEngine.Object;

namespace AssetCleanerNamespace
{
    public class AssetCleaner : EditorWindow
    {
        public enum Phase { Setup, Processing, Complete };
        
        private Phase currentPhase = Phase.Setup;

        //Initial SceneSetup
        private SceneSetup[] initialSceneSetup;

        //GUILayout Options
        public static GUILayoutOption GL_EXPAND_WIDTH = GUILayout.ExpandWidth (true);
        public static GUILayoutOption GL_EXPAND_HEIGHT = GUILayout.ExpandHeight (true);
        public static GUILayoutOption GL_WIDTH_25 = GUILayout.Width (25);
        public static GUILayoutOption GL_WIDTH_100 = GUILayout.Width (100);
        public static GUILayoutOption GL_WIDTH_250 = GUILayout.Width (250);
        public static GUILayoutOption GL_HEIGHT_30 = GUILayout.Height (30);
        public static GUILayoutOption GL_HEIGHT_35 = GUILayout.Height (35);
        public static GUILayoutOption GL_HEIGHT_40 = GUILayout.Height (40);

        private Vector2 scrollPosition = Vector2.zero;

        [MenuItem ("Tools/Asset Cleaner")]
        static void Init ()
        {
            AssetCleaner window = GetWindow<AssetCleaner> ();
            window.titleContent = new GUIContent ("Asset Cleaner");

            window.Show ();
        }

        void OnGUI ()
        {
            // Make the window scrollable
            scrollPosition = EditorGUILayout.BeginScrollView (scrollPosition, GL_EXPAND_WIDTH, GL_EXPAND_HEIGHT);

            GUILayout.BeginVertical ();

            GUILayout.Space (10);

            if (currentPhase == Phase.Processing)
            {
                GUILayout.Label (". . . something went wrong, check console . . .");
                
            }
            else if(currentPhase == Phase.Setup)
            {
                if (GUILayout.Button ("GO!", GL_HEIGHT_30))
                {
                    initialSceneSetup = EditorSceneManager.GetSceneManagerSetup ();

                    ExecuteQuery ();
                }
            }
            else if(currentPhase == Phase.Complete)
            {

            }

            GUILayout.EndVertical ();

            EditorGUILayout.EndScrollView ();
        }
        
        private void ExecuteQuery ()
        {
            HashSet<Type> allAssetClasses = new HashSet<Type> ();

            HashSet<string> sceneToSearch = new HashSet<string> ();

            // Get all scenes from the Assets folder
            string[] scenesTemp = AssetDatabase.FindAssets ("t:Scene");

            for (int i = 0; i < scenesTemp.Length; i++)
            {
                sceneToSearch.Add (AssetDatabase.GUIDToAssetPath (scenesTemp[i]));
            }

            foreach(var scenePath in sceneToSearch)
            {
                Debug.Log (scenePath);

                Scene scene = EditorSceneManager.GetSceneByPath (scenePath);

                if (!EditorApplication.isPlaying)
                {
                    scene = EditorSceneManager.OpenScene (scenePath, OpenSceneMode.Additive);
                }

                //씬 내부의 모든 Object를 가져온다.
                GameObject[] rootGameObjects = scene.GetRootGameObjects ();
                for (int i = 0; i < rootGameObjects.Length; i++)
                {
                    SearchGameObjectRecursively (rootGameObjects[i]);
                }
                
                //초기 씬이 아닌 경우
                if (initialSceneSetup.Any((x) => x.path != scenePath))
                {
                    if (scene.isLoaded)
                    {
                        EditorSceneManager.CloseScene (scene, true);
                    }
                }
            }
        }

        private void SearchGameObjectRecursively (GameObject go)
        {
            BeginSearchObject (go);
            Debug.LogFormat ("*** {0}", go.name);

            Transform tr = go.transform;
            for (int i = 0; i < tr.childCount; i++)
            {
                SearchGameObjectRecursively (tr.GetChild (i).gameObject);
            }
        }

        private void BeginSearchObject (Object obj)
        {
            if (obj is GameObject )
            {
                Component[] components = ((GameObject) obj).GetComponents<Component> ();
                
                for (int i = 0; i < components.Length; i++)
                {
                    if(components[i] == null) { continue; }
                    Debug.Log ("****** " + components[i].name);   
                }
            }
        }
    }
}