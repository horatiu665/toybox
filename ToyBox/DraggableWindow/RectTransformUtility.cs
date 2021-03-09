namespace ToyBox.DraggableWindowHHH
{
    using UnityEngine;
    using System.Collections;

    public static class RectTransformUtility
    {

        /// <summary>
        /// WARNING: REQUIRES REKT ANCHORS AT (0,0,0,0)
        /// Sets position on rektTransform such that its rect remains inside the screen.
        /// </summary>
        /// <param name="rektTransform"></param>
        /// <param name="canvas"></param>
        public static void KeepInsideScreen(RectTransform rektTransform, Canvas canvas)
        {

            // drag target rect is in CANVAS SPACE. WE MUST CONVERT TO SCREEN SPACE to use Screen.width and Screen.height.
            var rect = rektTransform.rect;
            var rectScreen = new Rect(CanvasSpaceToScreenPosition(rect.position, canvas), CanvasSpaceToScreenPosition(rect.size, canvas));

            var screenPos = CanvasSpaceToScreenPosition(rektTransform.anchoredPosition, canvas);

            var posx = Mathf.Clamp(screenPos.x, rectScreen.width * rektTransform.pivot.x, Screen.width - rectScreen.width * (1 - rektTransform.pivot.x));
            var posy = Mathf.Clamp(screenPos.y, rectScreen.height * rektTransform.pivot.y, Screen.height - rectScreen.height * (1 - rektTransform.pivot.y));

            screenPos.x = posx;
            screenPos.y = posy;

            rektTransform.anchoredPosition = ScreenPositionToCanvasSpace(screenPos, canvas);
        }

        public static Vector2 CanvasSpaceToScreenPosition(Vector2 pos, Canvas canvas)
        {
            var cRekt = (canvas.transform as RectTransform).rect;
            return new Vector2(
                (pos.x) / cRekt.width * Screen.width,
                (pos.y) / cRekt.height * Screen.height
                );
        }

        public static Vector2 ScreenPositionToCanvasSpace(Vector2 pos, Canvas canvas)
        {
            return new Vector2(
                pos.x / Screen.width * (canvas.transform as RectTransform).rect.width,
                pos.y / Screen.height * (canvas.transform as RectTransform).rect.height
                );
        }


        public static bool RektContainsMousePos(Vector3 mousePos, RectTransform rect, Canvas canvas)
        {
            var rektRect = rect.rect;
            var canvasRekt = (canvas.transform as RectTransform).rect;

            var screenSpaceRekt = new Rect(rektRect.position, rektRect.size);
            screenSpaceRekt.x = Mathf.InverseLerp(canvasRekt.x, canvasRekt.x + canvasRekt.width, rektRect.x) * Screen.width;
            screenSpaceRekt.y = Mathf.InverseLerp(canvasRekt.y, canvasRekt.y + canvasRekt.height, rektRect.y) * Screen.height;
            screenSpaceRekt.width = rektRect.width / canvasRekt.width * Screen.width;
            screenSpaceRekt.height = rektRect.height / canvasRekt.height * Screen.height;
            bool screenContainsMouserekt = screenSpaceRekt.Contains(mousePos);
            return screenContainsMouserekt;
        }
    }
}