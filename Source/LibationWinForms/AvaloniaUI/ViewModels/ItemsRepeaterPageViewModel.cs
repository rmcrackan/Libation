using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Avalonia.Media;
using ReactiveUI;

namespace LibationWinForms.AvaloniaUI.ViewModels
{
    public class ProcessQueueItems : ObservableCollection<ItemsRepeaterPageViewModel.Item>
	{
        public ProcessQueueItems(IEnumerable<ItemsRepeaterPageViewModel.Item> items) :base(items) { }

        public void MoveFirst(ItemsRepeaterPageViewModel.Item item)
		{
            var index = Items.IndexOf(item);
            if (index < 1) return;

            Move(index, 0);
        }
        public void MoveUp(ItemsRepeaterPageViewModel.Item item)
		{
            var index = Items.IndexOf(item);
            if (index < 1) return;

            Move(index, index - 1);
        }
        public void MoveDown(ItemsRepeaterPageViewModel.Item item)
		{
            var index = Items.IndexOf(item);
            if (index < 0 || index > Items.Count - 2) return;

            Move(index, index + 1);
        }

        public void MoveLast(ItemsRepeaterPageViewModel.Item item)
		{
            var index = Items.IndexOf(item);
            if (index < 0 || index > Items.Count - 2) return;

            Move(index, Items.Count - 1);
        }
    }


    public class ItemsRepeaterPageViewModel : ViewModelBase
    {
        private int _newItemIndex = 1;
        private int _newGenerationIndex = 0;
        private ProcessQueueItems _items;

        public ItemsRepeaterPageViewModel()
        {
            _items = CreateItems();
        }

        public ProcessQueueItems Items
        {
            get => _items;
            set => this.RaiseAndSetIfChanged(ref _items, value);
        }

        public Item? SelectedItem { get; set; }

        public void AddItem()
        {
            var index = SelectedItem != null ? Items.IndexOf(SelectedItem) : -1;
            Items.Insert(index + 1, new Item(index + 1, $"New Item {_newItemIndex++}"));
        }

        public void RemoveItem()
        {
            if (SelectedItem is not null)
            {
                Items.Remove(SelectedItem);
                SelectedItem = null;
            }
            else if (Items.Count > 0)
            {
                Items.RemoveAt(Items.Count - 1);
            }
        }

        public void RandomizeHeights()
        {
            var random = new Random();

            foreach (var i in Items)
            {
                i.Height = random.Next(240) + 10;
            }
        }

        public void ResetItems()
        {
            Items = CreateItems();
        }

        private ProcessQueueItems CreateItems()
        {
            var suffix = _newGenerationIndex == 0 ? string.Empty : $"[{_newGenerationIndex.ToString()}]";

            _newGenerationIndex++;

            return new ProcessQueueItems(
                Enumerable.Range(1, 100).Select(i => new Item(i, $"Item {i.ToString()} {suffix}")));
        }

        public class Item : ViewModelBase
        {
            private double _height = double.NaN;
            static Random rnd = new Random();

            public Item(int index, string text)
            {
                Index = index;
                Text = text;
                Narrator = "Narrator " + index;
                Author = "Author " + index;
                Title = "Book " + index + ": This is a book title.\r\nThis is line 2 of the book title";

                Progress = rnd.Next(0, 101);
                ETA = "ETA: 01:14";

                IsDownloading = rnd.Next(0, 2) == 0;

                if (!IsDownloading)
                    IsFinished = rnd.Next(0, 2) == 0;

                if (IsDownloading)
                    Title += "\r\nDOWNLOADING";
                else if (IsFinished)
                    Title += "\r\nFINISHED";
                else
                    Title += "\r\nQUEUED";
            }

            public bool IsFinished { get; }
            public bool IsDownloading { get; }
            public bool Queued => !IsFinished && !IsDownloading;


            public int Index { get; }
            public string Text { get; }
            public string ETA { get; }
            public string Narrator { get; }
            public string Author { get; }
            public string Title { get; }
            public int Progress { get; }

            public double Height
            {
                get => _height;
                set => this.RaiseAndSetIfChanged(ref _height, value);
            }
        }
    }
}
