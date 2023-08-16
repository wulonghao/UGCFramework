﻿using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.Events;

namespace UnityEngine.SignInWithApple
{
    public enum UserDetectionStatus
    {
        LikelyReal,
        Unknown,
        Unsupported
    }

    public enum UserCredentialState
    {
        Revoked,
        Authorized,
        NotFound
    }

    public struct UserInfo
    {
        public string userId;
        public string email;
        public string displayName;

        public string idToken;
        public string error;

        public UserDetectionStatus userDetectionStatus;
    }

    [System.Serializable]
    public class SignInWithAppleEvent : UnityEvent<SignInWithApple.CallbackArgs>
    {
    }

    public class SignInWithApple : MonoBehaviour
    {
        private static Callback s_LoginCompletedCallback;
        private static Callback s_CredentialStateCallback;

        private static readonly ConcurrentQueue<Action> s_EventQueue = new ConcurrentQueue<Action>();

        public struct CallbackArgs
        {
            /// <summary>
            /// The state of the user's authorization.
            /// </summary>
            public UserCredentialState credentialState;

            /// <summary>
            /// The logged in user info after the call is done.
            /// </summary>
            public UserInfo userInfo;

            /// <summary>
            /// Whether the call ends up with an error.
            /// </summary>
            public string error;
        }

        public delegate void Callback(CallbackArgs args);

        private delegate void LoginCompleted(int result, UserInfo info);

        [MonoPInvokeCallback(typeof(LoginCompleted))]
        private static void LoginCompletedCallback(int result, [MarshalAs(UnmanagedType.Struct)]UserInfo info)
        {
            var args = new CallbackArgs();
            if (result != 0)
            {
                args.userInfo = new UserInfo
                {
                    idToken = info.idToken,
                    displayName = info.displayName,
                    email = info.email,
                    userId = info.userId,
                    userDetectionStatus = info.userDetectionStatus
                };
            }
            else
            {
                args.error = info.error;
            }

            s_LoginCompletedCallback(args);
            s_LoginCompletedCallback = null;
        }

        private delegate void GetCredentialStateCompleted(UserCredentialState state);

        [MonoPInvokeCallback(typeof(GetCredentialStateCompleted))]
        private static void GetCredentialStateCallback([MarshalAs(UnmanagedType.SysInt)]UserCredentialState state)
        {
            var args = new CallbackArgs
            {
                credentialState = state
            };

            s_CredentialStateCallback(args);
            s_CredentialStateCallback = null;
        }

        #region events

        [Header("Event fired when login is complete.")]
        public SignInWithAppleEvent onLogin;

        [Header("Event fired when the users credential state has been retrieved.")]
        public SignInWithAppleEvent onCredentialState;

        [Header("Event fired when there is an error.")]
        public SignInWithAppleEvent onError;

        #endregion

        /// <summary>
        /// Get credential state and trigger onCredentialState or onError event when action is completed.
        /// </summary>
        /// <param name="userID">The user id to query the credential state on.</param>
        public void GetCredentialState(string userID)
        {
            GetCredentialState(userID, TriggerCredentialStateEvent);
        }

        /// <summary>
        /// Invoke login and provide a custom callback when action is completed.
        /// When a custom trigger is used, the onCredentialState or onError unity event won't trigger.
        /// </summary>
        /// <param name="userID">The user id to query the credential state on.</param>
        /// <param name="callback">The custom callback to trigger when action is completed.</param>
        public void GetCredentialState(string userID, Callback callback)
        {
            if (s_CredentialStateCallback != null)
                throw new InvalidOperationException("Credential state fetch called while another request is in progress");
            s_CredentialStateCallback = callback;

            GetCredentialStateInternal(userID);
        }

        private void GetCredentialStateInternal(string userID)
        {
#if (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
            IntPtr cback = IntPtr.Zero;
            GetCredentialStateCompleted d = GetCredentialStateCallback;
            cback = Marshal.GetFunctionPointerForDelegate(d);

            ExtendDeclaration.UnitySignInWithApple_GetCredentialState(userID, cback);
#endif
        }

        /// <summary>
        /// Invoke login and trigger onLogin or onError event when login is completed.
        /// </summary>
        public void Login()
        {
            Login(TriggerOnLoginEvent);
        }

        /// <summary>
        /// Invoke login and provide a custom callback when login is completed.
        /// When a custom trigger is used, the onLogin or onError unity event won't trigger.
        /// </summary>
        /// <param name="callback">The custom callback to trigger when login is completed.</param>
        public void Login(Callback callback)
        {
            if (s_LoginCompletedCallback != null)
                throw new InvalidOperationException("Login called while another login is in progress");
            s_LoginCompletedCallback = callback;

            LoginInternal();
        }

        private void LoginInternal()
        {
#if (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
            IntPtr cback = IntPtr.Zero;
            LoginCompleted d = LoginCompletedCallback;
            cback = Marshal.GetFunctionPointerForDelegate(d);

            ExtendDeclaration.UnitySignInWithApple_Login(cback);
#endif
        }

        private void TriggerOnLoginEvent(CallbackArgs args)
        {
            if (args.error != null)
            {
                TriggerOnErrorEvent(args);
                return;
            }

            s_EventQueue.Enqueue(delegate ()
            {
                if (onLogin != null)
                {
                    onLogin.Invoke(args);
                }
            });
        }

        private void TriggerCredentialStateEvent(CallbackArgs args)
        {
            if (args.error != null)
            {
                TriggerOnErrorEvent(args);
                return;
            }

            s_EventQueue.Enqueue(delegate ()
            {
                if (onCredentialState != null)
                {
                    onCredentialState.Invoke(args);
                }
            });
        }

        private void TriggerOnErrorEvent(CallbackArgs args)
        {
            s_EventQueue.Enqueue(delegate ()
            {
                if (onError != null)
                {
                    onError.Invoke(args);
                }
            });
        }

        public void Update()
        {
            Action action;
            while (s_EventQueue.TryDequeue(out action))
            {
                action.Invoke();
            }
        }
    }
}
