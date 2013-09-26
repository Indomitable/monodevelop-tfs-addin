using System;
using GLib;
using System.Collections.Generic;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.Linq;
using Microsoft.TeamFoundation.VersionControl.Common;

namespace MonoDevelop.VersionControl.TFS.Helpers
{
    public class HierarchyItem
    {
        private readonly Item _item;

        public HierarchyItem(Item item)
        {
            Children = new List<HierarchyItem>();
            _item = item;
        }

        public HierarchyItem Parent { get; set; }

        public List<HierarchyItem> Children { get; set; }

        public string ServerPath { get { return _item.ServerItem; } }

        public string Name
        {
            get
            {
                if (string.Equals(_item.ServerItem, VersionControlPath.RootFolder))
                    return VersionControlPath.RootFolder;
                return _item.ServerItem.Substring(_item.ServerItem.LastIndexOf(VersionControlPath.Separator) + 1);
            }
        }
    }

    public static class ItemSetToHierarchItemConverter
    {
        public static HierarchyItem Convert(Item[] items)
        {
            HierarchyItem[] linerHierarchy = items.Select(x => new HierarchyItem(x)).ToArray();
            HierarchyItem root = linerHierarchy[0];
            for (int i = 1; i < linerHierarchy.Length; i++)
            {
                var currentLine = linerHierarchy[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    var previousLine = string.Equals(linerHierarchy[j].ServerPath, VersionControlPath.RootFolder) ?
                                       VersionControlPath.RootFolder :
                                       linerHierarchy[j].ServerPath + VersionControlPath.Separator;
                    if (currentLine.ServerPath.StartsWith(previousLine, StringComparison.Ordinal) &&
                        currentLine.ServerPath.Substring(previousLine.Length).IndexOf(VersionControlPath.Separator) == -1)
                    {
                        currentLine.Parent = linerHierarchy[j];
                        currentLine.Parent.Children.Add(currentLine);
                        break;
                    }
                }
            }
            return root;
        }
    }
}

