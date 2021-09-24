using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Puerts;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Runtime
{
    public class TscLoader : ILoader
    {
        private string root = "";
        public TscLoader() { }

        public TscLoader( string root )
        {
            this.root = root.Trim( new[] {'\\', '/'} ) + "/";
        }

        private string PathToUse( string filepath )
        {
            return filepath.EndsWith( ".cjs" )
                ? filepath.Substring( 0, filepath.Length - 4 )
                : filepath;
        }

        public bool FileExists( string filepath )
        {
            if ( File.Exists( Path.Combine( root, filepath ) ) ) {
                return true;
            }

            var resName = $"Bundle/{filepath}.txt";
            if ( Addressables.LoadResourceLocationsAsync( resName ).WaitForCompletion().Any() ) {
                return true;
            }

            string pathToUse = this.PathToUse( filepath );
            return UnityEngine.Resources.Load( pathToUse ) != null;
        }

        public string ReadFile( string filepath, out string debugpath )
        {
            debugpath = Path.GetFullPath( Path.Combine( root, filepath ) );
            if ( filepath.StartsWith( "node_modules/" ) ) {
                debugpath = Path.GetFullPath( filepath );
                if ( File.Exists( debugpath ) ) {
                    return File.ReadAllText( debugpath );
                }
                else {
                    Debug.LogError( $"{filepath} not exists" );
                }

                return null;
            }

            if ( File.Exists( debugpath ) ) {
                return File.ReadAllText( debugpath );
            }

            UnityEngine.TextAsset file = null;
            var resName = $"Bundle/{filepath}.txt";
            if ( Addressables.LoadResourceLocationsAsync( resName ).WaitForCompletion().Any() ) {
                file = Addressables.LoadAssetAsync<TextAsset>( resName ).WaitForCompletion();
            }

            if ( file == null ) {
                string pathToUse = this.PathToUse( filepath );
                file = (UnityEngine.TextAsset) UnityEngine.Resources.Load( pathToUse );
            }

        #if UNITY_EDITOR
            if ( file != null ) {
                debugpath = Path.GetFullPath( AssetDatabase.GetAssetPath( file ) );
            }
        #endif
            return file?.text;
        }
    }
}