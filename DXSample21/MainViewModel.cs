using System.Collections.Generic;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;

namespace DXSample21
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            TreeProvider = new TreeProvider();
        }

        public TreeProvider TreeProvider { get; }


        [Command]
        public void Initialized()
        {
            TreeProvider.TreeListViewService = GetService<ITreeListViewService>();
        }

        [AsyncCommand]
        public async Task LoadData()
        {
            var items = new List<Item>();
            items.Add(new Item("Item 1", @"top1\sub1\"));
            items.Add(new Item("Item 2", @"top1\sub1\"));
            items.Add(new Item("Item 3", @"top1\sub2\"));
            items.Add(new Item("Item 4", @"top2\"));
            items.Add(new Item("Item 5", @"top2\sub1\"));
            items.Add(new Item("Item 6", @"top2\sub2\"));
            await TreeProvider.RebuildNodes(items);
        }
    }
}