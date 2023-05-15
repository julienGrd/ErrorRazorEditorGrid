using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Is.Geckos.Core.Extensions;
using ErrorRazorEditorGrid.Grid.Models;

namespace ErrorRazorEditorGrid.Grid
{
    /// <summary>
    /// todo migration ajouter la constraint lorsque ce sera possible de faire ca
    /// https://github.com/aspnet/AspNetCore/issues/8433
    /// </summary>
    /// <typeparam name="TableItem"></typeparam>
    public abstract class BaseGridComponent<TableItem> : CustomBaseComponent //where TableItem : INotifyPropertyChanged
    {

        [Parameter]
        public RenderFragment Columns { get; set; }

        [Parameter]
        public RenderFragment AdditionalHeader { get; set; }

        [Parameter]
        public Action<TableItem> OnDblClick { get; set; }

        [Parameter]
        public Action<TableItem> OnEnter { get; set; }

        [Parameter]
        public Action<TableItem> OnCancel { get; set; }

        [Parameter]
        public Action<TableItem, bool> OnSelect { get; set; }


        //[Parameter]
        //public Action<TableItem> OnRightClick { get; set; }

        [Parameter]
        public bool CanSelectOnRightClick { get; set; }

        [Parameter]
        public string AssociatedContextMenuId { get; set; }

        [Parameter]
        public bool CanOpenContextMenuOnEmpty { get; set; }

        [Parameter]
        public RenderFragment<TableItem> GridSubRow { get; set; }

        [Parameter]
        public Func<TableItem, string> GetElementId { get; set; }

        [Parameter]
        public Func<TableItem, string> RowTitle { get; set; }

        [Parameter]
        public Func<TableItem, string> RowClass { get; set; }

        [Parameter]
        public Func<TableItem, bool> CanSelectRow { get; set; }

        [Parameter]
        public bool HideHeader { get; set; }

        [Parameter]
        public bool DisableSelection { get; set; }

        [Parameter]
        public string TableClass { get; set; }

        [Parameter]
        public string StyleExpander { get; set; }
        [Parameter]
        public string StyleContentGroup { get; set; }

        [Parameter]
        public string ComponentClass { get; set; }

        [Parameter]
        public bool Disabled { get; set; }

        [Parameter]
        public bool ShowSubRow { get; set; } = true;

        [Parameter]
        public bool CanScroll { get; set; } = true;

        [Parameter]
        public bool ListenPropertyChanged { get; set; }

        [Parameter]
        public Func<TableItem, bool> IsGridSubRowHidden { get; set; }

        [Parameter]
        public Action<TableItem> OnDragStart { get; set; }

        [Parameter]
        public bool CanDrag { get; set; }

        [Parameter]
        public bool IsTransparent { get; set; }

        [Parameter]
        public bool HaveRowSeparator { get; set; }

        [Parameter]
        public string Id { get; set; }


        [Parameter]
        public int? VirtualizeIfRowCountSuperiorAt { get; set; }

        [Parameter]
        public bool SingleLine { get; set; }

        /// <summary>
        /// Indique si les rows peuvent avoir des sous-rows
        /// </summary>
        [Parameter]
        public bool CanRowExpand { get; set; }

        /// <summary>
        /// fonction de récuperation des sous rows
        /// </summary>
        [Parameter]
        public Func<TableItem, IEnumerable<TableItem>> GetSubRows { get; set; }


        [Parameter]
        public bool KeyDownPreventDefault { get; set; } = true;

        protected ElementReference contentElement { get; set; }




        protected string ContainerId => $"{Id}Container";

        public abstract TableItem CurrentItem { get; set; }


        protected abstract IList<TableItem> ReadOnlyCollection { get; }

        /// <summary>
        /// c'est l'ensemble des lignes total, elles ne doivent etre regenere que lorsque la source change
        /// </summary>
        protected IList<BaseRowModel<TableItem>> TotalRows { get; set; } = Enumerable.Empty<BaseRowModel<TableItem>>().ToList();

