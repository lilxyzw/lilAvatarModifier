using UnityEditor;
using UnityEngine;

namespace jp.lilxyzw.lilycalinventory
{
    using runtime;

    // プレースホルダーで親フォルダを表示
    [CustomPropertyDrawer(typeof(MenuFolderOverrideAttribute))]
    internal class MenuFolderOverrideDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string parentName;
            var gameObject = (property.serializedObject.targetObject as Component).gameObject;
            if(property.objectReferenceValue)
            {
                parentName = property.objectReferenceValue.name;
            }
            else if(property.serializedObject.targetObject is AutoDresser)
            {
                var root = gameObject.GetAvatarRoot();
                if(root)
                {
                    var settings = root.GetComponentInChildren<AutoDresserSettings>();
                    if(settings) parentName = settings.GetMenuName();
                    else parentName = "AutoDresser";
                }
                else
                {
                    parentName = "AutoDresser";
                }
            }
            else
            {
                var parent = gameObject.GetComponentInParentInAvatar<MenuFolder>();
                if(parent) parentName = parent.gameObject.name;
                else parentName = "(Root)";
            }
            GUIHelper.ObjectField(position, Localization.G(property), property, $"{parentName} (Menu Folder)");
        }
    }

    // Vector4を1行で表示
    [CustomPropertyDrawer(typeof(OneLineVectorAttribute))]
    internal class OneLineVectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            bool wideMode = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = true;
            var vec = EditorGUI.Vector4Field(position, Localization.G(property), property.vector4Value);
            EditorGUIUtility.wideMode = wideMode;
            if(EditorGUI.EndChangeCheck())
            {
                property.vector4Value = vec;
            }
        }
    }

    // ラベルなし
    [CustomPropertyDrawer(typeof(NoLabelAttribute))]
    internal class NoLabelDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, GUIContent.none);
        }
    }

    // プレースホルダーでメニュー名を表示
    [CustomPropertyDrawer(typeof(MenuNameAttribute))]
    internal class MenuNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool isGenerateParameter = property.serializedObject.targetObject is IGenerateParameter;
            string key = isGenerateParameter ? "inspector.menuParameterName" : "inspector.menuName";
            #if LIL_MODULAR_AVATAR
            bool overrideMA = property.serializedObject.FindProperty("parentOverrideMA").objectReferenceValue;
            if(overrideMA && isGenerateParameter) key = "inspector.parameterName";
            if(overrideMA && !isGenerateParameter) EditorGUI.BeginDisabledGroup(true);
            #endif
            GUIHelper.TextField(position, Localization.G(key), property, property.serializedObject.targetObject.name);
            #if LIL_MODULAR_AVATAR
            if(overrideMA && !isGenerateParameter) EditorGUI.EndDisabledGroup();
            #endif
        }
    }

    // プレースホルダーで衣装名を表示
    [CustomPropertyDrawer(typeof(CostumeNameAttribute))]
    internal class CostumeNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var copy = property.Copy();
            copy.Next(false);
            copy.Next(false);
            copy.Next(false);
            copy.Next(false);
            string name = property.stringValue;

            if(string.IsNullOrEmpty(name))
            {
                var togglers = copy.FPR("objects");
                for(int i = 0; i < togglers.arraySize; i++)
                {
                    var obj = togglers.GetArrayElementAtIndex(i).FPR("obj").objectReferenceValue;
                    if(!obj || string.IsNullOrEmpty(obj.name)) continue;
                    name = obj.name;
                    break;
                }
            }

            if(string.IsNullOrEmpty(name))
            {
                var modifiers = copy.FPR("blendShapeModifiers");
                for(int i = 0; i < modifiers.arraySize; i++)
                {
                    var nv = modifiers.GetArrayElementAtIndex(i).FPR("blendShapeNameValues");
                    for(int j = 0; j < nv.arraySize; j++)
                    {
                        var nameTemp = nv.GetArrayElementAtIndex(j).FPR("name").stringValue;
                        if(string.IsNullOrEmpty(nameTemp)) continue;
                        name = nameTemp;
                        break;
                    }
                    if(!string.IsNullOrEmpty(name)) break;
                }
            }

            if(string.IsNullOrEmpty(name))
            {
                var replacers = copy.FPR("materialReplacers");
                for(int i = 0; i < replacers.arraySize; i++)
                {
                    var t = replacers.GetArrayElementAtIndex(i).FPR("replaceTo");
                    for(int j = 0; j < t.arraySize; j++)
                    {
                        var m = t.GetArrayElementAtIndex(j).objectReferenceValue;
                        if(!m || string.IsNullOrEmpty(m.name)) continue;
                        name = m.name;
                        break;
                    }
                    if(!string.IsNullOrEmpty(name)) break;
                }
            }

            if(string.IsNullOrEmpty(name))
            {
                var modifiers = copy.FPR("materialPropertyModifiers");
                for(int i = 0; i < modifiers.arraySize; i++)
                {
                    var m = modifiers.GetArrayElementAtIndex(i).FPR("floatModifiers");
                    for(int j = 0; j < m.arraySize; j++)
                    {
                        var n = m.GetArrayElementAtIndex(j).FPR("propertyName").stringValue;
                        if(string.IsNullOrEmpty(n)) continue;
                        name = n;
                        break;
                    }
                    if(!string.IsNullOrEmpty(name)) break;
                }
            }

            if(string.IsNullOrEmpty(name))
            {
                var modifiers = copy.FPR("materialPropertyModifiers");
                for(int i = 0; i < modifiers.arraySize; i++)
                {
                    var m = modifiers.GetArrayElementAtIndex(i).FPR("vectorModifiers");
                    for(int j = 0; j < m.arraySize; j++)
                    {
                        var n = m.GetArrayElementAtIndex(j).FPR("propertyName").stringValue;
                        if(string.IsNullOrEmpty(n)) continue;
                        name = n;
                        break;
                    }
                    if(!string.IsNullOrEmpty(name)) break;
                }
            }

            if(string.IsNullOrEmpty(name))
            {
                name = Localization.S("inspector.menuNameEmpty");
            }

            if(!string.IsNullOrEmpty(property.stringValue)) name = " ";

            GUIHelper.TextField(position, Localization.G(property), property, name);
        }
    }

    // フレーム値をパーセント表記で表示
    [CustomPropertyDrawer(typeof(FrameAttribute))]
    internal class FrameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            var value = EditorGUI.Slider(position, Localization.G(property), property.floatValue * 100f, 0f, 100f);
            if(EditorGUI.EndChangeCheck()) property.floatValue = value / 100f;
        }
    }

    // プロパティをboxで囲んで表示
    [CustomPropertyDrawer(typeof(LILBoxAttribute))]
    internal class LILBoxDrawer : PropertyDrawer
    {
        ParametersPerMenuDrawer parametersPerMenuDrawer;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.BeginGroup(position, EditorStyles.helpBox);
            position = EditorStyles.helpBox.padding.Remove(position);
            position = EditorStyles.helpBox.padding.Remove(position);
            position.Indent(1);
            GUI.EndGroup();
            if(parametersPerMenuDrawer == null) parametersPerMenuDrawer = new ParametersPerMenuDrawer();
            parametersPerMenuDrawer.OnGUI(position, property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if(parametersPerMenuDrawer == null) parametersPerMenuDrawer = new ParametersPerMenuDrawer();
            return parametersPerMenuDrawer.GetPropertyHeight(property, label) + EditorStyles.helpBox.padding.vertical * 2;
        }
    }

    // パラメータの値を表示
    [CustomPropertyDrawer(typeof(ParameterValueAttribute))]
    internal class ParameterValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as ParameterValueAttribute;
            var isNonZero = false;
            switch(property.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    EditorGUI.PropertyField(position.SingleLine(), property, Localization.G(property));
                    isNonZero = property.boolValue;
                    break;
                case SerializedPropertyType.Integer:
                    if(string.IsNullOrEmpty(attr.RangeArrayName))
                    {
                        EditorGUI.PropertyField(position.SingleLine(), property, Localization.G(property));
                    }
                    else
                    {
                        var range = property.serializedObject.FindProperty(attr.RangeArrayName).arraySize;
                        property.intValue = EditorGUI.IntSlider(position.SingleLine(), Localization.G(property), property.intValue, 0, range - 1);
                    }
                    isNonZero = property.intValue != 0;
                    break;
            }
            if(attr.NonZeroWarning && isNonZero)
            {
                position.NewLine();
                position.height = GUIHelper.propertyHeight * 2;
                EditorGUI.HelpBox(position, Localization.S($"inspector.{property.name}NonZeroWarning"), MessageType.Warning);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var attr = attribute as ParameterValueAttribute;
            var isNonZero = false;
            switch(property.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    isNonZero = property.boolValue;
                    break;
                case SerializedPropertyType.Integer:
                    isNonZero = property.intValue != 0;
                    break;
            }
            return attr.NonZeroWarning && isNonZero
                ? GUIHelper.propertyHeight + GUIHelper.GetSpaceHeight() * GUIHelper.propertyHeight * 2
                : GUIHelper.propertyHeight;
        }
    }
}
