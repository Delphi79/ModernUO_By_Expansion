﻿namespace Server.Regions;

// TODO: Implement damage boost regions
public class AcidRiverRegion : MondainRegion
{
    public AcidRiverRegion(string name, Map map, Region parent, params Rectangle3D[] area): base(name, map, parent, area)
    {
    }

    public AcidRiverRegion(string name, Map map, Region parent, int priority, params Rectangle3D[] area)
        : base(name, map, parent, priority, area)
    {
    }
}
