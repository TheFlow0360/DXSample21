using System;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Grid;

namespace DXSample21
{
    public interface ITreeListViewService
    {
        void RefreshParentNode(Object dataItem);

        void RefreshNode(Object dataItem);
    }

    public class TreeListViewService : ServiceBase, ITreeListViewService
    {
        protected TreeListView View => AssociatedObject as TreeListView;

        public void RefreshParentNode(Object dataItem)
        {
            var node = this.View.GetNodeByContent(dataItem);
            if (node?.ParentNode != null)
            {
                this.View.ReloadChildNodes(node.ParentNode.RowHandle);
            }
        }

        public void RefreshNode(Object dataItem)
        {
            var node = this.View.GetNodeByContent(dataItem);
            if (node != null)
            {
                this.View.ReloadChildNodes(node.RowHandle);
            }
        }
    }
}