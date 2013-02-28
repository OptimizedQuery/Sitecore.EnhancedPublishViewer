using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.EnhancedPublishViewer.Extensions;
using Sitecore.Web.UI.Sheer;
using System.Collections;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Jobs;
using Sitecore.Publishing;

namespace Sitecore.EnhancedPublishViewer.Forms
{

	/// <summary>
	/// Job Entity
	/// </summary>
	public class JobEntity
	{
		/// <summary>
		/// Gets or sets the handle.
		/// </summary>
		/// <value>The handle.</value>
		public string Handle { get; set; }
		/// <summary>
		/// Gets or sets the message.
		/// </summary>
		/// <value>The message.</value>
		public string Message { get; set; }
		/// <summary>
		/// Gets or sets the name of the job.
		/// </summary>
		/// <value>The name of the job.</value>
		public string JobName { get; set; }
		/// <summary>
		/// Gets or sets the job category.
		/// </summary>
		/// <value>The job category.</value>
		public string JobCategory { get; set; }
		/// <summary>
		/// Gets or sets the name of the item.
		/// </summary>
		/// <value>The name of the item.</value>
		public string ItemName { get; set; }
		/// <summary>
		/// Gets or sets the item ID.
		/// </summary>
		/// <value>The item ID.</value>
		public string ItemID { get; set; }
		/// <summary>
		/// Gets or sets the source database.
		/// </summary>
		/// <value>The source database.</value>
		public string SourceDatabase { get; set; }
		/// <summary>
		/// Gets or sets the target database.
		/// </summary>
		/// <value>The target database.</value>
		public string TargetDatabase { get; set; }
		/// <summary>
		/// Gets or sets the current target database.
		/// </summary>
		/// <value>The current target database.</value>
		public string CurrentTargetDatabase { get; set; }
		/// <summary>
		/// Gets or sets the languages.
		/// </summary>
		/// <value>
		/// The languages.
		/// </value>
		public string Languages { get; set; }
		/// <summary>
		/// Gets or sets the current language.
		/// </summary>
		/// <value>The current language.</value>
		public string CurrentLanguage { get; set; }
		/// <summary>
		/// Gets or sets the job status.
		/// </summary>
		/// <value>The job status.</value>
		public string JobStatus { get; set; }
		/// <summary>
		/// Gets or sets the processed.
		/// </summary>
		/// <value>The processed.</value>
		public string Processed { get; set; }
		/// <summary>
		/// Gets or sets the mode.
		/// </summary>
		/// <value>The mode.</value>
		public string Mode { get; set; }
		/// <summary>
		/// Gets or sets the child count.
		/// </summary>
		/// <value>The child count.</value>
		public string ChildCount { get; set; }
		/// <summary>
		/// Gets or sets the percentage.
		/// </summary>
		/// <value>The percentage.</value>
		public string Percentage { get; set; }
		/// <summary>
		/// Gets or sets the owner.
		/// </summary>
		/// <value>The owner.</value>
		public string Owner { get; set; }
		/// <summary>
		/// Gets or sets the queue time.
		/// </summary>
		/// <value>
		/// The queue time.
		/// </value>
		public DateTime QueueTime { get; set; }
	}

	/// <summary>
	/// Publish Viewer Form
	/// </summary>
	public class ViewerForm : BaseForm
	{
		#region Declaration
		private List<JobEntity> lstJobDetails = new List<JobEntity>();
		private static Hashtable htChildCount = new Hashtable();
		protected Listview ItemList;
		protected int pollInterval = 3000;
		private string SingleItem = "singleitem";

		#endregion

