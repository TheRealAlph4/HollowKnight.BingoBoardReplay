namespace BingoBoardReplay
{
    public class GlobalSettings
    {
        public enum CloneRoomOption
        {
            Always,
            Never,
            Copy
        }
        public int DefaultMainDelaySeconds = 600;
        public int DefaultSecondaryDelaySeconds = 5;
        public CloneRoomOption LockoutWhenCloning = CloneRoomOption.Copy;
        public CloneRoomOption HideWhenCloning = CloneRoomOption.Copy;
    }
}
