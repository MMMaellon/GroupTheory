
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Serialization.OdinSerializer;

namespace MMMaellon.GroupTheory
{
    // [AddComponentMenu("Item (Group Theory)")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Item : UdonSharpBehaviour
    {
        public int maxIterationsPerFrame = 5;
        public IGroup[] startingGroups = { };
        [SerializeField, HideInInspector]
        IGroup[] prevStartingGroups = { };
        [SerializeField, ReadOnly]
        int itemId = 0;
        [SerializeField, ReadOnly]
        Singleton _singleton;
        public Singleton singleton
        {
            get => _singleton;
        }
        [SerializeField, ReadOnly, UdonSynced, FieldChangeCallback(nameof(dataVec))]
        Vector4 _dataVec = Vector4.zero;
        [OdinSerialize, HideInInspector]
        public DataList set = new DataList();//list of group ids
        [OdinSerialize, HideInInspector]
        public DataList prevSet = new DataList();
        [OdinSerialize, HideInInspector]
        public DataList intermediateSet = new DataList();
        [OdinSerialize, HideInInspector]
        public DataList targetSet = new DataList();
        string intermediateSetStr = "";
        string targetSetStr = "";

        public override void OnPreSerialization()
        {
            if (!HasActiveRequest())
            {
                intermediateSetStr = "";
                targetSetStr = "";
                return;
            }
            targetSetStr = _IntListToString(targetSet);
            var targetSetIndex = _singleton.GetSetIdByStr(targetSetStr);
            if (targetSetIndex < 0 && player.IsOwner(_singleton.gameObject))
            {
                targetSetIndex = _singleton._OnNewSetRequest(targetSet, targetSetStr);
                //guaranteed to be 0 or greater
            }
            if (targetSetIndex >= 0)
            {
                _dataVec = new Vector4(BitConverter.Int32BitsToSingle(targetSetIndex), 0, 0, 0);
                set = targetSet.ShallowClone();
                intermediateSetStr = "";
                targetSetStr = "";
            }
            else
            {
                _LoadRequestIntoData();
            }
        }

        public string PrintDataVec()
        {
            return "[" + BitConverter.SingleToInt32Bits(_dataVec[0]) + ", " + BitConverter.SingleToInt32Bits(_dataVec[1]) + ", " + BitConverter.SingleToInt32Bits(_dataVec[2]) + ", " + BitConverter.SingleToInt32Bits(_dataVec[3]) + "]";
        }

        Vector4 dataVec
        {
            get => _dataVec;
            set
            {
                _dataVec = value;
                _MatchSyncIdToSingleton();
            }
        }

        public void _MatchSyncIdToSingleton()
        {
            var setId = BitConverter.SingleToInt32Bits(_dataVec[0]);
            if (setId >= _singleton.GetSetCount())
            {
                //didn't sync newly created set yet
                return;
            }
            prevSet = set.ShallowClone();
            set = _singleton.GetSetById(setId);
            targetSet = set.ShallowClone();
            if (_dataVec[1] != 0)
            {
                _ApplyChangeToIntList(targetSet, BitConverter.SingleToInt32Bits(_dataVec[1]));
                _ApplyChangeToIntList(targetSet, BitConverter.SingleToInt32Bits(_dataVec[2]));
                _ApplyChangeToIntList(targetSet, BitConverter.SingleToInt32Bits(_dataVec[3]));
                if (player.IsOwner(_singleton.gameObject))
                {
                    _singleton._OnNewSetRequest(targetSet, _IntListToString(targetSet));
                }
            }
            _HandleAddsAndRemoves();
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if (player != null && player.IsValid())
            {
                if (player.isLocal)
                {
                    intermediateSetStr = _IntListToString(intermediateSet);
                    targetSetStr = intermediateSetStr;
                }
                else
                {
                    intermediateSetStr = "";
                    targetSetStr = "";
                }
                requests.Clear();
            }
        }

        public int GetSetId()
        {
            return BitConverter.SingleToInt32Bits(_dataVec[0]);
        }

        public DataList GetGroupIds()
        {
            return targetSet.ShallowClone();
        }

        public DataList GetGroups()
        {
            var groups = targetSet.ShallowClone();
            for (int i = 0; i < groups.Count; i++)
            {
                groups[i] = singleton.GetGroupById(groups[i].Int);
            }
            return groups;
        }

        public bool HasActiveRequest()
        {
            return _dataVec[1] != 0 || requests.Count > 0;
        }

        public int QueuedRequestCount()
        {
            return requests.Count;
        }

        public bool IsInGroup(IGroup group)
        {
            // if (group)
            // {
            //     if (HasActiveRequest())
            //     {
            //         return targetSet.BinarySearch(group.GetGroupId()) >= 0;
            //     }
            //     return group.IsPartOfSet(GetSetId());
            // }
            // return false;

            return group && group.HasItemId(itemId);
        }

        public int GetItemId()
        {
            return itemId;
        }

        DataList requests = new DataList();
        VRCPlayerApi _player;
        VRCPlayerApi player
        {
            get
            {
                if (_player == null)
                {
                    _player = Networking.LocalPlayer;
                }
                return _player;
            }
        }

        public void AddToGroup(IGroup group)
        {
            if (!group || !group.CanAddItem(this))
            {
                return;
            }
            if (!player.IsOwner(gameObject))
            {
                Networking.SetOwner(player, gameObject);
            }
            if (_AddToIntList(targetSet, group.GetGroupId()))
            {
                requests.Add(group.GetGroupId());
                group._OnAddItem(this, itemId);
                OnAddToGroup(group);
                RequestSerialization();
            }
        }

        public virtual void OnAddToGroup(IGroup group)
        {

        }

        public void RemoveFromGroup(IGroup group)
        {
            if (!group || !group.CanRemoveItem(this))
            {
                return;
            }
            if (!player.IsOwner(gameObject))
            {
                Networking.SetOwner(player, gameObject);
            }
            if (_RemoveFromIntList(targetSet, group.GetGroupId()))
            {
                requests.Add(-group.GetGroupId());
                group._OnRemoveItem(this, itemId);
                OnRemoveFromGroup(group);
                RequestSerialization();
            }
        }

        public virtual void OnRemoveFromGroup(IGroup group)
        {

        }

        string _IntListToString(DataList intList)
        {
            var arr = new string[intList.Count];
            for (int i = 0; i < intList.Count; i++)
            {
                arr[i] = intList[i].Int.ToString();
            }
            return string.Join(',', arr);
        }

        bool _ApplyChangeToIntList(DataList intList, int change)
        {
            if (change > 0)
            {
                return _AddToIntList(intList, change);
            }
            else if (change < 0)
            {

                return _RemoveFromIntList(intList, -change);
            }
            return false;
        }

        void _ApplyChangesToGroupIdList(DataList groupIdList, DataList changes)
        {
            int change;
            int groupIndex;
            for (int i = 0; i < changes.Count; i++)
            {
                change = changes[i].Int;
                if (change < 0)
                {
                    //remove this group
                    groupIndex = groupIdList.BinarySearch(-change);
                    if (groupIndex >= 0)
                    {
                        groupIdList.RemoveAt(groupIndex);
                    }
                }
                else if (change > 0)
                {
                    groupIndex = groupIdList.BinarySearch(change);
                    if (groupIndex < 0)
                    {
                        groupIdList.Insert(-1 - groupIndex, change);
                    }
                }
            }
        }

        bool _AddToIntList(DataList intList, int intToAdd)
        {
            var searchIndex = intList.BinarySearch(intToAdd);
            if (searchIndex < 0)
            {
                intList.Insert(-1 - searchIndex, intToAdd);
                return true;
            }
            return false;
        }

        bool _RemoveFromIntList(DataList intList, int intToRemove)
        {
            var searchIndex = intList.BinarySearch(intToRemove);
            if (searchIndex >= 0)
            {
                intList.RemoveAt(searchIndex);
                return true;
            }
            return false;
        }

        public void _HandleAddsAndRemoves()
        {
            var prevIndex = 0;
            var nextIndex = 0;
            while (true)
            {
                if (prevIndex < prevSet.Count && nextIndex < targetSet.Count)
                {
                    if (prevSet[prevIndex] == targetSet[nextIndex])
                    {
                        //no change
                        prevIndex++;
                        nextIndex++;
                    }
                    else
                    {
                        var prevGroupId = prevSet[prevIndex].Int;
                        var nextGroupId = targetSet[nextIndex].Int;
                        if (prevGroupId < nextGroupId)
                        {
                            var prevGroup = _singleton.GetGroupById(prevGroupId);
                            //something was removed
                            prevGroup._OnRemoveItem(this, itemId);
                            OnRemoveFromGroup(prevGroup);
                            prevIndex++;
                        }
                        else
                        {
                            var nextGroup = _singleton.GetGroupById(nextGroupId);
                            //something was added
                            nextGroup._OnAddItem(this, itemId);
                            OnAddToGroup(nextGroup);
                            nextIndex++;
                        }
                    }
                }
                else if (nextIndex < targetSet.Count)
                {
                    //reached end of previous set. All these ones are new now
                    var nextGroup = _singleton.GetGroupById(targetSet[nextIndex].Int);
                    nextGroup._OnAddItem(this, itemId);
                    OnAddToGroup(nextGroup);
                    nextIndex++;
                }
                else if (prevIndex < prevSet.Count)
                {
                    //reached end of new sets, all these are ones that were removed
                    var prevGroup = _singleton.GetGroupById(prevSet[prevIndex].Int);
                    prevGroup._OnRemoveItem(this, itemId);
                    OnRemoveFromGroup(prevGroup);
                    prevIndex++;
                }
                else
                {
                    //reached the end of both lists
                    break;
                }
            }
        }

        public string _GetTargetStr()
        {
            return targetSetStr;
        }

        public void _OnNewSetCreated(int newSetIndex, string newSetStr)
        {
            //Handle adds and removes when they're added to the request queue
            if (targetSetStr == newSetStr)
            {
                _dataVec = new Vector4(BitConverter.Int32BitsToSingle(newSetIndex), 0, 0, 0);
                set = _singleton.GetSetById(newSetIndex);
                intermediateSetStr = "";
                targetSetStr = "";
                RequestSerialization();
            }
            else if (intermediateSetStr == newSetStr)
            {
                _dataVec = new Vector4(BitConverter.Int32BitsToSingle(newSetIndex), 0, 0, 0);
                set = _singleton.GetSetById(newSetIndex);
                intermediateSetStr = "";
                RequestSerialization();
            }
        }

        void _LoadRequestIntoData()
        {
            if (intermediateSetStr != "")
            {
                //active request never got resolved or there are no new requests
                return;
            }
            int newSetIndex;
            DataList sublist;
            for (int i = 0; maxIterationsPerFrame <= 0 || i < maxIterationsPerFrame; i++)
            {
                //pop just the one's we're working with
                sublist = requests.GetRange(0, Mathf.Min(requests.Count, 3));
                requests.RemoveRange(0, sublist.Count);

                intermediateSet = set.ShallowClone();
                _ApplyChangesToGroupIdList(intermediateSet, sublist);
                intermediateSetStr = _IntListToString(intermediateSet);
                newSetIndex = _singleton.GetSetIdByStr(intermediateSetStr);
                if (newSetIndex < 0)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (j < sublist.Count)
                        {
                            _dataVec[j + 1] = BitConverter.Int32BitsToSingle(sublist[j].Int);
                        }
                        else
                        {
                            _dataVec[j + 1] = 0;
                        }
                    }
                    RequestSerialization();
                    return;
                }
                else
                {
                    //match found for intermediate set
                    _dataVec = new Vector4(BitConverter.Int32BitsToSingle(newSetIndex), 0, 0, 0);
                    set = intermediateSet.ShallowClone();
                    intermediateSetStr = "";
                }
                if (requests.Count == 0)
                {
                    return;
                }
            }
            if (requests.Count > 0)
            {
                //we need to go again next frame
                //the reason why we don't do it all this frame is because generating the set strings is costly and I don't want to cause lag when we can just spread it over several frames
                //also, realistically how often is someone going to add an object to that many groups all at once
                SendCustomEventDelayedFrames(nameof(_DelayedLoadRequest), 0);
            }
        }

        void _DelayedLoadRequest()
        {
            ;
            if (!player.IsOwner(gameObject))
            {
                return;
            }
            if (player.IsOwner(_singleton.gameObject))
            {
                _dataVec = new Vector4(BitConverter.Int32BitsToSingle(_singleton._OnNewSetRequest(targetSet, _IntListToString(targetSet))), 0, 0, 0);
                set = targetSet.ShallowClone();
                intermediateSetStr = "";
                targetSetStr = "";
                RequestSerialization();
            }
            else
            {
                _LoadRequestIntoData();
                if (requests.Count <= 9)
                {
                    RequestSerialization();
                }
            }
        }

