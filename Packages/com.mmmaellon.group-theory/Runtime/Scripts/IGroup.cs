using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Serialization.OdinSerializer;

namespace MMMaellon.GroupTheory
{
    public abstract class IGroup : UdonSharpBehaviour
    {
        [SerializeField, OdinSerialize, HideInInspector]
        DataDictionary setsIds = new DataDictionary();

        [SerializeField, OdinSerialize, HideInInspector]
        DataDictionary items = new DataDictionary();

        [SerializeField, ReadOnly]
        Singleton singleton;
        [SerializeField, ReadOnly]
        int groupId = -1001;//1 indexed

        public int GetGroupId()
        {
            return groupId;
        }

        public void _Setup(int groupId, Singleton singleton)
        {
            this.groupId = groupId;
            this.singleton = singleton;
            setsIds.Clear();
            setsIds.Add(groupId, groupId.ToString());//The set with just this group
            items.Clear();
        }

        public DataList GetSetIds()
        {
            return setsIds.GetKeys();
        }

        public virtual bool CanAddItem(Item item)
        {
            return true;
        }

        public virtual bool CanRemoveItem(Item item)
        {
            return true;
        }

        public void AddItem(Item item)
        {
            if (item)
            {
                item.AddToGroup(this);
            }
        }

        public void RemoveItem(Item item)
        {
            if (item)
            {
                item.RemoveFromGroup(this);
            }
        }

        public bool HasItem(Item item)
        {
            // if (item)
            // {
            //     return item.IsInGroup(this);
            // }
            return item && items.ContainsKey(item.GetItemId());
        }

        public bool IsPartOfSet(int setId)
        {
            return setsIds.ContainsKey(setId);
        }
        public virtual void _OnNewSetCreated(int newSetIndex, string newSetStr)
        {
            setsIds.Add(newSetIndex, newSetStr);
        }

        public void _OnAddItem(Item item)
        {
            if (item && !items.ContainsKey(item.GetItemId()))
            {
                items.Add(item.GetItemId(), item);
            }
            OnAddItem(item);
        }
        public void _OnRemoveItem(Item item)
        {
            if (item && items.ContainsKey(item.GetItemId()))
            {
                items.Remove(item.GetItemId());
            }
            OnRemoveItem(item);
        }

        public abstract void OnAddItem(Item item);
        public abstract void OnRemoveItem(Item item);

        public DataList GetItemIds()
        {
            return items.GetKeys();
        }

        public DataList GetItems()
        {
            return items.GetValues();
        }
    }
}