		#region Property
		/// <summary>
		/// Gets a value indicating whether this instance has enable timer.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance has enable timer; otherwise, <c>false</c>.
		/// </value>
		private bool EnableTimer
		{
			get
			{
				Checkbox t = Client.Page.Page.FindControl("chkAutoRefresh") as Checkbox;
				return t.Checked;
			}
		}
		/// <summary>
		/// Gets a value indicating whether [show additional jobs].
		/// </summary>
		/// <value>
		///   <c>true</c> if [show additional jobs]; otherwise, <c>false</c>.
		/// </value>
		private bool ShowAdditionalJobs
		{
			get
			{
				Checkbox t = Client.Page.Page.FindControl("chkShowAdditionalJobs") as Checkbox;
				return t.Checked;
			}
		}
		/// <summary>
		/// Gets or sets the persistent value.
		/// </summary>
		/// <value>
		/// The persistent value.
		/// </value>
		protected Hashtable PersistentValue
		{
			get
			{
				return Context.ClientPage.ServerProperties["PersistentValue"] as Hashtable;
			}
			set
			{
				Context.ClientPage.ServerProperties["PersistentValue"] = value;
			}
		}
		#endregion

		#region Methods and Events
		/// <summary>
		/// Raises the <see cref="E:PreRender"/> event.
		/// </summary>
		/// <param name="args">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected override void OnPreRender(EventArgs args)
		{
			base.OnPreRender(args);
			if (!Context.ClientPage.IsEvent)
			{
				CheckStatus();
			}
		}
		/// <summary>
		/// Checks the status.
		/// </summary>
		protected void CheckStatus()
		{
			ShowJobs();
			if (EnableTimer)
				SheerResponse.Timer("CheckStatus", pollInterval);
		}
		/// <summary>
		/// Removes the job.
		/// </summary>
		//protected void RemoveJob()
		//{
		//    // If a ListViewItem is not selected when this function is run, show an error message
		//    if (ItemList.SelectedItems.Length == 0)
		//    {
		//        Context.ClientPage.ClientResponse.Alert("Please select an item first");
		//        return;
		//    }