        /// <summary>
        /// c'est l'ensemble des lignes potentiellement affichable, c'est a dire toute pour une grille
        /// et en prenant en compte les groupes fermés/ouvert pour la grouping
        /// </summary>
        protected IList<BaseRowModel<TableItem>> TotalRowsShowable { get; set; } = Enumerable.Empty<BaseRowModel<TableItem>>().ToList();

        /// <summary>
        /// finalement les lignes que l'on affiche
        /// </summary>
        protected IList<BaseRowModel<TableItem>> RowsToShow { get; set; } = Enumerable.Empty<BaseRowModel<TableItem>>().ToList();

        //protected IEnumerable<BaseRowModel<TableItem>> TotalConcreteRowsToShow => RowsToShow.Where(r => r is RowModel<TableItem>).Cast<RowModel<TableItem>>();

        protected virtual void ComputeTotalRows(bool tryToKeepGroup, bool initialize)
        {
            //par défaut c'est la collection, le grouped va surcharger cette méthode pour y mettre des rows "groupes"
            TotalRows.Clear();
            if (ReadOnlyCollection != null)
            {
                this.GetOrderedItems(ReadOnlyCollection).ToList().ForEach(c => TotalRows.Add(new RowModel<TableItem>()
                {
                    Element = c
                }));
            }

        }

        public void HandleDblClick(TableItem element)
        {
            this.CurrentItem = element;
            this.OnDblClick?.Invoke(element);
        }
        public virtual void HandleSelect(TableItem element, bool rightClick, MouseEventArgs args)
        {
            if (args != null && args.CtrlKey)
            {
                if (this.CurrentItem?.Equals(element) ?? false)
                {
                    //seul cas de déselection, sinon traitement normal
                    this.CurrentItem = default;
                    return;
                }
            }
            this.CurrentItem = element;
            this.OnSelect?.Invoke(element, rightClick);
        }


        protected async Task ScrollToCurrentItem(bool down)
        {
            await this.InvokeJsAsync<object>("geckos.gridManager.synchronizeScroll", ContainerId, down);
        }

        protected async Task ScrollTo(double pHeight)
        {
            await this.InvokeJsAsync<object>("geckos.gridManager.scrollTo", ContainerId, pHeight);
        }

        public virtual bool IsSelected(TableItem item)
        {
            return this.CurrentItem?.Equals(item) ?? false;
        }

