using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Is.Geckos.Core.Extensions;
using System.ComponentModel;
using Microsoft.AspNetCore.Components.Web;
using Is.Geckos.Blazor.Client.Components.Framework.Grid.Models;

namespace Is.Geckos.Blazor.Client.Components.Framework.Grid
{
    /// <summary>
    /// todo migration ajouter la constraint lorsque ce sera possible de faire ca
    /// https://github.com/aspnet/AspNetCore/issues/8433
    /// </summary>
    /// <typeparam name="TableItem"></typeparam>
    public partial class GeckosGroupingGrid<TableItem>// where TableItem : INotifyPropertyChanged, IDisposable
    {

        [Parameter]
        public PagedCollectionView<TableItem> PagedCollectionView { get; set; }

        [Parameter]
        public Func<CollectionViewGroup<TableItem>, RenderFragment> GetCustomHeader { get; set; }


        [Parameter]
        public bool IsAutoExpand { get; set; }

        [Parameter]
        public Func<CollectionViewGroup<TableItem>, bool> ExpandAtInitialize { get; set; }

        [Parameter]
        public bool CanMultiSelect { get; set; }

        [Parameter]
        public bool ListenCollectionChanged { get; set; }

        [Parameter]
        public bool IsGroupKeyUnique { get; set; }

        [Parameter]
        public Action<RowGroupModel<TableItem>> OnExpandManuallyChanged { get; set; }


        [Parameter]
        public Func<CollectionViewGroup<TableItem>, int> GroupOrdering { get; set; }

        private bool _needScroll;

        public bool IsGlobalExpand
        {
            get
            {
                return TotalRows.Where(r => r is RowGroupModel<TableItem>).Cast<RowGroupModel<TableItem>>().ToList()
                .All(g => g.IsExpand);
            }
        }
        public void GlobalExpand()
        {
            //this.IsGlobalExpand = value ?? !this.IsGlobalExpand;
            if (!this.IsGlobalExpand)
            {
                this.Expand();
            }
            else
            {
                this.CollapseAll();
            }
            //this.LaunchStateHasChanged();
        }

        //public event EventHandler<bool> OnGlobalExpandChange;

        public void Expand(Func<CollectionViewGroup<TableItem>, bool> which = null)
        {
            //if(this.PagedCollectionView?.Groups != null)
            //{
            //    var allGroups = this.PagedCollectionView.Groups.SelectManyRecursive(g => g.SubGroups).Where(g => which == null || which(g));
            //    allGroups.ToList().ForEach(g => g.IsExpand = true);
            //}
            //this.OnGlobalExpandChange?.Invoke(this, true);
            TotalRows.Where(r => r is RowGroupModel<TableItem>).Cast<RowGroupModel<TableItem>>().ToList()
                .Where(g => which == null || which(g.ViewGroup))
                .ToList()
                .ForEach(r => r.IsExpand = true);
            ReactGroupingChanged();
        }

        private void CollapseAll()
        {
            //this.PagedCollectionView.Groups.SelectManyRecursive(g => g.SubGroups).ToList().ForEach(g => g.IsExpand = false);
            //this.OnGlobalExpandChange?.Invoke(this, false);
            TotalRows.Where(r => r is RowGroupModel<TableItem>).Cast<RowGroupModel<TableItem>>().ToList().ForEach(r => r.IsExpand = false);
            ReactGroupingChanged();
        }

        public void ReactGroupingChanged()
        {
            this.Compute();

            //ComputeGlobalExpand();

            this.LaunchStateHasChanged();
        }

        [Parameter]
        public override TableItem CurrentItem
        {
            get
            {
                if(this.PagedCollectionView != null && this.PagedCollectionView.CurrentItem != null)
                {
                    return this.PagedCollectionView.CurrentItem;
                }
                else
                {
                    return default(TableItem);
                }
            }
            set
            {
                if (this.PagedCollectionView != null && (this.PagedCollectionView.CurrentItem == null || !this.PagedCollectionView.CurrentItem.Equals(value)))
                {
                    this.PagedCollectionView.SetCurrentItem(value, false);
                    this.NotifyPropertyChanged();
                }
            }
        }

