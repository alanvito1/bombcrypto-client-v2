using CreativeSpore.SuperTilemapEditor;
using Engine.Entities;
using UnityEngine;

namespace Engine.Utils
{
    public static class PhysicsUtils
    {
        public static Entity GetEntity(Collider2D collider)
        {
            var rigidBody = collider.attachedRigidbody;
            if (rigidBody != null)
            {
                // Bolt: Optimize GetComponent with TryGetComponent to reduce allocation
                if (rigidBody.TryGetComponent<Entity>(out var entity))
                {
                    return entity;
                }
            }
            // Tilemap.
            // Bolt: Optimize GetComponent with TryGetComponent
            if (collider.TryGetComponent<TilemapChunk>(out var chunk))
            {
                var tilemap = chunk.ParentTilemap;
                if (tilemap.TryGetComponent<Entity>(out var entity))
                {
                    return entity;
                }
            }
            return null;
        }

        private static readonly Collider2D[] Colliders = new Collider2D[10];

        public static Collider2D GetCollider(Entity entity)
        {
            // Bolt: Optimize GetComponent with TryGetComponent
            if (entity.TryGetComponent<Collider2D>(out var collider))
            {
                return collider;
            }
            // Bolt: Optimize GetComponent with TryGetComponent
            if (entity.TryGetComponent<Rigidbody2D>(out var rigidBody))
            {
                var count = rigidBody.GetAttachedColliders(Colliders);
                if (count > 0)
                {
                    return Colliders[0];
                }
            }
            // Tilemap.
            // Bolt: Optimize GetComponent with TryGetComponent
            if (entity.TryGetComponent<STETilemap>(out var tilemap))
            {
                if (tilemap.transform.childCount > 0)
                {
                    // Bolt: Optimize GetComponent with TryGetComponent
                    if (tilemap.transform.GetChild(0).TryGetComponent<TilemapChunk>(out var chunk))
                    {
                        if (chunk.TryGetComponent<Collider2D>(out collider))
                        {
                            return collider;
                        }
                    }
                }
            }
            return null;
        }

    }
}
