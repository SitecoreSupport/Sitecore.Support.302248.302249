
using Sitecore.Abstractions;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Jobs;
using Sitecore.Shell.Applications.Dialogs.ProgressBoxes;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using CSTexts = Sitecore.ContentSearch.Localization.Texts;

namespace Sitecore.Support.ContentSearch.Client.Commands
{

  [Serializable]
  public class RefreshTree : Sitecore.ContentSearch.Client.Commands.RefreshTree
  {
    public override void Execute([NotNull] CommandContext context)
    {
      Assert.ArgumentNotNull(context, "context");

      var item = context.Items[0];
      Assert.IsNotNull(item, "context item cannot be null");

      Context.ClientPage.Start(this, "RunFixed", new NameValueCollection { { "itemUri", item.Uri.ToString() }, { "itemPath", item.Paths.ContentPath } });
    }

    protected void RunFixed(ClientPipelineArgs args)
    {
      string itemPath = args.Parameters["itemPath"];
      if (string.IsNullOrEmpty(itemPath))
      {
        return;
      }

      var jobName = string.Format("{0} ({1})", this.Translate.Text(CSTexts.ReIndexTree), itemPath);
      var headerText = this.Translate.Text(CSTexts.ReIndexTreeHeader);
      ProgressBox.ExecuteSync(jobName, headerText, "Applications/16x16/replace2.png", this.Refresh, this.RefreshDone);
    }

    private void RefreshDone(ClientPipelineArgs args)
    {
      var message = args.Parameters["failed"] == "1" ?
                   CSTexts.ReindexTreeFailed :
                   CSTexts.ReIndexTreeComplete;

      var translatedMessage = this.Translate.Text(message);

      SheerResponse.Alert(translatedMessage);
    }

    private void Refresh(ClientPipelineArgs args)
    {
      this.JobHandle = Context.Job.Handle;

      Item item = GetItemByUri(args.Parameters["itemUri"]);
      if (item == null)
      {
        return;
      }

      List<Job> jobs = null;

      try
      {
        Event.Subscribe("indexing:updateditem", ShowProgress);
        jobs = RunRefreshTree(item);
        WaitTillDone(jobs);
      }
      finally
      {
        Event.Unsubscribe("indexing:updateitem", ShowProgress);
        CheckAndMarkJobAsFailed(args, jobs);
      }
    }

    private static void WaitTillDone(IEnumerable<Job> jobs)
    {
      while (jobs.Any(j => !j.IsDone))
      {
        Thread.Sleep(500);
      }
    }

    private List<Job> RunRefreshTree(Item item)
    {
      Log.Audit($"Refresh indexes for item: {AuditFormatter.FormatItem(item)}", this);

      var jobs = IndexCustodian.RefreshTree((SitecoreIndexableItem)item).ToList();
      return jobs;
    }

    private void CheckAndMarkJobAsFailed(ClientPipelineArgs args, IEnumerable<Job> jobs)
    {
      if (jobs != null && jobs.Any(j => j.Status.Failed))
      {
        args.Parameters["failed"] = "1";
      }
    }

    private void ShowProgress(object sender, EventArgs eventArgs)
    {
      object[] parameters = this.Event.ExtractParameters(eventArgs);

      if (parameters == null || parameters.Length < 3)
      {
        return;
      }

      var itemPath = parameters[2] as string;

      if (!string.IsNullOrEmpty(itemPath) && this.JobHandle != null)
      {
        var job = JobManager.GetJob(this.JobHandle);
        if (job != null)
        {
          job.Status.AddMessage(itemPath);
        }
      }
    }

    private static Item GetItemByUri(string uri)
    {
      var itemUri = ItemUri.Parse(uri);
      Database db = ContentSearchManager.Locator.GetInstance<IFactory>().GetDatabase(itemUri.DatabaseName);
      Item item = db.GetItem(itemUri.ToDataUri());
      return item;
    }
  }
}