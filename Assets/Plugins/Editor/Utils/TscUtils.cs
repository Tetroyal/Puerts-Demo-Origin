using System.IO;
using Puerts;
using UnityEditor;
using UnityEngine;

namespace Utils
{
    public static class TscUtils
    {
        [MenuItem( "NodeTSC/Run Build", false, -301 )]
        static void RunBuild()
        {
            Puerts.Editor.Generator.Menu.GenerateDTS();
            var src = Configure.GetCodeOutputDirectory() + "Typing/csharp/index.d.ts";
            var saveTo = "Packages/TsProj/Typing/csharp/index.d.ts";
            File.WriteAllText( saveTo, File.ReadAllText( src ) );
            Debug.Log( "finished." );
        }
    }
}