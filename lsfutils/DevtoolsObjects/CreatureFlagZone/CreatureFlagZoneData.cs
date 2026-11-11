using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Pom.Pom;

namespace lsfUtils.DevtoolsObjects.CreatureFlagZone
{
    public class CreatureFlagZoneData : ManagedData
    {
        public CreatureFlagZoneData(PlacedObject po) : base(po, new ManagedField[] { })
        {

        }
        [StringField("CreatureFlagZone", "", "Creature Flags:")]
        public string creatureFlags;

        [Vector2Field("Radius", defX: 80f, defY: 0f, Vector2Field.VectorReprType.circle)]
        public Vector2 radius;

    }
}
