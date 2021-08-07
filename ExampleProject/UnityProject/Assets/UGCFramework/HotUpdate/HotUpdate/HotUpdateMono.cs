using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UGCF.HotUpdate
{
    public class HotUpdateMono : MonoBehaviour
    {
        public const string HotUpdateMBTypeFullName = "UGCF.HotUpdate.HotUpdateMonoBehaviour";
        public string hotUpdateClassFullName;
        public List<HotUpdateField> hotUpdateFields = new List<HotUpdateField>();
        private object hotUpdateInstance;
        public object HotUpdateInstance
        {
            get
            {
                if (hotUpdateInstance == null)
                {
                    hotUpdateInstance = InitHotUpdateMono(hotUpdateClassFullName);
                }
                return hotUpdateInstance;
            }
        }

        #region ...UnitySystemAction
        MethodInfo awakeAction;
        MethodInfo onEnableAction;
        MethodInfo startAction;
        MethodInfo updateAction;
        MethodInfo fixedUpdateAction;
        MethodInfo lateUpdateAction;
        MethodInfo onDisableAction;
        MethodInfo onDestroyAction;
        MethodInfo onApplicationFocus;
        MethodInfo onApplicationPause;
        MethodInfo onApplicationQuit;
        MethodInfo onCollisionEnter;
        MethodInfo onCollisionStay;
        MethodInfo onCollisionExit;
        MethodInfo onCollisionEnter2D;
        MethodInfo onCollisionStay2D;
        MethodInfo onCollisionExit2D;
        MethodInfo onTriggerEnter;
        MethodInfo onTriggerStay;
        MethodInfo onTriggerExit;
        MethodInfo onTriggerEnter2D;
        MethodInfo onTriggerStay2D;
        MethodInfo onTriggerExit2D;
        #endregion

        public object InitHotUpdateMono(string hotUpdateClassFullName)
        {
            if (string.IsNullOrEmpty(hotUpdateClassFullName))
                return null;
            this.hotUpdateClassFullName = hotUpdateClassFullName;
            hotUpdateInstance = HotUpdateTool.InitHotFixMono(this, hotUpdateClassFullName, hotUpdateFields);
            if (hotUpdateInstance != null)
            {
                GetAllUnitySystemAction();
                return hotUpdateInstance;
            }
            return null;
        }

        #region ...系统函数
        void Awake()
        {
            if (!string.IsNullOrEmpty(hotUpdateClassFullName) && hotUpdateInstance == null)
                InitHotUpdateMono(hotUpdateClassFullName);
            InvokeHotUpdateSystemAction(awakeAction, null);
        }

        void Start()
        {
            InvokeHotUpdateSystemAction(startAction, null);
        }

        void OnEnable()
        {
            InvokeHotUpdateSystemAction(onEnableAction, null);
        }

        void Update()
        {
            InvokeHotUpdateSystemAction(updateAction, null);
        }

        void FixedUpdate()
        {
            InvokeHotUpdateSystemAction(fixedUpdateAction, null);
        }

        void LateUpdate()
        {
            InvokeHotUpdateSystemAction(lateUpdateAction, null);
        }

        void OnDisable()
        {
            InvokeHotUpdateSystemAction(onDisableAction, null);
        }

        void OnDestroy()
        {
            InvokeHotUpdateSystemAction(onDestroyAction, null);
        }

        void OnApplicationFocus(bool focus)
        {
            InvokeHotUpdateSystemAction(onApplicationFocus, focus);
        }

        void OnApplicationPause(bool pause)
        {
            InvokeHotUpdateSystemAction(onApplicationPause, pause);
        }

        void OnApplicationQuit()
        {
            InvokeHotUpdateSystemAction(onApplicationQuit, null);
        }

        void OnCollisionEnter(Collision collision)
        {
            InvokeHotUpdateSystemAction(onCollisionEnter, collision);
        }

        void OnCollisionStay(Collision collision)
        {
            InvokeHotUpdateSystemAction(onCollisionStay, collision);
        }

        void OnCollisionExit(Collision collision)
        {
            InvokeHotUpdateSystemAction(onCollisionExit, collision);
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            InvokeHotUpdateSystemAction(onCollisionEnter2D, collision);
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            InvokeHotUpdateSystemAction(onCollisionStay2D, collision);
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            InvokeHotUpdateSystemAction(onCollisionExit2D, collision);
        }

        void OnTriggerEnter(Collider other)
        {
            InvokeHotUpdateSystemAction(onTriggerEnter, other);
        }

        void OnTriggerStay(Collider other)
        {
            InvokeHotUpdateSystemAction(onTriggerStay, other);
        }

        void OnTriggerExit(Collider other)
        {
            InvokeHotUpdateSystemAction(onTriggerExit, other);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            InvokeHotUpdateSystemAction(onTriggerEnter2D, other);
        }

        void OnTriggerStay2D(Collider2D other)
        {
            InvokeHotUpdateSystemAction(onTriggerStay2D, other);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            InvokeHotUpdateSystemAction(onTriggerExit2D, other);
        }

        void GetAllUnitySystemAction()
        {
            if (HotUpdateInstance != null)
            {
                Type type = HotUpdateInstance.GetType();
                awakeAction = GetUnitySystemAction(type, "Awake");
                onEnableAction = GetUnitySystemAction(type, "OnEnable");
                startAction = GetUnitySystemAction(type, "Start");
                updateAction = GetUnitySystemAction(type, "Update");
                fixedUpdateAction = GetUnitySystemAction(type, "FixedUpdate");
                lateUpdateAction = GetUnitySystemAction(type, "LateUpdate");
                onDisableAction = GetUnitySystemAction(type, "OnDisable");
                onDestroyAction = GetUnitySystemAction(type, "OnDestroy");
                onApplicationFocus = GetUnitySystemAction(type, "OnApplicationFocus");
                onApplicationPause = GetUnitySystemAction(type, "OnApplicationPause");
                onApplicationQuit = GetUnitySystemAction(type, "OnApplicationQuit");
                onCollisionEnter = GetUnitySystemAction(type, "OnCollisionEnter");
                onCollisionStay = GetUnitySystemAction(type, "OnCollisionStay");
                onCollisionExit = GetUnitySystemAction(type, "OnCollisionExit");
                onCollisionEnter2D = GetUnitySystemAction(type, "OnCollisionEnter2D");
                onCollisionStay2D = GetUnitySystemAction(type, "OnCollisionStay2D");
                onCollisionExit2D = GetUnitySystemAction(type, "OnCollisionExit2D");
                onTriggerEnter = GetUnitySystemAction(type, "OnTriggerEnter");
                onTriggerStay = GetUnitySystemAction(type, "OnTriggerStay");
                onTriggerExit = GetUnitySystemAction(type, "OnTriggerExit");
                onTriggerEnter2D = GetUnitySystemAction(type, "OnTriggerEnter2D");
                onTriggerStay2D = GetUnitySystemAction(type, "OnTriggerStay2D");
                onTriggerExit2D = GetUnitySystemAction(type, "OnTriggerExit2D");
            }
        }

        MethodInfo GetUnitySystemAction(Type type, string actionName)
        {
            MethodInfo im = type.GetMethod(actionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (im == null && type.BaseType != null)
                return GetUnitySystemAction(type.BaseType, actionName);
            else
                return im;
        }

        void InvokeHotUpdateSystemAction(MethodInfo method, params object[] paramss)
        {
            if (method != null)
                method.Invoke(HotUpdateInstance, paramss);
        }
        #endregion
    }

    [Serializable]
    public class HotUpdateField
    {
        public string fieldName;
        public string valueStr;
        public UnityEngine.Object valueUnityObj;

        public HotUpdateField(string fieldName, string valueStr)
        {
            this.fieldName = fieldName;
            this.valueStr = valueStr;
        }

        public HotUpdateField(string fieldName, UnityEngine.Object valueUnityObj)
        {
            this.fieldName = fieldName;
            this.valueUnityObj = valueUnityObj;
        }
    }
}
