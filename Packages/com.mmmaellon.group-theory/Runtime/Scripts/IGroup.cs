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
        [OdinSerialize, HideInInspector]
        DataDictionary setsIds = new DataDictionary();

        [OdinSerialize, HideInInspector]
        DataDictionary items = new DataDictionary();

        [SerializeField, ReadOnly]
        Singleton _singleton;
        public Singleton singleton
        {
            get => _singleton;
        }
        [SerializeField, ReadOnly]
        int groupId = -1001;//1 indexed

        public int GetGroupId()
        {
            return groupId;
        }

        public void _FixUnserializedItems()
        {
            foreach (var itemId in items.GetKeys().ToArray())
            {
                items[itemId] = singleton.GetItemById(itemId.Int);
            }
        }

        public void _Setup(int groupId, Singleton singleton)
        {
            this.groupId = groupId;
            _singleton = singleton;
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
            return item && items.ContainsKey(item.GetItemId());
        }

        public bool HasItemId(int itemId)
        {
            return items.ContainsKey(itemId);
        }

        public void PrintItemDict()
        {
            var keys = items.GetKeys().ToArray();
            Debug.LogWarning("Dict:");
            foreach (var key in keys)
            {
                Item item = (Item)items[key].Reference;
                Debug.LogWarning(" " + key.Int + " - " + item);
            }
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
            if (!items.ContainsKey(item.GetItemId()))
            {
                items.Add(item.GetItemId(), item);
            }
            OnAddItem(item);
        }
        public void _OnRemoveItem(Item item)
        {
            if (items.ContainsKey(item.GetItemId()))
            {
                Debug.LogWarning("should be removing");
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
