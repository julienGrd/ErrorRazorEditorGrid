using ErrorRazorEditorGrid.Grid.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
namespace ErrorRazorEditorGrid.Grid
{
    public partial class GeckosListGridRow<TableItem>
    {
        [CascadingParameter]
        public BaseGridComponent<TableItem> Container { get; set; }

        [Parameter]
        public IEnumerable<BaseRowModel<TableItem>> Items { get; set; }
    }
}

