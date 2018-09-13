using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Mvvm;

namespace DXSample21
{
    public abstract class BaseTreeItem : BindableBase
    {
        public BaseTreeItem(BaseTreeItem parent = null)
        {
            Checked = false;
            _parent = new WeakReference<BaseTreeItem>(parent);
        }

        public Boolean? Checked
        {
            get => GetProperty(() => Checked);
            set
            {
                SetProperty(() => Checked, AllowCheck ? value : false);
            }
        }

        public virtual Boolean AllowCheck => !IsShadow;

        public Boolean IsShadow { get; set; }

        public abstract String Caption { get; }

        private readonly WeakReference<BaseTreeItem> _parent;
        public BaseTreeItem Parent => _parent.TryGetTarget(out var parent) ? parent : null;

        public String Path { get; protected set; }

        public abstract NodeType Type { get; }

        public abstract Boolean HasChildren { get; }
    }

    public enum NodeType
    {
        Folder,
        Item
    }

    public class FolderTreeItem : BaseTreeItem
    {
        public FolderTreeItem(String caption, FolderTreeItem parent) : base(parent)
        {
            Caption = caption;
            Path = (String.IsNullOrEmpty(parent?.Path) ? caption : System.IO.Path.Combine(parent.Path, caption)) + System.IO.Path.DirectorySeparatorChar;
            SubFolders = new List<FolderTreeItem>();
            SubItems = new List<ItemTreeItem>();
        }

        public override String Caption { get; }

        public override NodeType Type => NodeType.Folder;

        public override Boolean HasChildren => SubFolders.Count > 0 || SubItems.Count > 0;

        public List<FolderTreeItem> SubFolders { get; }

        public List<ItemTreeItem> SubItems { get; }

        public IEnumerable<BaseTreeItem> Children => SubFolders.Cast<BaseTreeItem>().Concat(SubItems);
    }

    public class ItemTreeItem : BaseTreeItem
    {
        public ItemTreeItem(Item item, FolderTreeItem parent) : base(parent)
        {
            Item = item;
            Path = parent?.Path;
            if (!Path.Equals(Item.Path))
            {
                throw new Exception("inconsistent tree generation");
            }
        }

        public Item Item { get; }

        public override String Caption => Item.Caption;

        public override NodeType Type =>NodeType.Item;

        public override Boolean HasChildren => false;
    }
}