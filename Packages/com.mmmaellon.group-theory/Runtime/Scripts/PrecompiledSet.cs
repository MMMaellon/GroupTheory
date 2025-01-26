
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.GroupTheory
{
    public class PrecompiledSet : UdonSharpBehaviour
    {
        public IGroup[] groups;
        public void _Precompile(Singleton singleton)
        {
            DataList sortedGroupIds = new DataList();
            int binarySearchIndex;
            foreach (var group in groups)
            {
                if (group)
                {
                    binarySearchIndex = sortedGroupIds.BinarySearch(group.GetGroupId());
                    if (binarySearchIndex < 0)
                    {
                        sortedGroupIds.Insert(-1 - binarySearchIndex, group.GetGroupId());
                    }
                }
            }
            singleton._OnNewSetRequest(sortedGroupIds, _IntListToString(sortedGroupIds));
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
    }
}
