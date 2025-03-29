using System;
using Unity.Muse.Common;
using UnityEngine;

namespace Unity.Muse.Texture
{
    static class HdriProvider
    {
        internal static Cubemap GetHdri(HdriEnvironment environment)
        {
            return environment switch
            {
                HdriEnvironment.Default => ResourceManager.Load<Cubemap>(PackageResources.defaultHDRCubemap),
                HdriEnvironment.Inside => ResourceManager.Load<Cubemap>(PackageResources.indoorHDRCubemap),
                HdriEnvironment.DayOutside => ResourceManager.Load<Cubemap>(PackageResources.daylightOutdoorHDRCubemap),
                HdriEnvironment.NightOutside => ResourceManager.Load<Cubemap>(PackageResources.nightOutdoorHDRCubemap),
                HdriEnvironment.OutsideNeutral =>  ResourceManager.Load<Cubemap>(PackageResources.outdoorNeutralHDRCubemap),
                _ => throw new ArgumentOutOfRangeException(nameof(environment), environment, null)
            };
        }
    }
}