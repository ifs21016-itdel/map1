using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigBlit.Eddie.CollidersEditorTools
{

    internal abstract class ColliderToolManager<TClass, TTool> : ColliderEditorScriptableSingleton<TClass> where TClass : ColliderToolManager<TClass, TTool> where TTool : ColliderTool
    {
        public event Action transformToolChanged;
        public event Action stateChanged;

        public Type ActiveToolType => m_ActiveTool?.GetType() ?? NoneToolType;
        public TTool ActiveTool => m_ActiveTool as TTool;
        public TTool[] ColliderTools => m_ColliderToolsCached;

        protected abstract Type NoneToolType { get; }

        [SerializeField]
        private ScriptableObject m_ActiveTool;

        [SerializeField]
        private List<ScriptableObject> m_ColliderTools = new List<ScriptableObject>();

        [SerializeField]
        private TTool[] m_ColliderToolsCached = new TTool[] { };

        [SerializeField]
        private SerializableType m_LastActiveToolType;


        public void SetActiveTool<T>() => SetActiveTool(typeof(T));

        public void SetActiveTool(Type toolType)
        {
            if (toolType == null)
                toolType = NoneToolType;

            if (ActiveToolType == toolType)
                return;

            if (IsActive)
            {
                TTool tool = FindTool(toolType);
                setActiveTool(tool);
            }
            else
            {
                m_LastActiveToolType = new SerializableType(toolType);
            }
        }

        protected T FindTool<T>() where T : TTool
        {
            foreach (var colliderTool in m_ColliderToolsCached)
            {
                if (colliderTool is T tool)
                    return tool;
            }

            return null;
        }

        protected TTool FindTool(string toolName)
        {
            foreach (var colliderTool in m_ColliderToolsCached)
            {
                if (colliderTool.Name == toolName)
                    return colliderTool;
            }

            return null;
        }

        protected TTool FindTool(Type toolType)
        {
            foreach (var colliderTool in m_ColliderToolsCached)
            {
                if (colliderTool.GetType() == toolType)
                    return colliderTool;
            }

            return null;
        }


        protected virtual void OnToolActivating() { }
        protected virtual void OnToolWillBeDeactivated() { }

        protected override void OnActivated()
        {
            createTools();
            activateLastTool();
            stateChanged?.Invoke();
        }

        protected override void OnDeactivated()
        {

            if (m_ActiveTool != null)
            {
                OnToolWillBeDeactivated();
                ((TTool)m_ActiveTool).OnDeactivated();
                m_ActiveTool = null;
            }

            destroyTools();
            m_ColliderToolsCached = new TTool[] { };
            stateChanged?.Invoke();

        }

        private void setActiveTool(TTool tool)
        {
            if (m_ActiveTool != null)
            {
                OnToolWillBeDeactivated();
                ((TTool)m_ActiveTool).OnDeactivated();
            }

            m_ActiveTool = tool;
            m_LastActiveToolType = new SerializableType(tool);
            OnToolActivating();
            tool.OnActivated();
            transformToolChanged?.Invoke();
        }

        private void activateLastTool()
        {
            if (!SerializableType.IsValid(m_LastActiveToolType))
                m_LastActiveToolType = new SerializableType(NoneToolType);

            var tool = FindTool(m_LastActiveToolType);
            setActiveTool(tool);
        }

        private void createTools()
        {
            destroyTools();

            var toolsTypes = getToolsTypes();
            foreach (var toolType in toolsTypes)
            {
                if (toolType == null)
                    continue;

                var tool = createTool(toolType);
                m_ColliderTools.Add(tool);
            }

            m_ColliderToolsCached = m_ColliderTools.Cast<TTool>().ToArray();
        }

        private void destroyTools()
        {
            foreach (var tool in m_ColliderTools)
            {
                if (tool == null)
                    continue;
                ScriptableObject.DestroyImmediate(tool);
            }

            m_ColliderTools.Clear();
            m_ColliderToolsCached = new TTool[] { };
        }

        private TTool createTool(Type toolType)
        {
            var tool = ScriptableObject.CreateInstance(toolType) as TTool;
            tool.hideFlags = HideFlags.DontSave;
            return (TTool)tool;
        }

        private Type[] getToolsTypes()
        {
            var baseType = typeof(TTool);
            return baseType.Assembly.GetTypes().Where((x) => x.IsSubclassOf(baseType) && !x.IsAbstract && x.IsClass).ToArray();
        }
    }
}
