using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public partial class WoodenThreeStaff : Threestaff, ITreasure
{
    /// <inheritdoc />
    public override int LabelNumber => 6000037;

    /// <inheritdoc />
    public override uint BasePrice => 1;

    /// <inheritdoc />
    public override int Weight => 1800;

    /// <inheritdoc />
    public override int MinimumDamage => 1;

    /// <inheritdoc />
    public override int MaximumDamage => 6;

    /// <inheritdoc />
    public override int BaseArmorBonus => 2;

    /// <inheritdoc />
    public override int BaseAttackBonus => 0;
        
    /// <inheritdoc />
    public override bool CanDisintegrate => false;
        

    /// <inheritdoc />
    public override WeaponFlags Flags => WeaponFlags.Bashing;

    /// <summary>
    /// Initializes a new instance of the <see cref="WoodenThreeStaff"/> class.
    /// </summary>
    public WoodenThreeStaff() : base(115)
    {
           
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WoodenThreeStaff"/> class.
    /// </summary>
    public WoodenThreeStaff(Serial serial) : base(serial)
    {
            
    }

    /// <inheritdoc />
    public override void GetDescription(List<LocalizationEntry> entries)
    {
        entries.Add(new LocalizationEntry(6200000, 6200144)); /* [You are looking at] [a heavy wooden three-sectioned staff.] */
    }

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);

        writer.Write((short)1); /* version */
    }

    /// <inheritdoc />
    public override void Deserialize(BinaryReader reader)
    {
        base.Deserialize(reader);

        var version = reader.ReadInt16();

        switch (version)
        {
            case 1:
            {
                break;
            }
        }
    }
}