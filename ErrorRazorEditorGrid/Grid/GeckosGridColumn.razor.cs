using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.AspNetCore.Components.Web;

namespace ErrorRazorEditorGrid.Grid
{
    public partial class GeckosGridColumn<T> : IDisposable
    {
        [CascadingParameter]
        public BaseGridComponent<T> Parent { get; set; }

        [Parameter]
        public RenderFragment<T> Body { get; set; }

        [Parameter]
        public RenderFragment<T> EditTemplate { get; set; }

        [Parameter]
        public bool CanEdit { get; set; }

        [Parameter]
        public string Header { get; set; }

        [Parameter]
        public string Width { get; set; }

        [Parameter]
        public string MinWidth { get; set; }

        [Parameter]
        public string MaxWidth { get; set; }

        [Parameter]
        public bool FitContent { get; set; }

        [Parameter]
        public bool CanOrder { get; set; }

        [Parameter]
        public string OrderingExpression { get; set; }

        [Parameter]
        public string HeaderTitle { get; set; }

        [Parameter]
        public Func<T, string> CellTitle { get; set; }

        [Parameter]
        public RenderFragment CustomHeader { get; set; }

        [Parameter]
        public int Index { get; set; }

        [Parameter]
        public string Style { get; set; }

        [Parameter]
        public Func<T, string> CssClass { get; set; }

        [Parameter]
        public Action<MouseEventArgs, T> OnRightClickCell { get; set; }

        protected override void OnInitialized()
        {
            if (Parent == null)
                throw new ArgumentNullException(nameof(Parent));
            Parent.AddColumn(this);
            base.OnInitialized();
        }


        public ListSortDirection? CurrentDirection { get; set; }

        public string GetDirectionClass()
        {
            string className = string.Empty;
            if (this.CanOrder && !this.Parent.Disabled)
            {
                className = "orderable";
                if (this.CurrentDirection.HasValue)
                {
                    className += " ordered";
                    if (this.CurrentDirection == ListSortDirection.Ascending)
                    {
                        className += " asc";
                    }
                    else
                    {
                        className += " desc";
                    }
                }
            }
            return className;
        }

        public void Dispose()
        {
            if (Parent != null)
            {
                Parent.RemoveColumn(this);
            }
        }

        protected void Order()
        {
            if (!Parent.Disabled && this.CanOrder)
            {
                var currentDirection = CurrentDirection == null ? ListSortDirection.Ascending
                                    : (CurrentDirection == ListSortDirection.Descending ? ListSortDirection.Ascending
                                    : ListSortDirection.Descending);
                this.CurrentDirection = currentDirection;
                this.Parent.Order(this.OrderingExpression, currentDirection, this.Index);
            }

        }

        public void ResetDirection()
        {
            this.CurrentDirection = null;
            //this.LaunchStateHasChanged();
        }
    }
}


