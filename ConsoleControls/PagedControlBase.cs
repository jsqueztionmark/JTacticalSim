using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleControls
{
	public abstract class PagedControlBase<T> : ConsoleBox
	{
		public event EventHandler ItemSelected;
		public event EventHandler SelectionChanged;

		protected KeyValuePair<int, ListBoxItem<T>> _highlightedItem { get; set; }
		public ListBoxItem<T> HighlightedItem 
		{ 
			get { return _highlightedItem.Value; }  
		}

		public ListBoxItem<T> SelectedItem { get; protected set; }

		public int PageSize { get; set; }

		protected List<ItemPage<T>> _pages { get; set; } 
		protected ItemPage<T> _currentPage  { get; set; }

		public int ItemCount { get { return _pages.Sum(p => p.ItemCount); }}
		public int PageCount { get { return _pages.Count; }}

		protected PagedControlBase(BoxDisplayType displayType, PromptType promptType)
			: base(displayType, promptType)
		{
			_pages = new List<ItemPage<T>>{new ItemPage<T>(1)};
			_currentPage = _pages.SingleOrDefault(p => p.PageNum == 1);
			PostRender += On_PostRender;
		}

		public virtual void AddItem(ListBoxItem<T> item)
		{
			if (ItemExistsInList(item))
				return;

			var nextPageNum = PageCount + 1;
			var fillPage = _pages.SingleOrDefault(p => p.PageNum == _pages.Count);

			if (fillPage == null || fillPage.ItemCount >= PageSize) // restrict select lists to 1 - 9
			{
				var newPage = new ItemPage<T>(nextPageNum);
				newPage.AddItem(item);
				_pages.Add(newPage);
			}
			else
			{
				fillPage.AddItem(item);
			}
		}

		protected virtual bool ItemExistsInList(ListBoxItem<T> item)
		{
			return _pages.Any(p => p.SelectionItems.Any(i => i.Value.Equals(item)));

		}

		protected virtual void CyclePages(CycleDirection direction)
		{
			if (direction == CycleDirection.UP)
			{
				_currentPage = (_currentPage.PageNum == PageCount)
					               ? _currentPage
					               : _pages.SingleOrDefault(p => p.PageNum == _currentPage.PageNum + 1);

				// highlight the first item on the list page
				_highlightedItem = _currentPage.FirstItem;
			}
			if (direction == CycleDirection.DOWN)
			{
				_currentPage = (_currentPage.PageNum == 1)
					               ? _currentPage
					               : _pages.SingleOrDefault(p => p.PageNum == _currentPage.PageNum - 1);

				// highlight the last item on the list page
				if (_currentPage != null)
					_highlightedItem = _currentPage.LastItem;
			}
		}

		protected virtual void DrawPaging()
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.BackgroundColor = BorderBackColor;

			if (_currentPage.PageNum < PageCount)
			{
				Console.SetCursorPosition((LeftOrigin + 1 + Width), TopOrigin + Height);
				Console.Write("▼");
			}
			if (_currentPage.PageNum  > 1)
			{
				Console.SetCursorPosition((LeftOrigin + 1 + Width), TopOrigin + 1);
				Console.Write("▲");
			}

			Console.ResetColor();			
		}

		public virtual void SetHighlightedItem(ListBoxItem<T> item)
		{
			//1. Find the page in the select list with the item we want to select
			var itemPage = _pages.SingleOrDefault(p => p.SelectionItems
			                                            .Select(i => i.Value.Equals(item))
			                                            .SingleOrDefault());

			if (itemPage == null)
				return;

			// Set the page to the page the item is in
			_currentPage = itemPage;

			// highlight the now selected item
			_highlightedItem = _currentPage.SelectionItems.SingleOrDefault(kvp => kvp.Value.Equals(item));

			On_SelectionChanged();
		}

		public virtual void ClearSelection()
		{
			_highlightedItem = new KeyValuePair<int,ListBoxItem<T>>();
			SelectedItem = null;
		}

		public virtual void ClearItems()
		{
			_pages = new List<ItemPage<T>>{new ItemPage<T>(1)};
			_currentPage = _pages.SingleOrDefault(p => p.PageNum == 1);
		}


		protected virtual void On_ItemSelected()
		{
			if (ItemSelected != null) ItemSelected(this, new EventArgs());
		}

		protected virtual void On_SelectionChanged()
		{
			if (SelectionChanged != null) SelectionChanged(this, new EventArgs());
		}

		public virtual void On_PostRender(object sender, EventArgs e)
		{
			// Need to draw the paging widgets after the base box renders so that it's not overwritten
			DrawPaging();
		}
	}
}
