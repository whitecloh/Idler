using Plinko.Scripts.Data.Common;

namespace Plinko.Scripts.ECS.Events
{
    public struct LevelLoadedEvent
    {
        public int LevelIndex;
        public Enums.LevelType LevelType;
    }
}