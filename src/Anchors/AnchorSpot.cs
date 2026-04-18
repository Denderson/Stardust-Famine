using System;
using UnityEngine;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pom.Pom;

namespace Stardust
{
    public class AnchorData : ManagedData
    {
        public AnchorData(PlacedObject po) : base(po, new ManagedField[] { })
        {

        }
        [StringField("Name", "Anchor Name", "Name: ")]
        public string name;

        public bool Active(ref RainWorldGame game)
        {
            // here should be code for when to trigger it, aka checking for save data stuff
            return false;
        }
    }

    internal class AnchorUAD : UpdatableAndDeletable
    {
        private AnchorData data;

        public AnchorUAD(PlacedObject placedObject, Room room)
        {
            AnchorData maybedata = placedObject.data as AnchorData;
            if (maybedata == null)
            {
                throw new ArgumentException($"{nameof(PlacedObject)} was null or didn't contain a {nameof(AnchorData)} instance");
            }
            data = maybedata;
            this.room = room;

            // here should be the main part of Anchor code, including its graphics and stuff
        }
    }
}