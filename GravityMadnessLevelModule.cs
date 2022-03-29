using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GravityMadness
{
    public class GravityMadnessLevelModule : LevelModule
    {
        public bool customGravityOn;
        public Vector3 defaultGravity;
        public Vector3 currentGravity;
        public bool useDungeonRooms = true;
        public bool shiftEveryXSecs = true;
        public bool shiftGravity = true;
        public float timer;
        public float timeToChange = 7f;

        public override IEnumerator OnLoadCoroutine()
        {
            EventManager.onPossess += EventManager_onPossess;
            EventManager.onUnpossess += EventManager_onUnpossess;
            return base.OnLoadCoroutine();
        }

        private void EventManager_onUnpossess(Creature creature, EventTime eventTime)
        {
            if (useDungeonRooms)
            { 
                if (Level.current.dungeon != null)
                {
                    Level.current.dungeon.onPlayerChangeRoom -= Dungeon_onPlayerChangeRoom;
                }
            }
            Physics.gravity = defaultGravity;
        }

        private void EventManager_onPossess(Creature creature, EventTime eventTime)
        {
            if (useDungeonRooms)
            {
                if (Level.current.dungeon != null)
                {
                    Level.current.dungeon.onPlayerChangeRoom += Dungeon_onPlayerChangeRoom;
                }
            }
            defaultGravity = Physics.gravity;
            timer = 0f;
        }

        private void Dungeon_onPlayerChangeRoom(Room oldRoom, Room newRoom)
        {
            if(customGravityOn != true && oldRoom.index == 0)
            {
                customGravityOn = true;
            }
            if (oldRoom.index < newRoom.index && newRoom.exitDoor != null)
            {
                ShiftGravity(2f);
            }
            else
            {
                customGravityOn = false;
                Physics.gravity = defaultGravity;
            }
        }

        public override void Update()
        {
            base.Update();
            timer += Time.deltaTime;
            if(shiftEveryXSecs && timer >= timeToChange && (Level.current.data.id != "CharacterSelection" && Level.current.data.id != "Home"))
            {
                customGravityOn = true;
                currentGravity = ShiftGravity(2f);
                timer = 0f;
            }
            if(customGravityOn)
            {
                foreach (Creature creature in Creature.allActive)
                { 
                    creature.locomotion.SetPhysicModifier(this, 5, 0f);
                    creature.locomotion.rb.AddForce(currentGravity, ForceMode.Acceleration);
                }
                foreach (Item item in Item.allActive.Where(itemnKinematic => !itemnKinematic.rb.isKinematic))
                {
                    foreach(CollisionHandler collisionHandler in  item.collisionHandlers)
                    {
                        collisionHandler.SetPhysicModifier(this, 5, 0);
                        collisionHandler.rb.AddForce(currentGravity, ForceMode.Acceleration);
                    }
                }
                Player.local.locomotion.SetPhysicModifier(this, 5, 0f);
                Player.local.locomotion.rb.AddForce(currentGravity, ForceMode.Acceleration);
            }
            else
            {
                foreach (Creature creature in Creature.allActive)
                {
                    creature.locomotion.RemovePhysicModifier(this);
                }
                foreach (Item item in Item.allActive.Where(itemnKinematic => !itemnKinematic.rb.isKinematic))
                {
                    foreach (CollisionHandler collisionHandler in item.collisionHandlers)
                    {
                        collisionHandler.RemovePhysicModifier(this);
                    }
                }
                Player.local.locomotion.RemovePhysicModifier(this);
            }
        }

        private Vector3 ShiftGravity(float coefficient)
        {
            //Physics.gravity = new Vector3(Random.Range(-9.81f * coefficient, 9.81f * coefficient), Random.Range(-9.81f * 2f, 9.81f * coefficient), Random.Range(-9.81f * 2f, 9.81f * coefficient));
            return new Vector3(Random.Range(-9.81f * coefficient, 9.81f * coefficient), Random.Range(-9.81f * 2f, 9.81f * coefficient), Random.Range(-9.81f * 2f, 9.81f * coefficient));
        }
    }
}
