using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.EnhancedPublishViewer.Extensions;
using Sitecore.Publishing;
using Sitecore.Shell.Applications.Dialogs.Publish;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;

namespace Sitecore.EnhancedPublishViewer.Forms
{
	/// <summary>
	/// 
	/// </summary>
	public class CustomPublishForm : PublishForm
	{
		protected Literal LitProgress;

		protected override void OnLoad(EventArgs e)
		{
			if (!Context.ClientPage.IsEvent)
			{
				// Add styles
				AddStylesheet(Settings.GetSetting("PublishProgress.JqueryUiBaseCssHref"));
				AddStylesheet(Settings.GetSetting("PublishProgress.JqueryUiThemeCssHref"));
				AddStylesheet(Settings.GetSetting("PublishProgress.CustomCssHref"));
				// add js
				AddScript(Settings.GetSetting("PublishProgress.JquerySrc"));
				AddScript(Settings.GetSetting("PublishProgress.JquerUiSrc"));

				// Add no conflict
				Context.Page.Page.Header.Controls.Add(new LiteralControl("<script>jQuery.noConflict();</script>"));

				AddScript(Settings.GetSetting("PublishProgress.CustomJsSrc"));
			}
			base.OnLoad(e);
		}

		/// <summary>
		/// Adds a stylesheet link to the form.
		/// </summary>
		/// <param name="href">The href.</param>
		private void AddStylesheet(string href)
		{
			if (!string.IsNullOrEmpty(href))
			{
				HtmlGenericControl linkTag = new HtmlGenericControl("link");
				linkTag.Attributes["type"] = "text/css";
				linkTag.Attributes["rel"] = "stylesheet";
				linkTag.Attributes["href"] = href;

				Context.Page.Page.Header.Controls.Add(linkTag);
			}
		}

		/// <summary>
		/// Adds a script tag to the form.
		/// </summary>
		/// <param name="src">The SRC.</param>
		private void AddScript(string src)
		{
			if (!string.IsNullOrEmpty(src))
			{
				HtmlGenericControl scriptTag = new HtmlGenericControl("script");
				scriptTag.Attributes["type"] = "text/javascript";
				scriptTag.Attributes["src"] = src;

				Context.Page.Page.Header.Controls.Add(scriptTag);
			}
		}


		/// <summary>
		/// Adds update to progress bar to base CheckStatus function.
		/// </summary>
		public new void CheckStatus()
		{
			PublishStatus status = PublishManager.GetStatus(Handle.Parse(this.JobHandle));
			
			// if item id is null or empty then we are doing a site publish
			if(string.IsNullOrEmpty(ItemID))
			{
				SheerResponse.Eval("jQuery( \"#progress_container\" ).addClass( \"invisible\" );");
				SheerResponse.Eval("jQuery( \"#spinner_container\" ).removeClass( \"invisible\" );");
			}
			else
			{
				long processed = status.Processed;
				long? total = GetChildCount();

				double percentage = 0;
				if (total.HasValue && total.Value > 0)
				{
					percentage = Math.Round((double)(processed * 100 / total.Value));
				}

				LitProgress.Text = string.Format("{0}%", percentage);
				SheerResponse.Eval(string.Format("jQuery( \"#progressbar\" ).progressbar( \"option\", \"value\", {0} );", percentage));
			}
			
			base.CheckStatus();
		}

		protected long ChildCount
		{
			get
			{
				object obj = Context.ClientPage.ServerProperties["PublishForm.ChildCount"];
				if (obj != null)
				{
					return (long)obj;
				}

				return -1;
			}
			set
			{
				Context.ClientPage.ServerProperties["PublishForm.ChildCount"] = value;
			}
		}

		protected long? GetChildCount()
		{
			if (ChildCount <= 0)
			{
				Item rooItem = Sitecore.Context.ContentDatabase.GetItem(ItemID);
				if (rooItem != null)
				{
					long childCount = rooItem.GetChildCount(this.PublishChildren.Checked) + 1 + 1;
					long languageCount = GetLanguageCount();
					long targetCount = GetPublishingTargetCount();

					ChildCount = (childCount * languageCount * targetCount);
				}
			}

			return ChildCount;
		}

		private static long GetLanguageCount()
		{
			long l = 0;
			foreach (string str in Context.ClientPage.ClientRequest.Form.Keys)
			{
				if ((str != null) && str.StartsWith("la_"))
				{
					l++;
				}
			}
			return l;
		}

		private static long GetPublishingTargetCount()
		{
			long l = 0;
			foreach (string str in Context.ClientPage.ClientRequest.Form.Keys)
			{
				if ((str != null) && str.StartsWith("pb_"))
				{
					string str2 = ShortID.Decode(str.Substring(3));
					Item item = Context.ContentDatabase.Items[str2];
					Assert.IsNotNull(item, typeof(Item), "Publishing target not found.", new object[0]);
					l++;
				}
			}
			return l;
		}


	}
}
