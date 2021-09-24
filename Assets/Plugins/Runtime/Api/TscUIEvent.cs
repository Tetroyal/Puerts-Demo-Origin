using System;
using System.Linq;
using System.Reflection;
using Puerts;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif
using UnityEngine;
using UnityEngine.Assertions;

namespace Runtime
{
    [ExecuteAlways]
    public class TscUIEvent : MonoBehaviour
    {
        static JsEnv jsEnv;
        public static JsEnv env => GETEnv();
        public static string distPath = "Packages/TsProj/dist/";
        public static int debugPort = 9229;
        static TscUIEvent m_Instance;

        public static TscUIEvent instance =>
            m_Instance ?? ( m_Instance = FindObjectOfType<TscUIEvent>( true ) );

        bool inited { get; set; }

        [SerializeField]
        bool reportTick;

    #if UNITY_EDITOR
        [DidReloadScripts]
        static void Restart() => instance.Start();
    #endif

        void Start()
        {
            if ( !inited ) {
                var init = env.Eval<Action<MonoBehaviour>>( "require('UIEvent').init" );
                init?.Invoke( this );
            }

        #if UNITY_EDITOR
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        #endif
        }

        void Awake()
        {
            m_Instance ??= this;
        }

        static Assembly GetAssemblyByName( string name )
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SingleOrDefault( assembly => assembly.GetName().Name == name );
        }

        static void AutoUsing()
        {
            var type = GetAssemblyByName( "Assembly-CSharp" )
                .GetType( "PuertsStaticWrap.AutoStaticCodeUsing" );
            var mi = type?.GetMethod( "AutoUsing", BindingFlags.Static | BindingFlags.Public );
            Assert.IsNotNull( mi, "mi != null" );
            mi?.Invoke( null, new object[] {jsEnv} );
        }

        public static JsEnv GETEnv()
        {
            if ( jsEnv == null || jsEnv.isolate == IntPtr.Zero ) {
                jsEnv = new JsEnv( new TscLoader( distPath ), debugPort,
                    Application.isEditor ? JsEnvMode.Node : JsEnvMode.Default );
                AutoUsing();
                //jsEnv.UsingAction<bool>(); //toggle.onValueChanged用到
            }

            return jsEnv;
        }

        [Button]
        void ReloadEnv()
        {
            env.Dispose();
            env.Eval( "require('QuickStart')" );
        }

        void Update()
        {
            if ( jsEnv != null && jsEnv.isolate != IntPtr.Zero ) {
                if ( reportTick ) {
                    Debug.Log( DateTimeOffset.Now.ToUnixTimeSeconds() );
                }

                jsEnv.Tick();
            }
        }

    #if UNITY_EDITOR
        [MenuItem( "NodeTSC/Quick Start" )]
        static void RunQuickStart()
        {
            env.Eval<Action>( "require('QuickStart').default" ).Invoke();
        }
    #endif
    }
}