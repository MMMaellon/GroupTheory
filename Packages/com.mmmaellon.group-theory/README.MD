# Group Theory

Library for syncing groups. Requires a bit of coding to be useful.

Groups can hold multiple items and each item can be part of many groups. Which item is part of which group is automatically synced. Useful for complicated game worlds.

# Ask questions on my [Discord Server](https://discord.gg/S5sDC4PnFp)

## Usage:
1. Add the "Item" script to objects that will become members of groups.
2. Add a "Group" script to your scene somewhere and define what happens when an item joins the group or is removed from it.
3. Perform a bunch of group operations:
    - `item.AddToGroup(group)` to add an item to a group. Nothing happens if item was already in the group.
    - `item.RemoveFromGroup(group)` to remove an item from a group. Nothing happens if item was not in group.
    - `group.HasItem(item)` to check if an item is part of a group
    - `group.GetItems()` to get a DataList of all the items in that group
    - `item.GetGroups()` to get a DataList of all the groups the item is a member of

## List Operations (WIP):
These operations require all the input lists to be sorted and only have unique members. The lists you get from number 3 above are properly formatted.
    - `group.singleton.UnionGroupList(groups1, groups2)` to combine two lists of groups. A similar function exists for item lists.
    - `group.singleton.IntersectionGroupList(groups1, groups2)` to find the overlap between two lists of groups. A similar function exists for item lists.
    - `group.singleton.SubtractGroupList(groups1, groups2)` to subtract the second from the first list of groups. A similar function exists for item lists

Download all my VCC packages here: <https://mmmaellon.github.io/MMMaellonVCCListing/>
