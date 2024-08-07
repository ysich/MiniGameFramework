using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FrameWorkEditor.UILayoutTool
{
    public static class UILayoutToolHelper
    {
        /// <summary>
        /// 是否支持解体
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool CanUnGroup(GameObject obj)
        {
            if (obj == null)
                return false;
            return obj.transform != null && obj.transform.childCount > 0 && obj.GetComponent<Canvas>() == null && obj.transform.parent != null && obj.transform.parent.GetComponent<Canvas>() == null;
        }
        
        public static Bounds GetBounds(GameObject rect)
        {
            Vector3 Min = new Vector3(99999, 99999, 99999);
            Vector3 Max = new Vector3(-99999, -99999, -99999);

            RectTransform[] rectTrans = rect.GetComponentsInChildren<RectTransform>();
            Vector3[] corner = new Vector3[4];
            for (int i = 0; i < rectTrans.Length; i++)
            {
                //获取节点的四个角的世界坐标，分别按顺序为左下左上，右上右下
                rectTrans[i].GetWorldCorners(corner);
                if (corner[0].x < Min.x)
                    Min.x = corner[0].x;
                if (corner[0].y < Min.y)
                    Min.y = corner[0].y;
                if (corner[0].z < Min.z)
                    Min.z = corner[0].z;

                if (corner[2].x > Max.x)
                    Max.x = corner[2].x;
                if (corner[2].y > Max.y)
                    Max.y = corner[2].y;
                if (corner[2].z > Max.z)
                    Max.z = corner[2].z;
            }


            Vector3 center = (Min + Max) / 2;
            Vector3 size = new Vector3(Max.x - Min.x, Max.y - Min.y, Max.z - Min.z);
            return new Bounds(center, size);
        }
      
    }
}