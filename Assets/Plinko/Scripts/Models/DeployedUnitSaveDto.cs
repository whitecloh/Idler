using System;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class DeployedUnitSaveDto
    {
        public int OwnedUnitRuntimeId;
        public int DeploymentOrder;
    }
}
