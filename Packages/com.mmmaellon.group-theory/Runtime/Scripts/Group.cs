using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;


namespace MMMaellon.GroupTheory
{
    [AddComponentMenu("Group (Group Theory)")]
    public class Group : IGroup
    {
        [Header("On Added Triggers")]
        public UdonBehaviour[] onAddedBehaviours = { };
        public string[] onAddedEvents = { };
        [Header("On Removed Triggers")]
        public UdonBehaviour[] onRemovedBehaviours = { };
        public string[] onRemovedEvents = { };
        [ReadOnly]
        public Item lastAddedItem;
        [ReadOnly]
        public Item lastRemovedItem;
        public override void OnAddItem(Item item)
        {
            lastAddedItem = item;
            for (int i = 0; i < onAddedBehaviours.Length; i++)
            {
                if (i >= onAddedEvents.Length || !onAddedBehaviours[i])
                {
                    return;
                }
                onAddedBehaviours[i].SendCustomEvent(onAddedEvents[i]);
            }
        }

        public override void OnRemoveItem(Item item)
        {
            lastRemovedItem = item;
            for (int i = 0; i < onRemovedBehaviours.Length; i++)
            {
                if (i >= onRemovedEvents.Length || !onRemovedBehaviours[i])
                {
                    return;
                }
                onRemovedBehaviours[i].SendCustomEvent(onRemovedEvents[i]);
            }

        }
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public void OnValidate()
        {
            if (onAddedBehaviours.Length < onAddedEvents.Length)
            {
                var newEvents = new string[onAddedBehaviours.Length];
                Array.Copy(onAddedEvents, newEvents, newEvents.Length);
                onAddedEvents = newEvents;
            }
            else if (onAddedBehaviours.Length > onAddedEvents.Length)
            {
                var newEvents = new string[onAddedBehaviours.Length];
                Array.Copy(onAddedEvents, newEvents, onAddedEvents.Length);
                var newEventName = onAddedEvents.Length > 0 ? onAddedEvents[onAddedEvents.Length - 1] : "OnAddItem";
                Array.Fill(newEvents, newEventName, onAddedEvents.Length, newEvents.Length - onAddedEvents.Length);
                onAddedEvents = newEvents;
            }
            if (onRemovedBehaviours.Length < onRemovedEvents.Length)
            {
                var newEvents = new string[onRemovedBehaviours.Length];
                Array.Copy(onRemovedEvents, newEvents, newEvents.Length);
                onRemovedEvents = newEvents;
            }
            else if (onRemovedBehaviours.Length > onRemovedEvents.Length)
            {
                var newEvents = new string[onRemovedBehaviours.Length];
                Array.Copy(onRemovedEvents, newEvents, onRemovedEvents.Length);
                var newEventName = onRemovedEvents.Length > 0 ? onRemovedEvents[onRemovedEvents.Length - 1] : "OnRemoveItem";
                Array.Fill(newEvents, newEventName, onRemovedEvents.Length, newEvents.Length - onRemovedEvents.Length);
                onRemovedEvents = newEvents;
            }
        }
#endif
    }
}
