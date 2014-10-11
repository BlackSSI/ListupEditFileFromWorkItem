using System;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Collections.Generic;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace ListupEditFileFromWorkItem
{
    class TfsWorkItem
    {
        private readonly TfsTeamProjectCollection _tfs;
        private readonly WorkItemStore _workItemStore;

        public TfsWorkItem(string teamProjectCollectionUri)
        {
            _tfs = new TfsTeamProjectCollection(new Uri(teamProjectCollectionUri));
            _workItemStore = _tfs.GetService<WorkItemStore>();
        }

        public WorkItemCollection GetWorkItemsFromId(string workItemName, string workItemId)
        {
            string queryString = 
                string.Format("SELECT [タイトル] FROM WorkItems WHERE [作業項目の種類]='{0}' AND [ID]={1}", workItemName, workItemId);
            return _workItemStore.Query(queryString);
        }

        public SortedDictionary<string, string> GetChangeFileList(WorkItemCollection wic)
        {
            var vcs = _tfs.GetService<VersionControlServer>();
            var changeSourceFile = new SortedDictionary<string, string>();

            foreach (WorkItem wi in wic)
            {
                Console.WriteLine("作業項目名={0}", wi.Title);
                for (var i = 0; i < wi.Links.Count; ++i)
                {
                    if (!wi.Links[i].ArtifactLinkType.Name.Equals("Fixed in Changeset")) continue;

                    var uri = ((ExternalLink)wi.Links[i]).LinkedArtifactUri;
                    int changeSetId;
                    int.TryParse(uri.Substring(uri.LastIndexOf('/') + 1), out changeSetId);
                    var changeSet = vcs.GetChangeset(changeSetId);
                    foreach (var change in changeSet.Changes)
                    {
                        if (IsCollectChangeType(change.ChangeType) && IsCollectItemType(change.Item.ItemType))
                        {
                            if (!changeSourceFile.ContainsKey(change.Item.ServerItem))
                            {
                                changeSourceFile.Add(change.Item.ServerItem, change.ChangeType.ToString());
                            }
                            else
                            {
                                string value;
                                changeSourceFile.TryGetValue(change.Item.ServerItem, out value);
                                changeSourceFile.Remove(change.Item.ServerItem);
                                changeSourceFile.Add(change.Item.ServerItem, value + ", " + change.ChangeType.ToString());
                            }
                        }
                    }
                }
            }

            return changeSourceFile;
        }

        private bool IsCollectChangeType(ChangeType type)
        {
            if (type.HasFlag(ChangeType.Add) || type.HasFlag(ChangeType.Delete) || type.HasFlag(ChangeType.Edit) 
            || type.HasFlag(ChangeType.Merge) || type.HasFlag(ChangeType.Rename) || type.HasFlag(ChangeType.Rollback))
            {
                return true;
            }

            return false;
        }

        private bool IsCollectItemType(ItemType type)
        {
            return type.HasFlag(ItemType.File);
        }
    }
}
