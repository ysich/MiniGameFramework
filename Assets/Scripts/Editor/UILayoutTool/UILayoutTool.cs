using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FrameWorkEditor.UILayoutTool
{
    public class UILayoutTool
    {
        public static void MakeGroup()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length <= 0)
            {
                EditorUtility.DisplayDialog("Error", "当前没有选中节点", "Ok");
                return;
            }

            //先判断选中的节点是不是挂在同个父节点上的。
            Transform parent = Selection.gameObjects[0].transform.parent;
            //父节点如果是Canvas则取不到
            if (parent == null)
            {
                EditorUtility.DisplayDialog("Error", "父节点为Canvas无法进行打组操作", "Ok");
                return;
            }
            foreach (var item in Selection.gameObjects)
            {
                if (item.transform.parent != parent)
                {
                    EditorUtility.DisplayDialog("Error", "不能跨容器组合", "Ok");
                    return;
                }
            }

            GameObject box = new GameObject("group");
            RectTransform rectTrans = box.AddComponent<RectTransform>();
            Undo.IncrementCurrentGroup();
            int groupIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Make Group");
            Undo.RegisterCreatedObjectUndo(box, "Create Group Go");

            Vector2 leftTopPos = new Vector2(99999, -99999);
            Vector2 rightBottomPos = new Vector2(-99999, 99999);
            foreach (var item in Selection.gameObjects)
            {
                Bounds bound = UILayoutToolHelper.GetBounds(item);
                Vector3 boundMin = item.transform.parent.InverseTransformPoint(bound.min);
                Vector3 boundMax = item.transform.parent.InverseTransformPoint(bound.max);
                if (boundMin.x < leftTopPos.x)
                    leftTopPos.x = boundMin.x;
                if (boundMax.y > leftTopPos.y)
                    leftTopPos.y = boundMax.y;
                if (boundMax.x > rightBottomPos.x)
                    rightBottomPos.x = boundMax.x;
                if (boundMin.y < rightBottomPos.y)
                    rightBottomPos.y = boundMin.y;
            }

            rectTrans.SetParent(parent);
            rectTrans.sizeDelta =
                new Vector2(rightBottomPos.x - leftTopPos.x, leftTopPos.y - rightBottomPos.y);
            leftTopPos.x += rectTrans.sizeDelta.x / 2;
            leftTopPos.y -= rectTrans.sizeDelta.y / 2;
            rectTrans.localPosition = leftTopPos;
            rectTrans.localScale = Vector3.one;

            //需要先生成好Box和设置好它的坐标和大小才可以把选中的节点挂进来，注意要先排好序，不然层次就乱了
            GameObject[] sortedObjs = Selection.gameObjects.OrderBy(x => x.transform.GetSiblingIndex()).ToArray();
            for (int i = 0; i < sortedObjs.Length; i++)
            {
                Undo.SetTransformParent(sortedObjs[i].transform, rectTrans, "move item to group");
            }

            Selection.activeGameObject = box;
            Undo.CollapseUndoOperations(groupIndex);
        }
        public static void UnGroup()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length <= 0)
            {
                EditorUtility.DisplayDialog("Error", "当前没有选中节点", "Ok");
                return;
            }

            if (Selection.gameObjects.Length > 1)
            {
                EditorUtility.DisplayDialog("Error", "只能同时解除一个Group", "Ok");
                return;
            }

            GameObject target = Selection.activeGameObject;
            Transform newParent = target.transform.parent;
            if (target.transform.childCount > 0)
            {
                Transform[] child = target.transform.GetComponentsInChildren<Transform>(true);
                foreach (var item in child)
                {
                    //不是自己的子节点或是自己的话就跳过
                    if (item.transform.parent != target.transform || item.transform == target.transform)
                        continue;
                 
                    Undo.SetTransformParent(item.transform, newParent, "move item to group");
                }

                Undo.DestroyObjectImmediate(target);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "选择对象容器控件", "Ok");
            }
        }
    }
}