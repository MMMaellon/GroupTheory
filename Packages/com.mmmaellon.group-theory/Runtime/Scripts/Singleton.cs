
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;
using UnityEditor;
using System;
using VRC.Udon.Serialization.OdinSerializer;

namespace MMMaellon.GroupTheory
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Singleton : UdonSharpBehaviour
    {
        [SerializeField, ReadOnly, UdonSynced, FieldChangeCallback(nameof(setStrs))]
        string[] _setStrs = { };
        string[] setStrs
        {
            get => _setStrs;
            set
            {
                //ONLY gets run by remote players
                //new sets to parse
                for (int i = _setStrs.Length; i < value.Length; i++)
                {
                    CreateSet(value[i]);
                }
                _setStrs = value;
            }
        }

        [OdinSerialize, HideInInspector]
        DataList sets = new DataList();//list of list of group indexes

        // #if UNITY_EDITOR
        public void Start()
        {
            if (!Networking.LocalPlayer.isMaster)
            {
                // RunUnitTests();
                // SendCustomEventDelayedSeconds(nameof(RunUnitTests), 5);
                // SendCustomEventDelayedSeconds(nameof(GetFrameTime), 6);
            }
            // SendCustomEventDelayedSeconds(nameof(_PrintItemGroups), 20);
            _PrintItemGroups();
        }
        float startFrameTime;
        float endFrameTime;
        public void GetFrameTime()
        {
            startFrameTime = Time.realtimeSinceStartup;
            SendCustomEventDelayedFrames(nameof(CalcFrameTime), 0);
        }

        public void CalcFrameTime()
        {
            endFrameTime = Time.realtimeSinceStartup;
            Debug.LogWarning("TIME FOR SINGLE FRAME: " + ((endFrameTime - startFrameTime) * 1000));
        }
        public void RunUnitTests()
        {
            //Tests
            Debug.LogWarning("STARTING TESTS");
            var group1 = groups[0];
            var group2 = groups[1];
            var watch = new System.Diagnostics.Stopwatch();
            float testTime;
            var testIntList = new DataList();
            testIntList.Add(-1);
            testIntList.Add(0);
            testIntList.Add(2);
            testIntList.Add(5);
            testIntList.Add(6);
            testIntList.Add(7);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            testIntList.Add(8);
            Vector4 testVec4 = new Vector4();
            int testInt;
            IGroup group;
            int randValue;
            int randIndex;
            testIntList.Insert(testIntList.Count, 69);
            for (int i = 0; i < 1000; i++)
            {
                randValue = UnityEngine.Random.Range(0, 20);
                randIndex = BinarySearchIntList(testIntList, randValue);
                if (randIndex >= 0)
                {
                    testIntList.RemoveAt(randIndex);
                }
                else
                {
                    testIntList.Insert(-1 - randIndex, randValue);
                }
            }
            var lastValue = -1001;
            for (int i = 0; i < testIntList.Count; i++)
            {
                if (lastValue >= testIntList[i].Int)
                {
                    Debug.LogError("OH NO");
                }
                lastValue = testIntList[i].Int;
                Debug.Log(lastValue);
            }
            //Testing group
            //fizzbuzz
            string printStr;
            watch.Start();
            for (int i = 0; i < items.Length; i++)
            {
                if (i % 3 == 0)
                {
                    items[i].AddToGroup(group1);
                }
                if (i % 5 == 0)
                {
                    items[i].AddToGroup(group2);
                }
                printStr = "";
                if (items[i].IsInGroup(group1))
                {
                    printStr += " FIZZ";
                }
                if (items[i].IsInGroup(group2))
                {
                    printStr += " BUZZ";
                }
                if (i % 3 == 0 && i % 5 == 0)
                {
                    if (printStr != " FIZZ BUZZ")
                    {
                        Debug.LogError("Failed Fizz Buzz");
                    }
                }
                else
                if (i % 3 == 0)
                {
                    if (printStr != " FIZZ")
                    {
                        Debug.LogError("Failed Fizz Buzz");
                    }
                }
                else
                if (i % 5 == 0)
                {
                    if (printStr != " BUZZ")
                    {
                        Debug.LogError("Failed Fizz Buzz");
                    }
                }
                Debug.Log("item " + i + " :" + items[i].GetSetId() + printStr);
            }
            watch.Stop();
            testTime = ((watch.ElapsedTicks * 1000L * 1000L) / System.Diagnostics.Stopwatch.Frequency) / 1000f;
            Debug.LogWarning("Fizz Buzz Time: " + testTime + "ms");
            watch.Restart();
            for (int i = 0; i < 1000; i++)
            {
                var randomGroup = groups[UnityEngine.Random.Range(0, groups.Length)];
                var randomItem = items[UnityEngine.Random.Range(0, items.Length)];
                if (randomItem.IsInGroup(randomGroup))
                {
                    randomItem.RemoveFromGroup(randomGroup);
                }
                else
                {
                    randomItem.AddToGroup(randomGroup);
                }
            }
            watch.Stop();
            testTime = ((watch.ElapsedTicks * 1000L * 1000L) / System.Diagnostics.Stopwatch.Frequency) / 1000f;
            Debug.LogWarning("1000 random adds and removes" + testTime + "ms");
        }

        public void _PrintItemGroups()
        {
            Debug.LogWarning("Items:");
            for (int i = 0; i < items.Length; i++)
            {
                Debug.LogWarning("item " + i + ": " + items[i].QueuedRequestCount() + " - " + _IntListToString(items[i].GetGroupIds()));
            }
        }
        // #endif

        void CreateSet(string setStr)
        {
            DataList newSet = new DataList();
            foreach (var numStr in setStr.Split(','))
            {
                if (int.TryParse(numStr, out int groupId))
                {
                    newSet.Add(groupId);
                }
            }
            if (newSet.Count > 0)
            {
                //success
                var newSetIndex = sets.Count;
                sets.Add(newSet);
                setsLookup.Add(setStr, newSetIndex);
                for (int i = 0; i < newSet.Count; i++)
                {
                    groups[newSet[i].Int - 1]._OnNewSetCreated(newSetIndex, setStr);
                }
                foreach (var item in items)
                {
                    if (localPlayer.IsOwner(item.gameObject))
                    {
                        item._OnNewSetCreated(newSetIndex, setStr);
                    }
                    else if (item.GetSetId() == newSetIndex)
                    {
                        item._MatchSyncIdToSingleton();
                    }

                }
            }
        }

        public string GroupListToString(DataList groupList)
        {
            var setStr = "";
            for (int i = 0; i < groupList.Count - 1; i++)
            {
                setStr += ((IGroup)groupList[i].Reference).GetGroupId().ToString() + ",";
            }
            if (groupList.Count > 0)
            {
                setStr += ((IGroup)groupList[groupList.Count - 1].Reference).GetGroupId().ToString();
            }
            return setStr;
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

        public int _OnNewSetRequest(DataList newSet)
        {
            var str = _IntListToString(newSet);
            if (setsLookup.ContainsKey(str))
            {
                return setsLookup[str].Int;
            }
            var newSetStrs = new string[setStrs.Length + 1];
            var newSetId = setStrs.Length;
            Array.Copy(setStrs, newSetStrs, setStrs.Length);
            newSetStrs[newSetId] = str;
            _setStrs = newSetStrs;
            RequestSerialization();
            sets.Add(newSet.ShallowClone());
            setsLookup.Add(str, newSetId);
            return newSetId;
        }

        VRCPlayerApi localPlayer;
        public void OnEnable()
        {
            localPlayer = Networking.LocalPlayer;
        }

        [SerializeField, ReadOnly]
        Item[] items = { };
        [SerializeField, ReadOnly]
        IGroup[] groups = { };
        [OdinSerialize]
        DataDictionary setsLookup = new DataDictionary();//key is string, value is index in sets datalist

        public IGroup GetGroupById(int idValue)
        {
            if (idValue <= 0 || idValue > groups.Length)
            {
                return null;
            }
            return groups[idValue - 1];//group ids start at 1
        }

        public Item[] GetItems()
        {
            return items;
        }

        public IGroup[] GetGroups()
        {
            return groups;
        }

        public string[] GetSetStrs()
        {
            return _setStrs;
        }

        public DataList GetSets()
        {
            return sets;
        }

        public int GetSetCount()
        {
            return sets.Count;
        }

        public DataList GetSetById(int setId)
        {
            if (setId <= 0 || setId >= sets.Count)
            {
                return new DataList();
            }
            return sets[setId].DataList.ShallowClone();
        }

        public string GetSetStrById(int setId)
        {
            if (setId <= 0 || setId >= _setStrs.Length)
            {
                return "";
            }
            return _setStrs[setId];
        }

        public int GetSetIdByStr(string str)
        {
            if (setsLookup.ContainsKey(str))
            {
                return setsLookup[str].Int;
            }
            else
            {
                return -1 - sets.Count;
            }
        }

        public void SanitizeAndSortGroupList(DataList groupList)
        {
            if (groupList.Count == 0)
            {
                return;
            }
            //prune duplicates
            DataDictionary dict = new DataDictionary();
            for (int i = groupList.Count - 1; i >= 0; i--)
            {
                if (!groupList[i].IsNull && dict.ContainsKey(groupList[i]))
                {
                    groupList.RemoveAt(i);
                }
                else
                {
                    dict.Add(groupList[i], 1001);
                }
            }
        }

        public int BinarySearchIntSublist(DataList list, int target, int start, int end)
        {
            return list.BinarySearch(target, start, end);
        }

        public int BinarySearchItemSublist(DataList list, Item target, int start, int end)
        {
            if (target == null)
            {
                return -1;
            }
            while (end > start)
            {
                var mid = start + ((end - start) / 2);
                var midValue = ((IGroup)list[mid].Reference).GetGroupId();
                if (midValue == target.GetItemId())
                {
                    return mid;
                }
                else if (target.GetItemId() < midValue)
                {
                    end = mid;
                }
                else
                {
                    start = mid + 1;
                }
            }
            if (start == end)
            {
                return -start - 1;
            }
            return start;
        }

        public int BinarySearchGroupSublist(DataList list, IGroup target, int start, int end)
        {
            if (target == null)
            {
                return -1;
            }
            while (end > start)
            {
                var mid = start + ((end - start) / 2);
                var midValue = ((IGroup)list[mid].Reference).GetGroupId();
                if (midValue == target.GetGroupId())
                {
                    return mid;
                }
                else if (target.GetGroupId() < midValue)
                {
                    end = mid;
                }
                else
                {
                    start = mid + 1;
                }
            }
            if (start == end)
            {
                return -start - 1;
            }
            return start;
        }

        public int BinarySearchIntList(DataList list, int target)
        {
            return list.BinarySearch(target);
        }
        public int BinarySearchItemList(DataList list, Item target)
        {
            return BinarySearchItemSublist(list, target, 0, list.Count);
        }
        public int BinarySearchGroupList(DataList list, IGroup target)
        {
            return BinarySearchGroupSublist(list, target, 0, list.Count);
        }

        // public void _ApplyChangesToGroupIdList(DataList groupIdList, DataList changes)
        // {
        //     int change;
        //     int groupIndex;
        //     for (int i = 0; i < changes.Count; i++)
        //     {
        //         change = changes[i].Int;
        //         if (change < 0)
        //         {
        //             //remove this group
        //             groupIndex = BinarySearchIntList(groupIdList, -change);
        //             if (groupIndex >= 0)
        //             {
        //                 groupIdList.RemoveAt(groupIndex);
        //             }
        //         }
        //         else if (change > 0)
        //         {
        //             groupIndex = BinarySearchIntList(groupIdList, change);
        //             if (groupIndex < 0)
        //             {
        //                 groupIdList.Insert(-1 - groupIndex, change);
        //             }
        //         }
        //     }
        // }
        // public bool _ApplyChangeToGroupIdList(DataList groupIdList, int change)
        // {
        //     if (change == 0)
        //     {
        //         return false;
        //     }
        //     int groupIndex;
        //     if (change < 0)
        //     {
        //         //remove this group
        //         groupIndex = BinarySearchIntList(groupIdList, -change);
        //         if (groupIndex >= 0)
        //         {
        //             groupIdList.RemoveAt(groupIndex);
        //             return true;
        //         }
        //     }
        //     else if (change > 0)
        //     {
        //         groupIndex = BinarySearchIntList(groupIdList, change);
        //         if (groupIndex < 0)
        //         {
        //             groupIdList.Insert(-1 - groupIndex, change);
        //             return true;
        //         }
        //     }
        //     return false;
        // }
        // public void _ApplyChangeVectorToGroupIdList(DataList groupIdList, Vector4 changeVector)
        // {
        //     int groupIndex;
        //     for (int i = 1; i <= 3; i++)
        //     {
        //         var vectorValue = BitConverter.SingleToInt32Bits(changeVector[i]);
        //         if (vectorValue == 0)
        //         {
        //             continue;
        //         }
        //         else if (vectorValue < 0)
        //         {
        //             //remove this group
        //             groupIndex = BinarySearchIntList(groupIdList, -BitConverter.SingleToInt32Bits(changeVector[i]));
        //             if (groupIndex >= 0)
        //             {
        //                 groupIdList.RemoveAt(groupIndex);
        //             }
        //         }
        //         else
        //         {
        //             groupIndex = BinarySearchIntList(groupIdList, vectorValue);
        //             if (groupIndex < 0)
        //             {
        //                 groupIdList.Insert(-1 - groupIndex, vectorValue);
        //             }
        //         }
        //     }
        // }


#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public void AutoSetup(IGroup[] newGroups, Item[] newItems)
        {
            //because starting groups might have changed we can't quit early
            sets.Clear();
            setsLookup.Clear();
            groups = newGroups;
            items = newItems;
            _setStrs = new string[groups.Length + 1];

            //the null set
            _setStrs[0] = "";
            var blankSet = new DataList();
            sets.Add(blankSet.ShallowClone());
            setsLookup.Add("", 0);

            for (int i = 1; i < _setStrs.Length; i++)
            {
                _setStrs[i] = i.ToString();
                var newList = new DataList();
                newList.Add(i);
                sets.Add(newList);
                setsLookup.Add(i.ToString(), i);
            }
            for (int i = 0; i < groups.Length; i++)
            {
                groups[i]._Setup(i + 1, this);
                new SerializedObject(groups[i]).Update();
                PrefabUtility.RecordPrefabInstancePropertyModifications(groups[i]);
            }
            for (int i = 0; i < items.Length; i++)
            {
                items[i]._Setup(i, this);
                new SerializedObject(items[i]).Update();
                PrefabUtility.RecordPrefabInstancePropertyModifications(items[i]);
            }
            new SerializedObject(this).Update();
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        }

        // [MenuItem("MMMaellon/Test")]
        // public static void Test()
        // {
        //     Debug.LogWarning("Running TEST");
        //     Debug.LogWarning("Generating a random set of groups");
        //     Group[] cols = new Group[100];
        //     GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //     var singleton = obj.AddComponent<Singleton>();
        //     for (int i = 0; i < cols.Length; i++)
        //     {
        //         cols[i] = obj.AddComponent<Group>();
        //         cols[i].id = Random.Range(0, 100);
        //     }
        //     var str = singleton.GroupsToStr(cols);
        //     Debug.Log(str);
        //     GameObject.DestroyImmediate(obj);
        // }
#endif
    }
}
