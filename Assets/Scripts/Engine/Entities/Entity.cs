using UnityEngine;

using Engine.Manager;

using UnityEngine.Assertions;

using CodeStage.AntiCheat.ObscuredTypes;

using DG.Tweening;

using IEntityComponent = Engine.Components.IEntityComponent;

namespace Engine.Entities {
    
    /// <summary>
    /// Base class for all game entities in the Entity Component System (ECS).
    /// Manages lifecycle state (Alive/Dead), components, and integration with the EntityManager.
    /// </summary>
    public class Entity : MonoBehaviour, IEntity {
        /// <summary>
        /// Gets or sets the type of the entity (e.g., Player, Enemy, Bomb).
        /// </summary>
        public EntityType Type { get; set; }

        /// <summary>
        /// Gets the spatial index tree node for this entity.
        /// </summary>
        public IndexTree Index { get; } = new IndexTree();

        /// <summary>
        /// Reference to the manager that controls this entity.
        /// </summary>
        public IEntityManager EntityManager { get; set; }

        /// <summary>
        /// Gets a value indicating whether the entity is currently alive.
        /// Uses ObscuredBool for anti-cheat protection.
        /// </summary>
        public ObscuredBool IsAlive { get; private set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the entity is immune to damage/death.
        /// </summary>
        public ObscuredBool Immortal { get; set; } = false;

        private readonly ComponentContainer _componentContainer = new ComponentContainer();

        private void OnDestroy() {
            DOTween.Kill(transform, true);
        }

        /// <summary>
        /// Deactivates the entity without destroying it (e.g., returning to pool).
        /// </summary>
        public void DeActive() //wait in a queue = > not active 
        {
            IsAlive = false;
        }

        /// <summary>
        /// Attempts to bring a dead entity back to life.
        /// </summary>
        /// <returns>True if resurrection was successful; False if already alive.</returns>
        public bool Resurrect() //phuc sinh
        {
            if (IsAlive) {
                return false;
            }
            Assert.IsTrue(!IsAlive);
            IsAlive = true;
            return true;
        }

        /// <summary>
        /// Kills the entity.
        /// </summary>
        /// <param name="trigger">If true, triggers associated death events/effects.</param>
        /// <returns>True if the entity was successfully killed; False if already dead.</returns>
        public bool Kill(bool trigger) {
            if (!IsAlive) {
                return false;
            }
            Assert.IsTrue(IsAlive);

            PlayKillSound();
            IsAlive = false;
            EntityManager.MarkDestroy(this, trigger);
            return true;
        }

        /// <summary>
        /// Adds a logic component to this entity.
        /// </summary>
        /// <typeparam name="T">The type of component to add.</typeparam>
        /// <param name="component">The component instance.</param>
        public void AddEntityComponent<T> (IEntityComponent component) where T : IEntityComponent {
            _componentContainer.AddComponent<T>(component);
        }
        
        /// <summary>
        /// Retrieves a logic component from this entity.
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve.</typeparam>
        /// <returns>The component instance, or null if not found.</returns>
        public T GetEntityComponent<T>() where T : IEntityComponent {
            return _componentContainer.GetComponent<T>();
        }
        
        private void PlayKillSound() {
            //if (Type == EntityType.Bubbles || Type == EntityType.Doria)
            //{
            //    EE.ServiceLocator.Resolve<IAudioManager>().PlaySound(Audio.BossDestroy);
            //}
        }
    }

    /// <summary>
    /// An entity that tracks its location hash, used for spatial partitioning.
    /// </summary>
    public class EntityLocation : Entity {
        public int HashLocation { get; set; } = 0;
    }
}