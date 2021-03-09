// Snatched from Kaae.DebugButton and improved by HHH
namespace ToyBox
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true)]
    public sealed class DebugButtonDrawer : Editor
    {
        private const string _btnTitle = "<size=14><color=ff7f0f>Debug Buttons</color></size>";

        private class ParamTuple
        {
            public object obj;
            public ParameterInfo paramInfo;
        }

        private class MethodCallInfo
        {
            public MethodInfo methodInfo;
            public List<ParamTuple> parameters;
        }

        private readonly IList<MethodCallInfo> _methodCallInfo = new List<MethodCallInfo>();

        public void OnEnable()
        {
            FindMethods(this.target.GetType());
        }

        private void FindMethods(Type targetType)
        {
            var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                if (method.GetCustomAttributes(typeof(DebugButtonAttribute), false).Any())
                {
                    var mci = new MethodCallInfo
                    {
                        methodInfo = method,
                        parameters = new List<ParamTuple>()
                    };

                    foreach (var p in method.GetParameters())
                    {
                        mci.parameters.Add(new ParamTuple { obj = p.DefaultValue, paramInfo = p });
                    }

                    _methodCallInfo.Add(mci);
                }
            }

            if (_methodCallInfo.Count == 0 && targetType.BaseType != null)
            {
                // Could not find methods, look in parent class (as long as it is not simply 'object')
                FindMethods(targetType.BaseType);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (_methodCallInfo.Count == 0)
            {
                return;
            }

            GUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(_btnTitle, GUIStyle.none);
            foreach (var info in _methodCallInfo)
            {
                for (int i = 0; i < info.parameters.Count; i++)
                {
                    var p = info.parameters[i];
                    var fieldName = ObjectNames.NicifyVariableName(p.paramInfo.Name);

                    CheckNull<float>(ref p, f => EditorGUILayout.FloatField(fieldName, f));
                    CheckNull<int>(ref p, f => EditorGUILayout.IntField(fieldName, f));
                    CheckNull<long>(ref p, f => EditorGUILayout.LongField(fieldName, f));
                    CheckNull<bool>(ref p, f => EditorGUILayout.Toggle(fieldName, f));
                    CheckNull<string>(ref p, f => EditorGUILayout.TextField(fieldName, f));
                    //CheckNull<Enum>(ref p, f => EditorGUILayout.EnumPopup(fieldName, f));
                    CheckNull<Color>(ref p, f => EditorGUILayout.ColorField(fieldName, f));
                    CheckNull<Vector2>(ref p, f => EditorGUILayout.Vector2Field(fieldName, f));
                    CheckNull<Vector3>(ref p, f => EditorGUILayout.Vector3Field(fieldName, f));
                    CheckNull<Vector4>(ref p, f => EditorGUILayout.Vector4Field(fieldName, f));
                    CheckNull<Transform>(ref p, f => (Transform)EditorGUILayout.ObjectField(fieldName, f, typeof(Transform), true));
                    CheckNull<ScriptableObject>(ref p, f => (ScriptableObject)EditorGUILayout.ObjectField(fieldName, f, typeof(UnityEngine.Object), true));

                    // Extend here per project with specific types (you gotta copy this file into your project........ an even better way to extend would be super dope tho.
                    //CheckNull<PrefabType>(ref p, f => (PrefabType)EditorGUILayout.EnumPopup(fieldName, f));
                }

                if (GUILayout.Button(ObjectNames.NicifyVariableName(info.methodInfo.Name)))
                {
                    // invoke for each target in selection
                    foreach (var t in targets)
                    {
                        var result = info.methodInfo.Invoke(t, info.parameters.Select(p => p.obj).ToArray());
                        if (result != null)
                        {
                            Debug.Log("Result of " + info.methodInfo.Name + ": " + result);
                        }
                    }
                }
            }

            GUILayout.EndVertical();
        }

        private bool CheckNull<T>(ref ParamTuple tup, Func<T, T> drawMethod)
        {
            if (typeof(T) == tup.paramInfo.ParameterType)
            {
                if (DBNull.Value.Equals(tup.obj))
                {
                    tup.obj = default(T);
                }

                tup.obj = drawMethod((T)tup.obj);
                return true;
            }

            return false;
        }
    }
}