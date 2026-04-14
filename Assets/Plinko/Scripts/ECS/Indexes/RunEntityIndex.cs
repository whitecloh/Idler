namespace Plinko.Scripts.ECS.Indexes
{
    public sealed class RunEntityIndex
    {
        private int _runEntity = -1;

        public void SetRunEntity(int entity)
        {
            _runEntity = entity;
        }

        public bool TryGetRunEntity(out int entity)
        {
            entity = _runEntity;
            return entity >= 0;
        }
    }
}