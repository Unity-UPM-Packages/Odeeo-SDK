using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odeeo.Utils
{
    internal static class OdeeoRectHelper
    {
        internal static Rect GetScreenRect(RectTransform rectTransform, Canvas canvas)
        {
            Vector3[] corners = new Vector3[4];
            Vector3[] screenCorners = new Vector3[2];
            rectTransform.GetWorldCorners(corners);
            
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
            {
                screenCorners[0] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[1]);
                screenCorners[1] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[3]);
            }
            else
            {
                screenCorners[0] = RectTransformUtility.WorldToScreenPoint(null, corners[1]);
                screenCorners[1] = RectTransformUtility.WorldToScreenPoint(null, corners[3]);
            }

            int navigationBarHeight = 0;

            Rect rect = new Rect(screenCorners[0], screenCorners[0] - screenCorners[1]);
            rect = new Rect(new Vector2(rect.xMin, rect.yMin - rect.size.y + navigationBarHeight),
                new Vector2(-rect.size.x, rect.size.y));
            return rect;
        }

        internal static Rect LimitRectToScreen(Rect rect)
        {
            if (rect.x < 0)
            {
                rect.width += rect.x;
                rect.x = 0f;
            }

            if (rect.y < 0)
            {
                rect.height += rect.y;
                rect.y = 0f;
            }
            
            float maxWidth = Mathf.Max(0f, Screen.width - rect.x);
            float maxHeight = Mathf.Max(0f, Screen.height - rect.y);
            
            rect.width = Mathf.Clamp(rect.width, 0f, maxWidth);
            rect.height = Mathf.Clamp(rect.height, 0f, maxHeight);
            
            return rect;
        }

        internal static bool IsRectContainsRect(Rect ad, Rect parentRect)
        {
            float equalFactorRemover = .1f; // making sure that points are in different dimensions
            bool topLeft = parentRect.Contains(new Vector2(ad.xMin + equalFactorRemover, ad.yMax - equalFactorRemover));
            bool rightBot =
                parentRect.Contains(new Vector2(ad.xMax - equalFactorRemover, ad.yMin + equalFactorRemover));
            bool topRight =
                parentRect.Contains(new Vector2(ad.xMax - equalFactorRemover, ad.yMax - equalFactorRemover));
            bool leftBot = parentRect.Contains(new Vector2(ad.xMin + equalFactorRemover, ad.yMin + equalFactorRemover));
            return topLeft && rightBot && topRight && leftBot;
        }

        internal static Vector2 ConvertRectToPosition(Rect rect, OdeeoSdk.IconPosition iconPosition, int size)
        {
            Vector2 result = Vector2.zero;
            float halfSize = (float)size / 2f;
            switch (iconPosition)
            {
                case OdeeoSdk.IconPosition.Centered:
                    result.x = rect.center.x - halfSize;
                    result.y = rect.center.y - halfSize;
                    break;
                case OdeeoSdk.IconPosition.BottomLeft:
                    result.x = rect.xMin;
                    result.y = rect.yMin;
                    break;
                case OdeeoSdk.IconPosition.BottomRight:
                    result.x = rect.xMax - size;
                    result.y = rect.yMin;
                    break;
                case OdeeoSdk.IconPosition.TopLeft:
                    result.x = rect.xMin;
                    result.y = rect.yMax - size;
                    break;
                case OdeeoSdk.IconPosition.TopRight:
                    result.x = rect.xMax - size;
                    result.y = rect.yMax - size;
                    break;
                case OdeeoSdk.IconPosition.CenterLeft:
                    result.x = rect.xMin;
                    result.y = rect.center.y - halfSize;
                    break;
                case OdeeoSdk.IconPosition.CenterRight:
                    result.x = rect.xMax - size;
                    result.y = rect.center.y - halfSize;
                    break;
                case OdeeoSdk.IconPosition.BottomCenter:
                    result.x = rect.center.x - halfSize;
                    result.y = rect.yMin;
                    break;
                case OdeeoSdk.IconPosition.TopCenter:
                    result.x = rect.center.x - halfSize;
                    result.y = rect.yMax - size;
                    break;
            }

            return result;
        }

        internal static Vector2 PixelPositionToDp(Vector2 pos)
        {
            return pos / OdeeoDpiResolution.GetDeviceScale();
        }
    }
}