        protected override bool ShouldRender()
        {
            return base.ShouldRender();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            //sera rempli dans le onAfterRender des columns
            //InternalColumns.Clear();
            //Console.WriteLine("grid afterRender");
            var objectRef = DotNetObjectReference.Create(this);
            await this.InvokeJsAsync("geckos.gridManager.init", objectRef, this.Id);

            if (firstRender)
            {
                this.Compute();

                if (this.CurrentItem != null)
                {
                    await this.ScrollToCurrentItem(false);

                }
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        protected override void OnParametersSet()
        {
            if (string.IsNullOrEmpty(Id))
            {
                Id = Guid.NewGuid().ToString();
            }
            this.ComputeTotalRows(true, true);
            this.Compute();


            base.OnParametersSet();
        }

        public List<GeckosGridColumn<TableItem>> InternalColumns = new List<GeckosGridColumn<TableItem>>();
        public void AddColumn(GeckosGridColumn<TableItem> column)
        {
            if (!InternalColumns.Contains(column))
            {
                InternalColumns.Add(column);
            }

            //this.LaunchStateHasChanged();
        }

        public void RemoveColumn(GeckosGridColumn<TableItem> column)
        {
            if (InternalColumns.Contains(column))
            {
                InternalColumns.Remove(column);
            }

            //this.LaunchStateHasChanged();
        }

        public void ResetOrdering(int idxExclude)
        {
            foreach (var c in this.InternalColumns.Where(c => c.Index != idxExclude))
            {
                c.ResetDirection();
            }
        }

        public virtual bool HaveExpander()
        {
            return false;
        }

        private CurrentDirectionModel _currentDirection;

        public void Order(string orderExpr, ListSortDirection direction, int index)
        {
            this._currentDirection = new CurrentDirectionModel()
            {
                OrderExpr = orderExpr,
                Direction = direction,
                Index = index
            };
            this.ResetOrdering(this._currentDirection.Index);
            this.RecomputeAll();
        }

        protected void RecomputeAll()
        {
            Console.WriteLine(DateTime.Now.ToString() + " RecomputeAll asked");

            this.ComputeTotalRows(true, false);

            this.Compute();

            this.LaunchStateHasChanged();
            Console.WriteLine(DateTime.Now.ToString() + " RecomputeAll finished");
        }

        public IEnumerable<TableItem> GetOrderedItems(IEnumerable<TableItem> source)
        {
            if (!this._currentDirection?.Equals(default(ValueTuple<string, ListSortDirection, int>)) ?? false)
            {
                return source.AsQueryable().OrderBy(this._currentDirection.OrderExpr + (this._currentDirection.Direction == ListSortDirection.Descending ? " descending" : string.Empty));
            }
            else
            {
                return source;
            }
        }

        [JSInvokable]
        public async Task SelectNextResult()
        {
            var rowItem = SelectTo(1);
            if (rowItem != null)
            {
                await ScrollToCurrentItem(true);

            }


        }

        [JSInvokable]
        public async Task SelectPrevResult()
        {
            var rowItem = SelectTo(-1);
            if (rowItem != null)
            {
                await ScrollToCurrentItem(false);
            }


        }


        protected void Compute()
        {
            this.RowsToShow = TotalRowsShowable = TotalRows.Where(r => r.CanShow).ToList(); ;
            this.LaunchStateHasChanged();
        }


        private BaseRowModel<TableItem> SelectTo(int step)
        {

            Dictionary<TableItem, BaseRowModel<TableItem>> lDico = new Dictionary<TableItem, BaseRowModel<TableItem>>();
            RowsToShow
                .ToList()
                .ForEach(t =>
                {
                    var lRowModel = t as RowModel<TableItem>;
                    //var lRowGroupModel = t as RowGroupModel<TableItem>;
                    if (lRowModel != null)
                    {
                        lDico.Add(lRowModel.Element, lRowModel);
                    }
                    //else if (lRowGroupModel != null && lRowGroupModel.ViewGroup.Element != null)
                    //{
                    //    lDico.Add(lRowGroupModel.ViewGroup.Element, lRowGroupModel);
                    //}

                });
            var listConcreteRows = lDico.Keys.ToList();

            var lCurrentPosition = this.CurrentItem == null ? -1 : listConcreteRows.IndexOf(this.CurrentItem);
            int toIdx = 0;
            if (lCurrentPosition > -1)
            {
                toIdx = lCurrentPosition + step;
            }
            var toItem = (toIdx >= 0 && toIdx < listConcreteRows.Count()) ? listConcreteRows.GetItemAt(toIdx) : default;
            if (toItem != null)
            {
                //this.CurrentItem = toItem;
                this.HandleSelect(toItem, false, null);
                return lDico.ContainsKey(toItem) ? lDico[toItem] : default;
            }
            else
            {
                return null;
            }

        }

        //[JSInvokable]
        //public void OnEnter()
        //{
        //    HandleDblClick(this.CurrentItem);
        //}

        protected async Task OnKeyUp(KeyboardEventArgs args)
        {
            var currentKey = System.Windows.Input.KeyHelper.GetKeyFromString(args.Key);


            if (currentKey == System.Windows.Input.Key.Up)
            {
                await SelectPrevResult();
            }
            else if (currentKey == System.Windows.Input.Key.Down)
            {
                await SelectNextResult();
            }
            else if (currentKey == System.Windows.Input.Key.Enter)
            {
                this.OnEnter?.Invoke(this.CurrentItem);
                HandleDblClick(this.CurrentItem);
            }
            else if (currentKey == System.Windows.Input.Key.Escape)
            {
                this.OnCancel?.Invoke(this.CurrentItem);
            }
        }

        protected void OnContextMenuEmpty(MouseEventArgs args)
        {
            if (this.CanOpenContextMenuOnEmpty)
            {
                //BlazorContextMenuService.ShowMenu(this.AssociatedContextMenuId, (int)args.ClientX, (int)args.ClientY);
            }
        }

    }
    internal class CurrentDirectionModel
    {
        public string OrderExpr { get; set; }
        public ListSortDirection Direction { get; set; }
        public int Index { get; set; }
    }
}
