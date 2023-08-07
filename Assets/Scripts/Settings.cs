using UnityEngine;

[CreateAssetMenu(fileName = "AppSettings")]
public class Settings : ScriptableObject
{
    public Resolution ActiveResolution = new(512, 512);
    public Resolution[] Resolutions = new Resolution[4];

    public KeyCode Key_NextImage = KeyCode.RightArrow;
    public KeyCode Key_PreviousImage = KeyCode.LeftArrow;
    public KeyCode Key_SlowScrolling = KeyCode.LeftShift;
    public KeyCode Key_ExceedConstrains = KeyCode.LeftAlt;
    public KeyCode Key_RescaleImage = KeyCode.LeftControl;
    public KeyCode Key_SetSelection = KeyCode.Space;
    public KeyCode Key_UndoSelection = KeyCode.Backspace;

    public KeyCode Key_SelectResolution1 = KeyCode.Alpha1;
    public KeyCode Key_SelectResolution2 = KeyCode.Alpha2;
    public KeyCode Key_SelectResolution3 = KeyCode.Alpha3;
    public KeyCode Key_SelectResolution4 = KeyCode.Alpha4;

    public struct Resolution
    {
        public int Width;
        public int Height;

        public Resolution(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
