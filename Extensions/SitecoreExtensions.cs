using Sitecore.Data.Items;

namespace Sitecore.EnhancedPublishViewer.Extensions
{
	public static class SitecoreExtensions
	{
		/// <summary>
		/// Gets the count of child items below a given item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="nLevelChild">if set to <c>true</c> [n level child].</param>
		/// <returns></returns>
		public static int GetChildCount(this Item item, bool nLevelChild)
		{
			int childCount = 0;
			if (item != null)
			{
				if (item.HasChildren && nLevelChild)
				{
					childCount = item.Children.Count;

					foreach (Item child in item.Children)
					{
						childCount += child.GetChildCount(nLevelChild);
					}
				}
			}
			return childCount;
		}
	}
}
