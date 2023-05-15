using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;

namespace ErrorRazorEditorGrid.Grid
{
    /// <summary>
    /// todo migration ajouter la constraint lorsque ce sera possible de faire ca
    /// https://github.com/aspnet/AspNetCore/issues/8433
    /// </summary>
    /// <typeparam name="TableItem"></typeparam>
    public partial class GeckosGrid<TableItem> //where TableItem : INotifyPropertyChanged
    {

        [Parameter]
        public IEnumerable<TableItem> Items { get; set; } = Enumerable.Empty<TableItem>();


        private TableItem _currentItem;

        [Parameter]
        public override TableItem CurrentItem
        {
            get
            {
                return this._currentItem;
            }
            set
            {
                if (!(this._currentItem?.Equals(value) ?? false))
                {
                    this._currentItem = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        protected override IList<TableItem> ReadOnlyCollection => this.Items?.ToList();
    }
}