        //private void ComputeGlobalExpand()
        //{
        //    //si tous les groupes sont ouverts, on met a jour le IsGlobalExpand
        //    this.IsGlobalExpand = TotalRows.Where(r => r is RowGroupModel<TableItem>)
        //                                    .Cast<RowGroupModel<TableItem>>()
        //                                    .All(r => r.IsExpand);
        //}

        protected override void ComputeTotalRows(bool tryToKeepGroup, bool initialize)
        {
            if (this.PagedCollectionView?.IsGrouped ?? false)
            {
                _oldGroups = tryToKeepGroup ? TotalRows.Where(r => r is RowGroupModel<TableItem>).Cast< RowGroupModel<TableItem>>().ToList() : null;
                TotalRows.Clear();
                this.GenerateGroupAndRows(this.PagedCollectionView.Groups, null, initialize);

            }
            else if (this.CanRowExpand && GetSubRows != null)
            {
                _oldGroups = tryToKeepGroup ? TotalRows.Where(r => r is RowGroupModel<TableItem>)
                                                .Cast<RowGroupModel<TableItem>>()
                                                .Where(g => g.ViewGroup?.SubGroups?.Any() ?? false)
                                                .ToList() : null;
                TotalRows.Clear();

                //ici on va regenerer une arborescence de coolectionviewGroup
                Func<IEnumerable<TableItem>, int, List<CollectionViewGroup<TableItem>>> fillCollectionViewGroup = null;
                fillCollectionViewGroup = (items, idx) =>
                {
                    List<CollectionViewGroup<TableItem>> subList = new List<CollectionViewGroup<TableItem>>();
                    foreach(var item in items)
                    {
                        //on y met un id pour pas qu'il referme les groupes deja ouverts
                        var localGroup = new CollectionViewGroup<TableItem>($"grp-{item.GetHashCode().ToString()}")
                        {
                            Depth = idx,
                            Element = item,
                        };
                        subList.Add(localGroup);
                        var subItems = GetSubRows(item);
                        if (subItems?.Any() ?? false)
                        {
                            localGroup.SubGroups = fillCollectionViewGroup(this.GetOrderedItems(subItems), idx + 1);
                        }
                    }
                    return subList;
                };
                List<CollectionViewGroup<TableItem>> groups = fillCollectionViewGroup(this.GetOrderedItems(this.ReadOnlyCollection), 0);
                this.GenerateGroupAndRows(groups, null, initialize);
            }
            else
            {
                base.ComputeTotalRows(tryToKeepGroup, initialize);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if(_needScroll)
                this.ScrollToCurrentItem(false);
            _needScroll = false;
        }

        IList<RowGroupModel<TableItem>> _oldGroups;

        private void GenerateGroupAndRows(IList<CollectionViewGroup<TableItem>> groups, RowGroupModel<TableItem> parent, bool initialize)
        {
            var lGroups = GroupOrdering != null ?
                            groups.OrderBy(GroupOrdering).ToList() :
                            groups;

            foreach (var group in lGroups)
            {
                //on determine notre expand
                var expand = false;
                RowGroupModel<TableItem> lOldGroup = null;
                if (_oldGroups != null)
                {
                    //on pourrait theoriquement faire un single par défaut mais faudrai bien retester toutes les grouping grid
                    //tester le key posait pb, plusieurs groupes peuvent avoir le même key

                    //un jour faudra revoir ca, mais dans certains cas je me retrouve a bien vouloir tester le key car mes id sont regenérés donc différent (ex charg. des mails mss)
                    //je sais pas pourquoi dans certains cas ca passe
                    //en attendant je me srt d'une prop pour ne pas apporter d eregression mais quand meme que ca fasse le taff pour la liste des mails
                    if (IsGroupKeyUnique)
                    {
                        lOldGroup = _oldGroups?.FirstOrDefault(g => g.ViewGroup != null && (g.ViewGroup.Equals(group) ||
                                                                                    g.ViewGroup.Key != null && g.ViewGroup.Key.Equals(group.Key)));
                    }
                    else
                    {
                        lOldGroup = _oldGroups?.FirstOrDefault(g => g.ViewGroup != null && (g.ViewGroup.Equals(group) ||
                                                                                    g.ViewGroup.Id == group.Id));// ||
                    }
                    
                                                                                    //(g.ViewGroup.Key != null && g.ViewGroup.Key.Equals(group.Key))));

                    //var lListOldGroup = _oldGroups?.Where(g => g.ViewGroup != null && (g.ViewGroup.Equals(group) ||
                    //                                                                g.ViewGroup.Id == group.Id ||
                    //                                                                (g.ViewGroup.Key != null && g.ViewGroup.Key.Equals(group.Key))));
                }

                if (initialize)
                {
                    //attention on ne fait rien si le groupe a été bougé manuellement par le user
                    if(lOldGroup == null || !lOldGroup.IsExpandManuallyChanged)
                    {
                        expand = IsAutoExpand ? true : (ExpandAtInitialize?.Invoke(group) ?? false);
                    }
                   
                }
                if(!expand && _oldGroups != null)
                {
                    expand = lOldGroup?.IsExpand ?? false;
                }
                //on tente de recuperer l'info de groupement
                var current = new RowGroupModel<TableItem>()
                {
                    IsExpandManuallyChanged = lOldGroup?.IsExpandManuallyChanged ?? false,
                    IsExpand = expand,
                    ViewGroup = group,
                    Parent = parent
                };
                TotalRows.Add(current);
                if (group.SubGroups?.Any() ?? false)
                {
                    GenerateGroupAndRows(group.SubGroups, current, initialize);
                }
                else if(group.Items?.Any() ?? false)
                {
                    var orderedItems = this.GetOrderedItems(group.Items);
                    foreach(var item in orderedItems)
                    {
                        TotalRows.Add(new RowModel<TableItem>()
                        {
                            Element = item,
                            Parent = current
                        });
                    };
                }
                
            }
        }

        protected override bool ShouldRender()
        {
            return base.ShouldRender();
        }

        private bool eventInitialized;
        protected override void OnParametersSet()
        {
            //attention, on gère les évenement ici car nos grid peuvent etre initialises avec une pagesCollectionView
            //a null qui sera initilisé plus tard
            if (!eventInitialized)
            {
                //Console.WriteLine("GroupingGrid OnParametersSet");
                if (this.PagedCollectionView != null)
                {
                    //Console.WriteLine("GroupingGrid OnParametersSet PagedCollectionView not null");
                    this.PagedCollectionView.CurrentChanged -= PagedCollectionView_CurrentChanged;
                    this.PagedCollectionView.CurrentChanged += PagedCollectionView_CurrentChanged;

                    this.PagedCollectionView.OnCollapse -= PagedCollectionView_OnCollapse;
                    this.PagedCollectionView.OnCollapse += PagedCollectionView_OnCollapse;

                    this.PagedCollectionView.OnsScrollToCurrentItem -= PagedCollectionView_OnsScrollToCurrentItem;
                    this.PagedCollectionView.OnsScrollToCurrentItem += PagedCollectionView_OnsScrollToCurrentItem;

                    if (this.ListenCollectionChanged)
                    {
                        //Console.WriteLine("GroupingGrid OnParametersSet listen refresh");
                        this.PagedCollectionView.OnRefresh -= PagedCollectionView_OnRefresh;
                        this.PagedCollectionView.OnRefresh += PagedCollectionView_OnRefresh;
                    }

                    eventInitialized = true;
                }
            }
            if (this.IsAutoExpand)
            {
                //this.Expand();
                this.GlobalExpand();
            }
            base.OnParametersSet();
        }

        private void PagedCollectionView_OnsScrollToCurrentItem(object sender, EventArgs e)
        {
            _needScroll = true;
        }

        private void PagedCollectionView_OnCollapse(object sender, EventArgs e)
        {
            this.CollapseAll();
        }

        private void PagedCollectionView_CurrentChanged(object sender, EventArgs e)
        {
            //this.LaunchStateHasChanged();
            //on s'assure de ne pas tout recharger mais laisser les rows se recharger d'eux même si besoin
            this.NotifyPropertyChanged(nameof(this.CurrentItem));
        }

        private void PagedCollectionView_OnRefresh(object sender, EventArgs e)
        {
            //Console.WriteLine("GroupinGrid.PagedCollectionView_OnRefresh");
            this.RecomputeAll();

        }

        public override bool HaveExpander()
        {
            return this.PagedCollectionView.IsGrouped || this.CanRowExpand;
        }

        public override void HandleSelect(TableItem element, bool rightClick, MouseEventArgs args)
        {
            if (this.CanMultiSelect && this.PagedCollectionView != null && args != null)
            {
                if (args.ShiftKey && !this.PagedCollectionView.IsGrouped)
                {
                    //selection de tous les éléments entre currentItem et element
                    //on n'applique cette logique que sur les non groupés sinon trop le bordel
 
                    if (this.CurrentItem == null)
                    {
                        this.PagedCollectionView.SetCurrentItem(element);
                    }
                    else
                    {
                        if(this.PagedCollectionView.FilteredCollection != null)
                        {
                            //on prend l'index le plus petit entre nos éléments déja sélectionné et lenouvel élément
                            var minIndex = this.PagedCollectionView.SelectedItems?.Any() ?? false ?
                                            Math.Min(this.PagedCollectionView.SelectedItems.Select(i => this.PagedCollectionView.FilteredCollection.IndexOf(i)).Min(), this.PagedCollectionView.FilteredCollection.IndexOf(element)) :
                                            this.PagedCollectionView.FilteredCollection.IndexOf(element);
                            //et le plus grand
                            var maxIndex = this.PagedCollectionView.SelectedItems?.Any() ?? false ?
                                            Math.Max(this.PagedCollectionView.SelectedItems.Select(i => this.PagedCollectionView.FilteredCollection.IndexOf(i)).Max(), this.PagedCollectionView.FilteredCollection.IndexOf(element)) :
                                            this.PagedCollectionView.FilteredCollection.IndexOf(element);
                            if (this.PagedCollectionView.SelectedItems != null)
                                this.PagedCollectionView.SelectedItems.Clear();
                            //var minIndex = Math.Min(this.PagedCollectionView.FilteredCollection.IndexOf(this.CurrentItem), this.PagedCollectionView.FilteredCollection.IndexOf(element));
                            //var maxIndex = Math.Max(this.PagedCollectionView.FilteredCollection.IndexOf(this.CurrentItem), this.PagedCollectionView.FilteredCollection.IndexOf(element));
                            for (int i = minIndex; i <= maxIndex; i++)
                            {
                                this.PagedCollectionView.SetCurrentItem(this.PagedCollectionView.FilteredCollection.ElementAt(i), false);
                            }
                        }
                       

                    }
                }
                else if(args.CtrlKey)
                {
                    if (this.PagedCollectionView.SelectedItems?.Contains(element) ?? false)
                    {
                        //attention ici notre élement étant enlevé il faut s'assurer de ne pas le setter en currentItem
                        this.PagedCollectionView.SelectedItems.Remove(element);
                        element = default;
                        this.CurrentItem = element;
                    }
                    else
                    {
                        this.PagedCollectionView.SetCurrentItem(element, false);
                    }
                }
                else
                {
                    //cas de selection normal, on ne laisse que l'élément en sélectionné
                    //sauf si clic droit sur un élément déja sélection
                    if(!(rightClick && (this.PagedCollectionView.SelectedItems?.Contains(element) ?? false)))
                    {
                        this.PagedCollectionView.SetCurrentItem(element, true);
                    }
                    
                }
            }
            else
            {
                this.PagedCollectionView.SetCurrentItem(element);
            }
            this.NotifyPropertyChanged(nameof(CurrentItem));
            this.OnSelect?.Invoke(element, rightClick);
        }

        protected override IList<TableItem> ReadOnlyCollection => this.PagedCollectionView?.FilteredCollection;


        public override bool IsSelected(TableItem item)
        {
            return this.PagedCollectionView != null && this.CurrentItem != null && (this.CurrentItem.Equals(item)  || this.PagedCollectionView.SelectedItems.Contains(item));
        }

        public override void Dispose()
        {
            if (this.PagedCollectionView != null)
            {
                this.PagedCollectionView.OnCollapse -= PagedCollectionView_OnCollapse;
                this.PagedCollectionView.CurrentChanged -= PagedCollectionView_CurrentChanged;
                this.PagedCollectionView.OnsScrollToCurrentItem -= PagedCollectionView_OnsScrollToCurrentItem;
                if (this.ListenCollectionChanged)
                {
                    this.PagedCollectionView.OnRefresh -= PagedCollectionView_OnRefresh;
                }
            }
            base.Dispose(); 
        }
    }
}
