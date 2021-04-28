//*****************************************************************************
//
// Copyright (C) SAAB AB
//
// All rights, including the copyright, to the computer program(s)
// herein belong to SAAB AB. The program(s) may be used and/or
// copied only with the written permission of Saab AB, or in
// accordance with the terms and conditions stipulated in the
// agreement/contract under which the program(s) have been
// supplied.
//
//
// Information Class:	COMPANY UNCLASSIFIED
// Defence Secrecy:		NOT CLASSIFIED
// Export Control:		NOT EXPORT CONTROLLED
//
//
// File			: NodeProperties.cs
// Module		:
// Description	: Adds PropertyAttributes to GizmoSDK NodeHandler
// Author		: Anders Modén
// Product		: Gizmo3D 2.10.6
//
//
//
// NOTE:	Gizmo3D is a high performance 3D Scene Graph and effect visualisation 
//			C++ toolkit for Linux, Mac OS X, Windows, Android, iOS and HoloLens for  
//			usage in Game or VisSim development.
//
//
// Revision History...
//
// Who	Date	Description
//
// AMO	180607	Created file                        (2.9.1)
//
//******************************************************************************

// ************************** NOTE *********************************************
//
//      Stand alone from BTA !!! No BTA code in this !!!
//
// *****************************************************************************

#if UNITY_EDITOR // Only active in unity editor

using System.Collections.Generic;

// Unity Managed classes
using UnityEngine;
using System;
using UnityEditor;
using System.Reflection;


using GizmoSDK.Gizmo3D;
using Saab.Foundation.Unity.MapStreamer;

[AttributeUsage(AttributeTargets.Property)]
public class ExposePropertyAttribute : Attribute
{

}

public static class ExposeProperties
{
    public static void Expose(PropertyField[] properties)
    {

        GUILayoutOption[] emptyOptions = new GUILayoutOption[0];

        EditorGUILayout.BeginVertical(emptyOptions);

        foreach (PropertyField field in properties)
        {

            EditorGUILayout.BeginHorizontal(emptyOptions);

            switch (field.Type)
            {
                case SerializedPropertyType.Integer:
                    field.SetValue(EditorGUILayout.IntField(field.Name, (int)field.GetValue(), emptyOptions));
                    break;

                case SerializedPropertyType.Float:
                    field.SetValue(EditorGUILayout.FloatField(field.Name, (float)field.GetValue(), emptyOptions));
                    break;

                case SerializedPropertyType.Boolean:
                    field.SetValue(EditorGUILayout.Toggle(field.Name, (bool)field.GetValue(), emptyOptions));
                    break;

                case SerializedPropertyType.String:
                    field.SetValue(EditorGUILayout.TextField(field.Name, (String)field.GetValue(), emptyOptions));
                    break;

                case SerializedPropertyType.Vector2:
                    field.SetValue(EditorGUILayout.Vector2Field(field.Name, (Vector2)field.GetValue(), emptyOptions));
                    break;

                case SerializedPropertyType.Vector3:
                    field.SetValue(EditorGUILayout.Vector3Field(field.Name, (Vector3)field.GetValue(), emptyOptions));
                    break;

                case SerializedPropertyType.Enum:
                    field.SetValue(EditorGUILayout.EnumPopup(field.Name, (Enum)field.GetValue(), emptyOptions));
                    break;

                case SerializedPropertyType.ObjectReference:
                    field.SetValue(EditorGUILayout.ObjectField(field.Name,field.GetValue() as UnityEngine.Object, field.GetPropertyType(), true, emptyOptions));
                    break;

                default:

                    break;

            }

            EditorGUILayout.EndHorizontal();

        }

        EditorGUILayout.EndVertical();

    }

    public static PropertyField[] GetProperties(System.Object obj)
    {

        List<PropertyField> fields = new List<PropertyField>();

        PropertyInfo[] infos = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (PropertyInfo info in infos)
        {

            if (!(info.CanRead && info.CanWrite))
                continue;

            object[] attributes = info.GetCustomAttributes(true);

            bool isExposed = false;

            foreach (object o in attributes)
            {
                if (o.GetType() == typeof(ExposePropertyAttribute))
                {
                    isExposed = true;
                    break;
                }
            }

            if (!isExposed)
                continue;

            SerializedPropertyType type = SerializedPropertyType.Integer;

            if (PropertyField.GetPropertyType(info, out type))
            {
                PropertyField field = new PropertyField(obj, info, type);
                fields.Add(field);
            }

        }

        return fields.ToArray();

    }

}


public class PropertyField
{
    System.Object m_Instance;
    PropertyInfo m_Info;
    SerializedPropertyType m_Type;

    MethodInfo m_Getter;
    MethodInfo m_Setter;

    public SerializedPropertyType Type
    {
        get
        {
            return m_Type;
        }
    }

    public String Name
    {
        get
        {
            return ObjectNames.NicifyVariableName(m_Info.Name);
        }
    }

    public PropertyField(System.Object instance, PropertyInfo info, SerializedPropertyType type)
    {

        m_Instance = instance;
        m_Info = info;
        m_Type = type;

        m_Getter = m_Info.GetGetMethod();
        m_Setter = m_Info.GetSetMethod();
    }

    public System.Object GetValue()
    {
        return m_Getter.Invoke(m_Instance, null);
    }

    public void SetValue(System.Object value)
    {
        m_Setter.Invoke(m_Instance, new System.Object[] { value });
    }

    public Type GetPropertyType()
    {
        return m_Info.PropertyType;
    }

