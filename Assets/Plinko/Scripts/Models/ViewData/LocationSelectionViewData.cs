using System;
using System.Collections.Generic;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class LocationSelectionViewData
    {
        public List<LocationEntryViewData> Locations = new();
    }
}