#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public void SetupStartingGroups()
        {
            var startingGroupIds = new DataList();
            var sortedStartingGroupIds = new DataList();
            int binarySearchIndex;
            foreach (var group in startingGroups)
            {
                if (group)
                {
                    binarySearchIndex = sortedStartingGroupIds.BinarySearch(group.GetGroupId());
                    if (binarySearchIndex < 0)
                    {
                        startingGroupIds.Add(group.GetGroupId());
                        sortedStartingGroupIds.Insert(-1 - binarySearchIndex, group.GetGroupId());
                    }
                }
            }
            if (startingGroupIds.Count == 0)
            {
                _dataVec = Vector4.zero;
                set.Clear();
                prevSet.Clear();
                intermediateSet.Clear();
                targetSet.Clear();
                return;
            }

            IGroup tempGroup;
            for (int i = 0; i < startingGroupIds.Count; i++)
            {

                tempGroup = _singleton.GetGroupById(startingGroupIds[i].Int);
                tempGroup._OnAddItem(this, itemId);
                OnAddToGroup(tempGroup);
            }

            var setId = _singleton._OnNewSetRequest(sortedStartingGroupIds, _IntListToString(sortedStartingGroupIds));
            set = sortedStartingGroupIds.ShallowClone();
            targetSet = set.ShallowClone();
            intermediateSet = set.ShallowClone();
            _dataVec = new Vector4(BitConverter.Int32BitsToSingle(setId), 0, 0, 0);
        }

        public void _Setup(int newId, Singleton newSingleton)
        {
            itemId = newId;
            _singleton = newSingleton;
            SetupStartingGroups();
        }
#endif
    }
}
