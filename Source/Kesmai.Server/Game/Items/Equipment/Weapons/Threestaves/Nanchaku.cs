using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public partial class Nanchaku : Threestaff, ITreasure
{
    /// <inheritdoc />
    public override int LabelNumber => 6000037;

    /// <inheritdoc />
    public override uint BasePrice => 1;

    /// <inheritdoc />
    public override int Weight => 1800;

    /// <inheritdoc />
    public override int MinimumDamage => _weaponStats.MinimumDamage;

    /// <inheritdoc />
    public override int MaximumDamage => _weaponStats.MaximumDamage;

    /// <inheritdoc />
    public override int BaseArmorBonus => _weaponStats.BaseArmorBonus;

    /// <inheritdoc />
    public override int BaseAttackBonus => _weaponStats.BaseAttackBonus;
        
    /// <inheritdoc />
    public override bool CanDisintegrate => false;
        
    /// <inheritdoc />
    public override bool CanBind => true;

    /// <inheritdoc />
    public override WeaponFlags Flags => WeaponFlags.Bashing | WeaponFlags.BlueGlowing;

    private int _weaponLevel;
    private NanchakuWeapon _weaponStats;
    /// <summary>
    /// Initializes a new instance of the <see cref="Nanchaku"/> class.
    /// </summary>
    public Nanchaku(int weaponLevel) : base(944)
    {
        _weaponLevel = weaponLevel;
        _weaponStats = GetWeaponStats(weaponLevel);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Nanchaku"/> class.
    /// </summary>
    public Nanchaku(Serial serial) : base(serial)
    {
            
    }

    /// <inheritdoc />
    public override void GetDescription(List<LocalizationEntry> entries)
    {
        entries.Add(new LocalizationEntry(6200000, 6200143)); /* [You are looking at] [a three-sectioned staff made from a strange black wood.  There is a curious monogram at one end.] */
    }

    /// <inheritdoc />
    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);

        writer.Write((short)1); /* version */

        writer.Write((int)_weaponLevel); /* weapon level */
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
                _weaponLevel = reader.ReadInt32();
                _weaponStats = GetWeaponStats(_weaponLevel);
                break;
            }
        }
    }

    public override TimeSpan GetSwingDelay(MobileEntity entity)
    {
        return entity.GetRoundDelay(_weaponStats.WeaponSpeed);
    }

    public NanchakuWeapon GetWeaponStats()
    {
        NanchakuWeapon nanchaku = new NanchakuWeapon();

        switch (_weaponLevel)
        {
            case 1:
                nanchaku = SetWeaponStats(1, 6, 1, 1, 1.00);
                break;
            case 2:
                nanchaku = SetWeaponStats(2, 7, 1, 1, 1.00);
                break;
            case 3:
                nanchaku = SetWeaponStats(3, 7, 1, 2, 1.00);
                break;
            case 4:
                nanchaku = SetWeaponStats(4, 8, 2, 2, 1.00);
                break;
            case 5:
                nanchaku = SetWeaponStats(4, 8, 2, 3, 1.00);
                break;
            case 6:
                nanchaku = SetWeaponStats(5, 9, 3, 3, 1.00);
                break;
            case 7:
                nanchaku = SetWeaponStats(5, 9, 3, 4, 1.00);
                break;
            default:
                nanchaku = SetWeaponStats(6, 9, 1, 0, 1.00);
                break;
        }

        return nanchaku;
    }

    public NanchakuWeapon SetWeaponStats(int minimumDamage, int maximumDamage, int baseAttackBonus, int baseDefenseBonus, double weaponSpeed)
    {
        return new NanchakuWeapon()
        {
            MinimumDamage = minimumDamage,
            MaximumDamage = maximumDamage,
            BaseAttackBonus = baseAttackBonus,
            BaseArmorBonus = baseDefenseBonus,
            WeaponSpeed = weaponSpeed
        };
    }

    public class NanchakuWeapon
    {
        public int MinimumDamage { get; set; }
        public int MaximumDamage { get; set; }
        public int BaseAttackBonus { get; set; }
        public int BaseArmorBonus { get; set; }
        public double WeaponSpeed { get; set; }
    }
}