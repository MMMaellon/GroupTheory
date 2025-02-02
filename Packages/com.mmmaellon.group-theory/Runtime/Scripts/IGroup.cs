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
        public DataDictionary setsIds = new DataDictionary();

        [OdinSerialize, HideInInspector]
        public DataList itemIds = new DataList();
        [OdinSerialize, HideInInspector]
        public DataList itemList = new DataList();

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
            itemList.Clear();
            for (int i = 0; i < itemIds.Count; i++)
            {
                itemList.Add(singleton.GetItemById(itemIds[i].Int));
            }
        }

        public void _Setup(int groupId, Singleton singleton)
        {
            this.groupId = groupId;
            _singleton = singleton;
            setsIds.Clear();
            setsIds.Add(groupId, groupId.ToString());//The set with just this group
            itemList.Clear();
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
            return item && (itemIds.BinarySearch(item.GetItemId()) >= 0);
        }

        public bool HasItemId(int itemId)
        {
            return itemIds.BinarySearch(itemId) >= 0;
        }

        public void PrintItemDict()
        {
            Debug.LogWarning("ItemList:");
            for (int i = 0; i < itemList.Count; i++)
            {
                Item item = (Item)itemList[i].Reference;
                Debug.LogWarning(" " + i + " - " + item);
            }
            Debug.LogWarning("ItemIds:");
            for (int i = 0; i < itemIds.Count; i++)
            {
                Debug.LogWarning(" " + i + " - " + itemIds[i].Int);
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

        public void _OnAddItem(Item item, int itemId)
        {
            var index = itemIds.BinarySearch(itemId);
            if (index < 0)
            {
                itemList.Insert(-1 - index, item);
                itemIds.Insert(-1 - index, itemId);
                OnAddItem(item);
            }
        }
        public void _OnRemoveItem(Item item, int itemId)
        {
            var index = itemIds.BinarySearch(itemId);
            if (index >= 0)
            {
                itemList.RemoveAt(index);
                itemIds.RemoveAt(index);
                OnRemoveItem(item);
            }
        }

        public abstract void OnAddItem(Item item);
        public abstract void OnRemoveItem(Item item);

        public DataList GetItemIds()
        {
            return itemIds;
        }

        public DataList GetItems()
        {
            return itemList;
        }
    }
}
