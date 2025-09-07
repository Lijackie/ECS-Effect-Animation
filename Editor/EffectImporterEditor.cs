using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EffectAnimation
{
    public class EffectImporterEditor : EditorWindow
    {
        [SerializeField]
        private Material material;

        [SerializeField]
        private VisualTreeAsset asset;

        [MenuItem("Effect Import/Import")]
        public static void ShowEffectImporterEditor()
        {
            //EffectImporterEditor wnd = GetWindow<EffectImporterEditor>();
            //wnd.titleContent = new GUIContent("EffectImporterEditor");

            if (!Directory.Exists("Assets/Effects/Prefabs/"))
                Directory.CreateDirectory("Assets/Effects/Prefabs/");

            string path = EditorUtility.OpenFilePanel("Overwrite with png", "", "str");
            if (path.Length != 0)
            {
                var loader = new EffectLoader();
                var anim = loader.Load(path);
                if (anim == null)
                {
                    Debug.Log($"不能讀取，是否選錯!?");
                    return;
                }

                loader.MakeAtlas(@"Assets/Effects/Atlas/");
            }
        }

        public void CreateGUI()
        {
            var serializedObject = new SerializedObject(this);

            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // VisualElements objects can contain other VisualElement following a tree hierarchy.
            VisualElement label = new Label("Hello World! From C#");

            var property = serializedObject.FindProperty(nameof(material));
            var field = new PropertyField(property);
            field.Bind(serializedObject);
            root.Add(field);

            root.Add(label);
        }
    }
}