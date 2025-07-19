using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class ClearcastStatus : SpellStatus
{
	public override int SpellRemovedSound => 221;
	
	public override bool Hidden => true;
	
	private Timer _internalTimer;

	public ClearcastStatus(MobileEntity entity) : base(entity)
	{
	}
	
	public override void OnAcquire()
	{
		Refresh();
	}
		
	public override void OnRemoved()
	{
		if (_internalTimer != null && _internalTimer.Running)
			_internalTimer.Stop();

		_internalTimer = null;
			
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