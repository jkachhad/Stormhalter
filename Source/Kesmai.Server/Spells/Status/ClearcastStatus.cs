using System.Drawing;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class ClearcastStatus : SpellStatus
{
	public override int SpellRemovedSound => 223;

	private Timer _internalTimer;

	public ClearcastStatus(MobileEntity entity) : base(entity)
	{
		Inscription = new SpellInscription()
		{
			SpellId = 112,
		};
	}
	
	public override void OnAcquire()
	{
		Refresh();

		if (_entity.Client != null)
		{
			_entity.PlaySound(10004);
			_entity.SendCombatMessage(Color.Magenta, 6300433); /* You gain Clearcast. */
		}
	}
		
	public override void OnRemoved()
	{
		if (_internalTimer != null && _internalTimer.Running)
			_internalTimer.Stop();

		_internalTimer = null;
		
		if (_entity.Client != null)
			_entity.SendCombatMessage(Color.Magenta, 6300434); /* You lost Clearcast. */
			
		base.OnRemoved();
	}
		
	public void Refresh()
	{
		if (_internalTimer != null && _internalTimer.Running)
			_internalTimer.Stop();
			
		_internalTimer = Timer.DelayCall(_entity.Facet.TimeSpan.FromRounds(10.0), OnTick);
	}
		
	private void OnTick()
	{
		_entity.RemoveStatus(this);
	}
}