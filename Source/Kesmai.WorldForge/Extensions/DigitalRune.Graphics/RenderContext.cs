using Kesmai.WorldForge;
using Kesmai.WorldForge.Editor;

namespace DigitalRune.Graphics;

public static class RenderContextExtensions
{
	public static PresentationTarget GetPresentationTarget(this RenderContext context)
	{
		if (context.Data.TryGetValue("PresentationTarget", out var value) && value is PresentationTarget target)
			return target;

		return default(PresentationTarget);
	}
		
	public static void SetPresentationTarget(this RenderContext context, PresentationTarget target)
	{
		context.Data["PresentationTarget"] = target;
	}
}