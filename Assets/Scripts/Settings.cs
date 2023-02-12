using UnityEngine;

[CreateAssetMenu(fileName = "AppSettings")]
public class Settings : ScriptableObject
{
    public int TargetWidth = 512;
    public int TargetHeight = 512;
                               
    public KeyCode Key_NextImage = KeyCode.RightArrow;
    public KeyCode Key_PreviousImage = KeyCode.LeftArrow;
    public KeyCode Key_SlowScrolling = KeyCode.LeftShift;
    public KeyCode Key_ExceedConstrains = KeyCode.LeftAlt;
    public KeyCode Key_RescaleImage = KeyCode.LeftControl;
    public KeyCode Key_SetSelection = KeyCode.Space;
}