    public static bool GetPropertyType(PropertyInfo info, out SerializedPropertyType propertyType)
    {

        propertyType = SerializedPropertyType.Generic;

        Type type = info.PropertyType;

        if (type == typeof(int))
        {
            propertyType = SerializedPropertyType.Integer;
            return true;
        }

        if (type == typeof(float))
        {
            propertyType = SerializedPropertyType.Float;
            return true;
        }

        if (type == typeof(bool))
        {
            propertyType = SerializedPropertyType.Boolean;
            return true;
        }

        if (type == typeof(string))
        {
            propertyType = SerializedPropertyType.String;
            return true;
        }

        if (type == typeof(Vector2))
        {
            propertyType = SerializedPropertyType.Vector2;
            return true;
        }

        if (type == typeof(Vector3))
        {
            propertyType = SerializedPropertyType.Vector3;
            return true;
        }

        if (type.IsEnum)
        {
            propertyType = SerializedPropertyType.Enum;
            return true;
        }
        // COMMENT OUT to NOT expose custom objects/types
        propertyType = SerializedPropertyType.ObjectReference;
        return true;

        //return false;

    }

}

[CustomEditor(typeof(NodeHandle))]
public class NodeHandleEditor : Editor
{
    NodeHandle m_Instance;
    PropertyField[] m_fields;

    string m_defaultClass = "gzGeometry";

    public void OnEnable()
    {
        m_Instance = target as NodeHandle;
        m_fields = ExposeProperties.GetProperties(m_Instance);
    }

    bool PropEdit_Object(GizmoSDK.GizmoBase.Object obj)
    {
        if (obj == null)
            return false;

        bool change = false;

        GUILayoutOption[] emptyOptions = new GUILayoutOption[0];

        // ------------------- Ref Count ------------------------------------------------------

        EditorGUILayout.LabelField("Ref Count", string.Format("{0}", obj.GetReferenceCount()));

        EditorGUILayout.Separator();
        
        return change;
    }

    bool PropEdit_Node(Node node)
    {
        if (node == null)
            return false;

        bool change = false;

        GUILayoutOption[] emptyOptions = new GUILayoutOption[0];
               
        // ------------------------- Name -----------------------------------------------------

        string name = EditorGUILayout.TextField("Name",node.GetName(), emptyOptions);
               
        if (name != node.GetName())
        {
            node.SetName(name);
            change = true;

            if (name.Length > 0)
                m_Instance.gameObject.name = name;
            else
                m_Instance.gameObject.name = node.GetNativeTypeName();
        }

        // ------------------------ Boundary --------------------------------------------------

        EditorGUILayout.LabelField("Boundary Radius", string.Format("{0}",node.BoundaryRadius));

        bool forceLocal=EditorGUILayout.Toggle("Force Local Include", node.ForceLocalIncludeBoundary);

        if(forceLocal != node.ForceLocalIncludeBoundary)
        {
            node.ForceLocalIncludeBoundary = forceLocal;
            change = true;
        }


        EditorGUILayout.Separator();


        return change;
    }

    bool PropEdit_DynamicLoader(DynamicLoader loader)
    {
        if (loader == null)
            return false;

        bool change = false;

        GUILayoutOption[] emptyOptions = new GUILayoutOption[0];

        // ------------------------- NodeURL -----------------------------------------------------

        string url = EditorGUILayout.TextField("Node URL", loader.NodeURL, emptyOptions);

        if (url != loader.NodeURL)
        {
            loader.NodeURL = url;
            change = true;
        }

        // ------------------------ GetLastAccessRenderCount ------------------------------------

        EditorGUILayout.LabelField("Last Access", string.Format("{0}", loader.GetLastAccessRenderCount()));

        EditorGUILayout.Separator();

        return change;
    }
    public override void OnInspectorGUI()
    {

        if (m_Instance == null)
            return;

        this.DrawDefaultInspector();
        
        // Expose the decorated fields
        ExposeProperties.Expose(m_fields);

        GUILayoutOption[] emptyOptions = new GUILayoutOption[0];

        if(m_Instance.node==null || !m_Instance.node.IsValid())
        {
            EditorGUILayout.LabelField("No created native Node Handle", emptyOptions);

            m_defaultClass = EditorGUILayout.TextField("Class Name", m_defaultClass, emptyOptions);

            EditorGUILayout.BeginHorizontal(emptyOptions);

            if (GUILayout.Button("Create "+ m_defaultClass))
            {
                
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            return;
        }

        /////////////////////////////////////////////////////
        /// Presentation Area of node attributes
        /// Shall reamin Locked
        /// 

        try
        {
            NodeLock.WaitLockEdit();

            /////////////////////////////////////////////////////////////////////////////////////////////

            bool change = false;

            if (PropEdit_Object(m_Instance.node as GizmoSDK.GizmoBase.Object))
                change = true;

            if (PropEdit_Node(m_Instance.node as Node))
                change = true;

            if (PropEdit_DynamicLoader(m_Instance.node as DynamicLoader))
                change = true;

            if (change)
                m_Instance.node.SetDirtySaveData(true);

            if (m_Instance.node.HasDirtySaveData())
            {
                EditorGUILayout.BeginHorizontal(emptyOptions);

                if (GUILayout.Button("Save"))
                {
                    m_Instance.node.SaveDirtyData();
                }

                GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();
            }

            //////////////////////////////////////////////////////////////////////////////////////////
        }
        finally
        {
            NodeLock.UnLock();
        }
    }
}

#endif  // Only active in unity editor
