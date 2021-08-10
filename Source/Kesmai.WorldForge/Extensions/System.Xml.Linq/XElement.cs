namespace System.Xml.Linq
{
	public static class XElementExtensions
	{
		public static bool TryGetElement(this XElement source, XName name, out XElement element)
			=> ((element = source.Element(name)) != null);

		public static double Element(this XElement source, XName name, double defaultValue)
		{
			if (source.TryGetElement(name, out var element))
				return (double)element;

			return defaultValue;
		}
	}
}