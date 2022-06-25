using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Balaso
{
    [CustomEditor(typeof(Balaso.Settings))]
    public class SettingsInspector : Editor
    {
        private static string SETTINGS_ASSET_PATH = "Assets/IDFA_ATT/Editor/Settings.asset";
        //"Assets/Balaso Software/Editor/Settings.asset";
        private static Settings settings;
        private static string findString;


        private static GUIStyle ToggleButtonStyleNormal = null;
        private static GUIStyle ToggleButtonStyleToggled = null;


        public static Settings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = (Settings)AssetDatabase.LoadAssetAtPath(SETTINGS_ASSET_PATH, typeof(Balaso.Settings));
                    if (settings == null)
                    {
                        settings = CreateDefaultSettings();
                    }
                }

                return settings;
            }
        }

        private static Settings CreateDefaultSettings()
        {
            Settings asset = ScriptableObject.CreateInstance(typeof(Balaso.Settings)) as Settings;
            AssetDatabase.CreateAsset(asset, SETTINGS_ASSET_PATH);
            asset.PopupMessage = "Your data will only be used to deliver personalized ads to you.";
            //"Your data will only be used to deliver personalized ads to you.";
            return asset;
        }

        [MenuItem("Window/Balaso/App Tracking Transparency/Settings", false, 0)]
        static void SelectSettings()
        {
            Selection.activeObject = Settings;
        }

        public override void OnInspectorGUI()
        {
            if (ToggleButtonStyleNormal == null)
            {
                ToggleButtonStyleNormal = "Button";
                ToggleButtonStyleToggled = new GUIStyle(ToggleButtonStyleNormal);
                ToggleButtonStyleToggled.normal.background = ToggleButtonStyleToggled.active.background;
            }

            settings = target as Balaso.Settings;

            FontStyle fontStyle = EditorStyles.label.fontStyle;
            bool wordWrap = GUI.skin.textField.wordWrap;
            EditorStyles.label.fontStyle = FontStyle.Bold;
            GUI.skin.textField.wordWrap = true;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("App Tracking Transparency", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Present the app-tracking authorization request to the end user with this customizable message", EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PopupMessage"), new GUIContent("Popup Message"));
            DrawHorizontalLine(Color.grey);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("SkAdNetwork", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("SkAdNetworkIds specified will be automatically added to your Info.plist file.", EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("NOTICE: This plugin does not include the ability to show ads.\nYou will need to use your favorite ads platform SDK.", EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            GUILayout.Space(10);
            GUILayout.Label("Sort list", EditorStyles.wordWrappedLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("By name A-Z", settings.nameSorting ? ToggleButtonStyleToggled : ToggleButtonStyleNormal))
            {
                settings.nameSorting = !settings.nameSorting;
                UpdateSorting();
            }
            if (GUILayout.Button("By status", settings.statusSorting ? ToggleButtonStyleToggled : ToggleButtonStyleNormal))
            {
                settings.statusSorting = !settings.statusSorting;
                UpdateSorting();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            GUILayout.Space(10);
            GUILayout.Label("Find in list", EditorStyles.wordWrappedLabel);
            GUILayout.BeginHorizontal();
            findString = EditorGUILayout.TextField(findString);
            if (GUILayout.Button("Find"))
            {
                var obj = settings.SkAdNetworkIds;
                bool found = false;
                foreach (var item in obj)
                {
                    if (item.skAdNetworkIds != null)
                    {
                        foreach (var id in item.skAdNetworkIds)
                        {
                            if (id.Contains(findString))
                            {
                                Debug.Log($"Search result =>  {item.sdkName} | enable : {item.enable}");
                                found = true;
                            }
                        }
                    }
                }
                if (!found)
                    Debug.Log("Search result => not matched");
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);


            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SkAdNetworkIds"), new GUIContent("SkAdNetworkIds"), true);
            GUI.enabled = true;
            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                Repaint();
            }
            GUI.skin.textField.wordWrap = wordWrap;
            EditorStyles.label.fontStyle = fontStyle;
        }

        private void UpdateSorting()
        {
            var ids = settings.SkAdNetworkIds;

            if (settings.nameSorting && settings.statusSorting)
                ids = ids.OrderBy(x => x.sdkName).ThenBy(x => !x.enable).ToList();

            if (settings.nameSorting)
                ids = ids.OrderBy(x => x.sdkName).ToList();

            if (settings.statusSorting)
                ids = ids.OrderBy(x => !x.enable).ToList();

            settings.SkAdNetworkIds = ids;
            serializedObject.Update();
            EditorUtility.SetDirty(target);
            Repaint();
        }

        private void DrawHorizontalLine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }
    }

    //[CustomPropertyDrawer(typeof(MediationInfo))]
    [CustomPropertyDrawer(typeof(MediationInfo))]
    public class MediationInfoEditor : PropertyDrawer
    {
        static Color RED_COLOR = new Color(211f / 255f, 87f / 255f, 87f / 255f, 1);
        static Color GREEN_COLOR = new Color(60f / 255f, 215f / 255f, 95f / 255f, 1);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            bool enable = property.FindPropertyRelative("enable").boolValue;
            GUI.color = enable ? GREEN_COLOR : RED_COLOR;
            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
