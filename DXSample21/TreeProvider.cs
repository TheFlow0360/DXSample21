using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;

namespace DXSample21
{
    public class TreeProvider : ViewModelBase, IChildNodesSelector
    {
        public TreeProvider()
        {
            Source = new ObservableCollection<BaseTreeItem>();
            ItemNodes = new Collection<ItemTreeItem>();
        }

        public async Task RebuildNodes(IEnumerable<Item> items)
        {
            var nodes = await Task.Run(() =>
            {
                var folders = new List<FolderTreeItem>();
                foreach (var unit in items)
                {
                    var parent = ProcessPath(unit.Path.Trim(Path.DirectorySeparatorChar), folders, null);
                    var itemNode = new ItemTreeItem(unit, parent);
                    ItemNodes.Add(itemNode);
                    parent.SubItems.Add(itemNode);
                }

                return folders;
            });

            Source.Clear();
            foreach (var folderTreeItem in nodes)
            {
                Source.Add(folderTreeItem);
            }
        }

        private FolderTreeItem ProcessPath(String path, List<FolderTreeItem> parentList, FolderTreeItem parent)
        {
            var idx = path.IndexOf(Path.DirectorySeparatorChar);
            var directory = idx >= 0 ? path.Substring(0, idx) : path;

            if (!(parentList.FirstOrDefault(item =>
                    item is FolderTreeItem folder &&
                    folder.Caption.Equals(directory, StringComparison.InvariantCultureIgnoreCase)) is FolderTreeItem
                node))
            {
                node = new FolderTreeItem(directory, parent);
                parentList.Add(node);
            }

            if (idx >= 0)
            {
                return ProcessPath(path.Substring(idx + 1), node.SubFolders, node);
            }

            return node;
        }

        public void AdjustNodePosition(Item item)
        {
            var nodes = ItemNodes.Where(x => x.Item == item);
            var foundCurrent = false;
            foreach (var node in nodes.ToList())
            {
                if (node.Path.Equals(item.Path))
                {
                    foundCurrent = true;
                    node.IsShadow = false;
                    TreeListViewService.RefreshNode(node);
                    continue;
                }

                if (node.Path.Equals(item.OriginalPath))
                {
                    node.IsShadow = true;
                    TreeListViewService.RefreshNode(node);
                    continue;
                }

                if (node.Parent is FolderTreeItem folder)
                {
                    folder.SubItems.Remove(node);
                    TreeListViewService.RefreshNode(folder);
                }
                else
                {
                    Source.Remove(node);
                }

                ItemNodes.Remove(node);
            }

            if (!foundCurrent)
            {
                var rootFolders = Source.Select(x => x is FolderTreeItem folder ? folder : null).Where(x => x != null).ToList();
                var parent = ProcessPath(item.Path.Trim(Path.DirectorySeparatorChar), rootFolders, null);
                foreach (var folder in rootFolders)
                {
                    if (!Source.Contains(folder))
                    {
                        Source.Add(folder);
                    }
                }
                var itemNode = new ItemTreeItem(item, parent);
                ItemNodes.Add(itemNode);
                parent.SubItems.Add(itemNode);
                TreeListViewService.RefreshNode(parent);
            }
        }

        public ObservableCollection<BaseTreeItem> Source { get; }

        public Collection<ItemTreeItem> ItemNodes { get; }

        public ITreeListViewService TreeListViewService { get; set; }

        private BaseTreeItem _selectedItem;

        public BaseTreeItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                //FocusedItemChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public IEnumerable SelectChildren(Object item)
        {
            switch (item)
            {
                case FolderTreeItem folder:
                    return folder.Children;
                case ItemTreeItem _:
                    return null;
                case BaseTreeItem _:
                    Debug.Assert(false, "unhandled node type");
                    return null;
                default:
                    throw new ArgumentException("This ChildNodesSelector is intended exclusively for TreeItems.");
            }
        }

        [Command]
        public void StartDrag(StartRecordDragEventArgs e)
        {
            e.AllowDrag = (e.Records != null) && e.Records.All(record => record is ItemTreeItem item && item.AllowCheck);
        }

        [Command]
        public void DragOver(DragRecordOverEventArgs e)
        {
            e.Effects = e.IsFromOutside ? DragDropEffects.None : DragDropEffects.Move;
        }

        [Command]
        public void Drop(DropRecordEventArgs e)
        {
            var data = (RecordDragDropData)e.Data.GetData(typeof(RecordDragDropData));
            if (data?.Records == null)
            {
                return;
            }

            var targetPath = String.Empty;
            if (e.TargetRecord is ItemTreeItem targetUnit)
            {
                targetPath = targetUnit.Item.Path;
                if (e.DropPosition == DropPosition.Inside)
                {
                    e.DropPosition = DropPosition.After;
                }
            }
            else if (e.TargetRecord is FolderTreeItem targetFolder)
            {
                targetPath = e.DropPosition == DropPosition.Inside ? targetFolder.Path : Path.GetDirectoryName(targetFolder.Path);
            }
            else
            {
                return;
            }

            var items = data.Records.Select(x => x is ItemTreeItem item ? item.Item : null).Where(x => x != null).ToList();
            MoveItems(items, targetPath);
            foreach (var item in items)
            {
                AdjustNodePosition(item);
            }
        }

        private void MoveItems(List<Item> items, String targetPath)
        {
            foreach (var item in items)
            {
                item.Path = targetPath;
            }
        }

        [Command]
        public void CompleteDragDrop(CompleteRecordDragDropEventArgs e)
        {
            // abort the real drag-drop because this doesn't work with ChildNodesSelector
        }
    }
}