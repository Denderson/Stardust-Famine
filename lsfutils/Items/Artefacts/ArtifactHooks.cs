using System.Collections.Generic;
using static lsfUtils.Enums.AbstractObjectType;

namespace lsfUtils.Items.Artefacts;

public static class ArtifactHooks
{
    public static List<AbstractPhysicalObject.AbstractObjectType> artifactTypes = [];
    public static void Apply()
    {
        On.AbstractPhysicalObject.UsesAPersistantTracker += AbstractPhysicalObject_UsesAPersistantTracker;
    }

    private static bool AbstractPhysicalObject_UsesAPersistantTracker(On.AbstractPhysicalObject.orig_UsesAPersistantTracker orig, AbstractPhysicalObject abs)
    {
        var value = orig(abs);
        if (artifactTypes.Contains(abs.type)) return true;
        return value;
    }

    public static void RegisterArtifacts()
    {
        List<AbstractPhysicalObject.AbstractObjectType> types = 
            [
            Enums.AbstractObjectType.KarmaMask,
            Enums.AbstractObjectType.BasementKey
            ];
        artifactTypes.AddRange(types);
    }
}
