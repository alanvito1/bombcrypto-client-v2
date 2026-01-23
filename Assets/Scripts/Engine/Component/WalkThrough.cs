using System.Collections;
using System.Collections.Generic;
using Engine.Entities;
using UnityEngine;

namespace Engine.Components
{

    public class WalkThrough : EntityComponentV2
    {
        [SerializeField]
        private bool throughBrick;
        public bool ThroughBrick
        {
            set
            {
                throughBrick = value;
            }

            get => throughBrick;
        }

        [SerializeField]
        private bool throughBomb;
        public bool ThroughBomb
        {
            set
            {
                throughBomb = value;
            }

            get => throughBomb;
        }

        [SerializeField]
        private bool throughWall;
        public bool ThroughWall {
            set {
                throughWall = value;
            }
            get => throughWall;
        }
        
        private EntityType _entityType;
        private Movable _movable;
        public bool destroyMode;

            
        public WalkThrough(EntityType entityType,  Movable movable) {
            _entityType = entityType;
            _movable = movable;
        }

        public void HitObstacle(Entity obstacle)
        {
            // Do not force stop when player hit bomb...in encounterListener... let it do in PlayerBombListener...
            if (_entityType == EntityType.BomberMan && obstacle.Type == EntityType.Bomb)
            {
                return;
            }
            //-----------

            // Optimization: Use pattern matching (is) instead of GetComponent to avoid O(N) lookup.
            if (ThroughBrick)
            {
                if (obstacle is SoftBlock)
                {
                    return;
                }
            }

            if (ThroughBomb)
            {
                if (obstacle is Bomb bomb)
                {
                    if (destroyMode)
                    {
                        bomb.StartExplode(obstacle.transform.localPosition);
                    }
                    return;
                }

                if (obstacle is BombExplosion)
                {
                    return;
                }
            }

            if (ThroughWall) {
                if (obstacle is Wall) {
                    return;
                }
            }

            _movable?.ForceStop();

        }
    }
}
