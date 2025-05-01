using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class VigorSpell : InstantSpell
{
	private static SpellInfo _info = new SpellInfo(97, "Vigor", typeof(VigorSpell), 8);
		
	public override string Name => _info.Name;

	protected override bool CheckSequence()
	{
		if (_caster.Stamina <= 0)
			return false;

		return base.CheckSequence();
	}

	protected override void OnCast()
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;

		if (CheckSequence())
		{
			base.OnCast();
			
			if (_caster.GetStatus<VigorStatus>() is not VigorStatus vigorStatus)
			{
				_caster.AddStatus(vigorStatus = new VigorStatus(_caster)
				{
					Inscription = new SpellInscription() { SpellId = _info.SpellId },
				});
			}

			vigorStatus.Consume();
			vigorStatus.Activate();

			if (_caster is PlayerEntity player && _item == null)
				player.AwardMagicSkill(this);
		}
		else
		{
			Fizzle();
		}
			
		FinishSequence();
	}
}