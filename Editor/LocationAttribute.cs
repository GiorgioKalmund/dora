#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using SpaceFoundationSystem;
using UnityEditor;

namespace giorgiokalmund.Dora.Editor
{
    public class LocationAttribute : PropertyAttribute
    {
        internal readonly string DataMemberName;
        
        public LocationAttribute(string dataMemberName)
        {
            DataMemberName = dataMemberName;
        }
    }
    
    [CustomPropertyDrawer(typeof(LocationAttribute))]
    public class LocationAttributeDrawer : PropertyDrawer
    {
        private static readonly Dictionary<int, bool> ShowManual = new ();
        private static readonly Dictionary<int, bool> IntegrationIssue = new ();
        
        private static readonly GUIContent DropdownIcon =
            EditorGUIUtility.IconContent("scenevis_hidden_hover@2x");

        private static readonly GUIContent ManualIcon =
            EditorGUIUtility.IconContent("scenevis_visible_hover@2x");
        
        private const float ShowButtonWidth = 22f;
        private const float ShowButtonPadding = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.isEditingMultipleObjects)
            {
                // TODO: @Multi-Object editing
                EditorGUI.HelpBox(position, "The Location attribute currently do not support multi-object editing.", MessageType.Info);
                return;
            }
            
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position,
                    $"Location attributes can only be applied to strings and not {property.propertyType.ToString()}",
                    MessageType.Error);
                return;
            }

            var locationAttribute = (LocationAttribute)attribute;
            
            var sfsData = ResolveData(property.serializedObject.targetObject, locationAttribute.DataMemberName);

            float line = EditorGUIUtility.singleLineHeight;
            float space = EditorGUIUtility.standardVerticalSpacing;
            float lowerHelpBoxHeight = line * 2;

            var rowRect = new Rect(position.x, position.y, position.width, line);
            var fieldRect = new Rect(rowRect.x, rowRect.y, rowRect.width - ShowButtonWidth - ShowButtonPadding, line);
            var buttonRect = new Rect(fieldRect.xMax + ShowButtonPadding, rowRect.y, ShowButtonWidth, line);
            var lowerHelpBoxPos = new Rect(position.x, position.y + line + space, position.width, lowerHelpBoxHeight);

            var key = GetUniqueKey(property);
            var anchorId = property.stringValue;
            
            bool manual = ShowManual.GetValueOrDefault(key);
            bool integrationIssue = IntegrationIssue.GetValueOrDefault(key);

            EditorGUI.BeginProperty(position, label, property);

            var ids = sfsData?.AnchorIDs();
            var names = sfsData?.AnchorNames();
            bool emptyAnchor = string.IsNullOrEmpty(anchorId);

            
            if (sfsData == null)
            {
                EditorGUI.HelpBox(position, $"The Provided SFSData is null!", MessageType.Warning);
                return;
            }
            if (ids == null)
            {
                EditorGUI.HelpBox(position, $"The Provided SFSData does not contain any anchor ids!", MessageType.Warning);
                return;
            }
            if (names == null)
            {
                EditorGUI.HelpBox(position, $"The Provided SFSData does not contain any anchor names!", MessageType.Warning);
                return;
            }
            if (ids.Length != names.Length)
            {
                EditorGUI.HelpBox(position, $"The Provided SFSData is malformatted. {ids.Length} ids != {names.Length} names!", MessageType.Warning);
                return;
            }
            
            if (sfsData.TryGetAnchorSoAIndex(anchorId, out int soaIdx) || (emptyAnchor && ids.Length > 0))
            {
                if (emptyAnchor)
                {
                    // We default to the first anchor in the SFSData if available, else we fall back down below
                    property.stringValue = ids[0];
                    return;
                }
                
                if (manual)
                {
                    // Manual Textfield
                    property.stringValue = EditorGUI.TextField(fieldRect, label, property.stringValue);
                }
                else
                {
                    // Popup
                    int newIndex = EditorGUI.Popup(fieldRect, label.text, soaIdx, names);
                    if (newIndex >= 0 && newIndex < ids.Length)
                        property.stringValue = ids[newIndex];
                }

                IntegrationIssue[key] = false;
            }
            else 
            {
                // Anchor reference not found in data || empty anchorId + no anchors in data
                var old = GUI.backgroundColor;
                GUI.backgroundColor = Color.orange;
                property.stringValue = EditorGUI.TextField(rowRect, label, property.stringValue);
                GUI.backgroundColor = old;
                EditorGUI.HelpBox(lowerHelpBoxPos, $"The anchor id '{anchorId}' cannot be found in the linked SFSData '{sfsData.name}'!", MessageType.Error);
                
                IntegrationIssue[key] = true;
            }

            if (!integrationIssue)
            {
                if (GUI.Button(buttonRect, manual ? DropdownIcon : ManualIcon, GUIStyle.none))
                {
                    manual = !manual;
                    ShowManual[key] = manual;
                }
            }

            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            bool integrationIssue = IntegrationIssue.GetValueOrDefault(GetUniqueKey(property));
            return (EditorGUIUtility.singleLineHeight * (integrationIssue ? 3 : 1)) + EditorGUIUtility.standardVerticalSpacing;
        }

        // TODO: Could be more unique. Especially required if @Multi-Object Editing is enabled
        private int GetUniqueKey(SerializedProperty property)
        {
            var locationAttribute = (LocationAttribute)attribute;
            return HashCode.Combine(locationAttribute.DataMemberName, property.propertyPath);
        }

        // TODO @Performance: Cache via some compile time tricks
        private static SpaceFoundationData ResolveData(UnityEngine.Object target, string memberName)
        {
            if (target == null || string.IsNullOrWhiteSpace(memberName))
                return null;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type type = target.GetType();

            FieldInfo field = type.GetField(memberName, flags);
            if (field != null && typeof(SpaceFoundationData).IsAssignableFrom(field.FieldType))
                return field.GetValue(target) as SpaceFoundationData;

            PropertyInfo prop = type.GetProperty(memberName, flags);
            if (prop != null && typeof(SpaceFoundationData).IsAssignableFrom(prop.PropertyType))
                return prop.GetValue(target) as SpaceFoundationData;

            MethodInfo method = type.GetMethod(memberName, flags, null, Type.EmptyTypes, null);
            if (method != null && typeof(SpaceFoundationData).IsAssignableFrom(method.ReturnType))
                return method.Invoke(target, null) as SpaceFoundationData;

            return null;
        }
    }
#endif
}