		//    ListviewItem selectedItem = ItemList.SelectedItems[0];
		//    string handle = selectedItem.ColumnValues["Handle"] as string;
		//    //Remove selected job.
		//    JobManager.RemoveJob(Handle.Parse(handle));
		//    ShowJobs();
		//}
		/// <summary>
		/// Shows the jobs.
		/// </summary>
		protected void ShowJobs()
		{
			//Getting from viewstate
			if (PersistentValue != null)
				htChildCount = PersistentValue;

			ItemList.Controls.Clear();
			bool isPublishJobExist = false;
			//Getting all jobs running in background.
			Job[] jobs = JobManager.GetJobs();
			for (int i = 0; i < jobs.Length; i++)
			{
				//Getting single job.
				Job job = jobs[i];

				if (job.Status.State != JobState.Finished)
				{
					int childCount = 0;
					JobEntity jobDetail = new JobEntity();
					//Getting handle
					jobDetail.Handle = job.Handle.ToString();
					//Getting Job Name
					jobDetail.JobName = job.Name;
					//Getting Job Catergory
					jobDetail.JobCategory = job.Category;
					jobDetail.QueueTime = job.QueueTime;


					//Sitecore.Publishing.PublishStatus publishingStatus = ((Sitecore.Publishing.PublishStatus)(job.Options.Parameters[1]));
					//Sitecore.Publishing.PublishOptions publishingOptions = ((Sitecore.Publishing.PublishOptions[])(job.Options.Parameters[0]))[0];
					//Special check for Publish jobs.
					if (job.Name.ToLower().Equals("publish"))
					{
						PublishStatus jobPublishStatus = (PublishStatus)job.Options.Parameters[1];
						PublishOptions[] jobPublishOptions = (PublishOptions[])job.Options.Parameters[0];
						
						//Getting Job Mode
						jobDetail.Mode = jobPublishOptions[0].Mode.ToString();

						isPublishJobExist = true;
						//Getting status
						jobDetail.JobStatus = jobPublishStatus.State.ToString();
						//Getting owner
						if (!String.IsNullOrEmpty(job.Options.ContextUser.Name))
							jobDetail.Owner = job.Options.ContextUser.Name;
						//Setting status if initializing then set to Queue.
						if (jobDetail.JobStatus.ToLower().Equals("initializing"))
						{
							jobDetail.JobStatus = JobState.Queued.ToString();
						}

						//Setting current laguage being executed.
						if (jobPublishStatus.CurrentLanguage != null)
							jobDetail.CurrentLanguage = jobPublishStatus.CurrentLanguage.CultureInfo.DisplayName;
						//Setting current target being executed..
						if (jobPublishStatus.CurrentTarget != null)
							jobDetail.CurrentTargetDatabase = jobPublishStatus.CurrentTarget.Name;
						//Number of item processed.
						jobDetail.Processed = jobPublishStatus.Processed.ToString();
						//Gets Root Item Name
						if (jobDetail.Mode.ToLower().Equals(SingleItem))
						{
							jobDetail.ItemName = jobPublishOptions[0].RootItem.Paths.Path;

							//Gets total number of Children of Root item which includes all child in tree.

							if (htChildCount.ContainsKey(job.Handle))
							{
								childCount = System.Convert.ToInt32(htChildCount[job.Handle]);
							}
							else
							{
								childCount = (jobPublishOptions[0].RootItem.GetChildCount(jobPublishOptions[0].Deep) + 1 + 1);
								htChildCount.Add(job.Handle, childCount);
							}

							//Gets total language selected in job.
							int languageCount = jobPublishOptions.Length;
							//Calculating total children count
							jobDetail.ChildCount = (childCount * languageCount).ToString();

							//Calculating percentage of completions
							double percentage = Math.Round((double)(jobPublishStatus.Processed * 100) / (childCount * languageCount));
							percentage = (percentage > 100) ? 100 : percentage;//Special case when home item is get published with Child item.
							jobDetail.Percentage = percentage.ToString() + "%";
						}
						else
						{
							jobDetail.ItemName = "Full Site";
							jobDetail.ChildCount = "NA";
							jobDetail.Percentage = "NA";
						}

						
						//Gets Source database name.
						jobDetail.SourceDatabase = jobPublishOptions[0].SourceDatabase.Name;


						//Array for targets and language.
						ArrayList aryTargets = new ArrayList();
						ArrayList aryLanguage = new ArrayList();

						foreach (PublishOptions publishoptions in jobPublishOptions)
						{
							//Prepares list of targets available
							if (!aryTargets.Contains(publishoptions.TargetDatabase.Name))
								aryTargets.Add(publishoptions.TargetDatabase.Name);

							//Prepares list of languages available
							if (!aryLanguage.Contains(publishoptions.Language.CultureInfo.DisplayName))
								aryLanguage.Add(publishoptions.Language.CultureInfo.DisplayName);

							////Get Publish mode.
							//jobDetail.Mode = publishoptions.Mode.ToString();
							//Getting total messages during process.
							int msgCount = jobPublishStatus.Messages.Count;

							if (msgCount > 0)
							{
								//Showing last message.
								jobDetail.Message = jobPublishStatus.Messages[msgCount - 1];
							}
						}
						//CVS Target Database
						jobDetail.TargetDatabase = ArrayListToString(aryTargets);
						//CVS Language Database.
						jobDetail.Languages = ArrayListToString(aryLanguage);
						//Add jobdetail in list.
						lstJobDetails.Add(jobDetail);
					}
					else
					{
						//Other than publish jobs.
						//Check wheather Show Additional information is checked or not.
						if (ShowAdditionalJobs)
						{
							//Gets status
							jobDetail.JobStatus = job.Status.State.ToString();
							jobDetail.Owner = (job.Options.ContextUser != null) ? job.Options.ContextUser.Name : string.Empty; ;
							//Collects message.
							foreach (string msg in job.Status.Messages)
							{
								jobDetail.Message += msg;
							}
							//Add job details in job list.
							lstJobDetails.Add(jobDetail);
						}
					}
				}
			}
			//If no publishing Jobs exists then clear old data.
			if (!isPublishJobExist)
			{
				htChildCount.Clear();
			}

			PersistentValue = htChildCount;
			//Binding to grid.
			FillItemList();
		}

