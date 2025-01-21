#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;


namespace MMMaellon.GroupTheory
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {

        bool show = true;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // show = EditorGUI.Foldout(position, true, "ADSF", true);
            // show = EditorGUILayout.Foldout(show, "ASDF", true);
            GUI.enabled = false;
            if (property.propertyType == SerializedPropertyType.Vector4)
            {
                // EditorGUI.IntField(position, 1);
                property.vector4Value = EditorGUI.Vector4Field(position, property.name, property.vector4Value);
                // EditorGUI.IntField(position, 1);
                // EditorGUI.IntField(position, 1);
                // EditorGUI.IntField(position, 1);
                // EditorGUI.PropertyField(position, property, label, true);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, false);
            }
            GUI.enabled = true;

        }
    }

    public class BuildHandler : IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => 0;
        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }
            AutoSetup();
        }

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            if (!EditorPrefs.GetBool(autoSetupKey, true))
            {
                return true;
            }
            return AutoSetup();
        }

        public static bool IsEditable(Component component)
        {
            return !EditorUtility.IsPersistent(component.transform.root.gameObject) && !(component.gameObject.hideFlags == HideFlags.NotEditable || component.gameObject.hideFlags == HideFlags.HideAndDontSave);
        }
        private const string MenuItemPath = "MMMaellon/GroupTheory/Automatically run setup";
        private const string autoSetupKey = "GroupTheoryAutoSetup";
        [MenuItem(MenuItemPath)]
        public static void ToggleAutoSetup()
        {
            var autoSetupOn = EditorPrefs.GetBool(autoSetupKey, true);
            autoSetupOn = !autoSetupOn;
            Menu.SetChecked(MenuItemPath, autoSetupOn);
            EditorPrefs.SetBool(autoSetupKey, autoSetupOn);
        }

        [MenuItem(MenuItemPath, true)]
        public static bool ValidateToggleAutoSetup()
        {
            var autoSetupOn = EditorPrefs.GetBool(autoSetupKey, true);
            Menu.SetChecked(MenuItemPath, autoSetupOn);
            return true;
        }

        [MenuItem("MMMaellon/GroupTheory/Run setup")]
        public static bool AutoSetup()
        {
            Debug.Log("Running GroupTheory AutoSetup");
            var allGroups = Object.FindObjectsByType<Group>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
            if (allGroups.Length > 0)
            {
                var allSingletons = Object.FindObjectsOfType<Singleton>(true);
                Singleton singleton;
                if (allSingletons.Length >= 1)
                {
                    singleton = allSingletons[0];
                    for (int i = 1; i < allSingletons.Length; i++)
                    {
                        Object.DestroyImmediate(allSingletons[i].gameObject);
                    }
                }
                else
                {
                    GameObject singletonObject = new("GroupTheorySingleton");
                    singleton = UdonSharpComponentExtensions.AddUdonSharpComponent<Singleton>(singletonObject);
                }
                var allItems = Object.FindObjectsByType<Item>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
                singleton.AutoSetup(allGroups, allItems);
                singleton.hideFlags |= HideFlags.NotEditable;
            }
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            return true;
        }
    }
}
#endif
