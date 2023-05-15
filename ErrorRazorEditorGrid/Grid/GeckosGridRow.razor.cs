using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;

namespace ErrorRazorEditorGrid.Grid
{
    /// <summary>
    /// todo migration ajouter la constraint lorsque ce sera possible de faire ca
    /// https://github.com/aspnet/AspNetCore/issues/8433
    /// </summary>
    /// <typeparam name="TableItem"></typeparam>
    public partial class GeckosGridRow<TableItem> : IDisposable// where TableItem : INotifyPropertyChanged
    {
        [CascadingParameter]
        public BaseGridComponent<TableItem> Container { get; set; }

        [Parameter]
        public TableItem Element { get; set; }

        //[Parameter]
        //public RowGroupModel<TableItem> GroupedElement { get; set; }

        [Parameter]
        public RenderFragment<TableItem> GridSubRow { get; set; }

        [Parameter]
        public int Index { get; set; }

        [Parameter]
        public Func<TableItem, bool> IsHidden { get; set; }


        //[Inject]
        //private IBlazorContextMenuService _blazorContextMenuService { get; set; }

        //private bool isMeasurable => Element is IMeasurableItem;

        //private IMeasurableItem MeasurableItem => Element as IMeasurableItem;

        private bool IsSubRow => GridSubRow != null;


        protected bool IsSelected { get; set; }

        protected override void OnParametersSet()
        {
            this.ManageSuscribe(true);
            this.ManageSelection();
            base.OnParametersSet();
        }

        protected override void OnInitialized()
        {
            this.Container.PropertyChanged -= Container_PropertyChanged;
            this.Container.PropertyChanged += Container_PropertyChanged;
            base.OnInitialized();
        }

        private string Id => $"{Container.Id}Row{Index}";

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            //Console.WriteLine($"rendering table {this?.Container?.Id} / row {Index}");
            //if (isMeasurable && !MeasurableItem.RealMesure.HasValue)
            //{
            //    //on le mesure concrètement
            //    MeasurableItem.RealMesure = await InvokeJsAsync<double>("geckos.gridManager.mesureHeight", Id);
            //    //Parent.ReplaceHeight(MeasurableItem);
            //}
            await base.OnAfterRenderAsync(firstRender);
        }

        protected override bool ShouldRender()
        {
            return base.ShouldRender();
        }

        private void Expand()
        {
            //var groupedContainer = this.Container as GeckosGroupingGrid<TableItem>;
            //if (this.GroupedElement != null && groupedContainer != null)
            //{
            //    this.GroupedElement.ManuallyExpand(!this.GroupedElement.IsExpand);
            //    groupedContainer.ReactGroupingChanged();
            //}

        }

        private void Container_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //on s'abonne au changement de selectedItem savoir si on doit rafraichir notre élément
            if (e.PropertyName == nameof(this.Container.CurrentItem))
            {
                this.ManageSelection();
            }
        }

        protected IList<int> ColumnsEdited { get; set; } = new List<int>();


        protected void OnCellDblClick(GeckosGridColumn<TableItem> column)
        {
            if (column.CanEdit)
            {
                if (!ColumnsEdited.Contains(column.Index))
                {
                    ColumnsEdited.Add(column.Index);
                }
            }
        }

        private void ManageSelection()
        {
            //si le current item est notre elemnt, on le sélectionne
            if (!this.IsSelected && this.Container.IsSelected(Element))
            {
                this.IsSelected = true;
                this.LaunchStateHasChanged();
            }
            else if (this.IsSelected && !this.Container.IsSelected(Element))
            {
                //on n'est plus sélectionné
                this.IsSelected = false;
                this.LaunchStateHasChanged();
            }
        }

        protected void RightClickRow(TableItem element, MouseEventArgs args)
        {
            if (Container.DisableSelection) return;
            if (!Container.CanSelectRow?.Invoke(Element) ?? false) return;
            //pas besoin de recharger, on va laisser l'event de changement de prop selected du container se lever et faire ca proprement
            if (this.Container.CanSelectOnRightClick)
            {
                this.Container.HandleSelect(element, true, args);

                if (!string.IsNullOrEmpty(Container.AssociatedContextMenuId))
                {
                    //_blazorContextMenuService.ShowMenu(Container.AssociatedContextMenuId, (int)args.ClientX, (int)args.ClientY);
                }
            }
            //this.LaunchStateHasChanged();
        }

        protected void RightClickCell(TableItem element, MouseEventArgs args, GeckosGridColumn<TableItem> pColumn)
        {
            if (pColumn.OnRightClickCell != null)
            {
                pColumn.OnRightClickCell.Invoke(args, element);
            }
        }

        protected void SelectElement(TableItem element, MouseEventArgs args)
        {
            if (Container.DisableSelection) return;
            if (!Container.CanSelectRow?.Invoke(Element) ?? false) return;
            //pas besoin de recharger, on va laisser l'event de changement de prop selected du container se lever et faire ca proprement
            this.Container.HandleSelect(element, false, args);
            //this.LaunchStateHasChanged();
        }

        protected void SelectAndValideElement(TableItem element, MouseEventArgs args)
        {
            if (Container.DisableSelection) return;
            if (!Container.CanSelectRow?.Invoke(Element) ?? false) return;
            this.Container.HandleDblClick(element);
        }

        private void ManageSuscribe(bool withSuscribe)
        {
            if (this.Container.ListenPropertyChanged)
            {
                //todo migration on ne peiut pas mettre de contrainter sur le type des composants pour le moement en blazor
                //on est donc obligé de vérifier le type cans le cas ou on veut s'abonner au changement de propriété
                var propertyChangedObject = this.Element as INotifyPropertyChanged;
                if (propertyChangedObject != null)
                {
                    propertyChangedObject.PropertyChanged -= PropertyChangedObject_PropertyChanged;
                    if (withSuscribe)
                    {
                        propertyChangedObject.PropertyChanged += PropertyChangedObject_PropertyChanged;
                    }

                }
            }
        }

        private void PropertyChangedObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.LaunchStateHasChanged();
        }

        public override void Dispose()
        {
            this.Container.PropertyChanged -= Container_PropertyChanged;
            this.ManageSuscribe(false);
            base.Dispose();
        }
    }
}


