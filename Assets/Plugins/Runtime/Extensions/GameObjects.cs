using UnityEngine;

namespace Runtime
{
    public static class GameObjects
    {
        public static void SayHello( this GameObject go ) =>
            Debug.Log( $"Hello, {go.name} in {go.scene.name}" );
    }
}