		/// <summary>
		/// Arrays the list to string.
		/// </summary>
		/// <param name="list">The list.</param>
		/// <returns></returns>
		public static string ArrayListToString(ArrayList list)
		{
			return ArrayListToString(list, ",");
		}
		/// <summary>
		/// Arrays the list to string.
		/// </summary>
		/// <param name="list">The list.</param>
		/// <param name="separator">The separator.</param>
		/// <returns></returns>
		public static string ArrayListToString(ArrayList list, string separator)
		{
			string ret = String.Empty;
			if (list != null)
			{
				ret = String.Join(separator, (string[])list.ToArray(Type.GetType("System.String")));
			}
			return ret;
		}
		/// <summary>
		/// Refreshes list of jobs
		/// </summary>
		protected void Refresh()
		{
			CheckStatus();
		}
		/// <summary>
		/// Starts the timer.
		/// </summary>
		/// <param name="message">The message.</param>
		[HandleMessage("local:StartTimer")]
		protected void StartTimer(Message message)
		{
			//This method work to launch a timer  
			SheerResponse.Timer("CheckStatus", 1);
		}
		/// <summary>
		/// Autoes the refresh.
		/// </summary>
		protected void AutoRefresh()
		{
			Toolbutton t = Client.Page.Page.FindControl("AutoRefreshToolButton") as Toolbutton;
			if (t != null)
			{
				if (t.Header.Equals("Auto Refresh Off"))
				{
					t.Header = "Auto Refresh On";
				}
				else
				{
					t.Header = "Auto Refresh Off";
					CheckStatus();
				}
			}
		}
		// Method that creates and fills a new Listview control with data from our query
		protected void FillItemList()
		{
			//Query to show job in order.
			var datasource = from JobEntity job in lstJobDetails
											 orderby job.QueueTime, job.JobName, job.JobCategory, job.JobStatus
											 select job;


			if (datasource == null)
			{
				ItemList.Controls.Clear();
				Context.ClientPage.ClientResponse.SetOuterHtml("ItemList", ItemList);
				return;
			}

			foreach (JobEntity jd in datasource)
			{

				ListviewItem listItem = new ListviewItem();
				Context.ClientPage.AddControl(ItemList, listItem);

				//Check if full site then show in red color(Alerting)
				if (jd.JobName.ToLower().Equals("publish") && !jd.Mode.ToLower().Equals(SingleItem))
					listItem.Style.Add("background-color", "#FFA07A");

				//As publishing jobs are heavy jobs and if child count is more then 1000 then it requires attention,
				//1000 can be changed as per environment requirement.
				if (jd.JobName.ToLower().Equals("publish") && jd.Mode.ToLower().Equals(SingleItem))
				{
					if (int.Parse(jd.ChildCount) > 1000)
						listItem.Style.Add("background-color", "#68BCFF");
				}

				listItem.ID = Control.GetUniqueID("I");
				// Populate the list item with data.
				listItem.ColumnValues["JobName"] = jd.JobName;
				listItem.ColumnValues["QueueTime"] = String.Format("{0:T}", jd.QueueTime.ToLocalTime());
				listItem.ColumnValues["Handle"] = jd.Handle;
				listItem.ColumnValues["JobStatus"] = jd.JobStatus;
				listItem.ColumnValues["JobCategory"] = jd.JobCategory;
				listItem.ColumnValues["Mode"] = jd.Mode;
				listItem.ColumnValues["Owner"] = jd.Owner;
				listItem.ColumnValues["ItemName"] = jd.ItemName;
				listItem.ColumnValues["SourceDatabase"] = jd.SourceDatabase;
				listItem.ColumnValues["TargetDatabase"] = jd.TargetDatabase;
				listItem.ColumnValues["CurrentTargetDatabase"] = jd.CurrentTargetDatabase;
				listItem.ColumnValues["Processed"] = jd.Processed;
				listItem.ColumnValues["ChildCount"] = jd.ChildCount;
				listItem.ColumnValues["Percentage"] = jd.Percentage;
				listItem.ColumnValues["Languages"] = jd.Languages;
				listItem.ColumnValues["CurrentLanguage"] = jd.CurrentLanguage;
				listItem.ColumnValues["Message"] = jd.Message;
			}

			Context.ClientPage.ClientResponse.SetOuterHtml("ItemList", ItemList);
		}
		#endregion
	}
}
