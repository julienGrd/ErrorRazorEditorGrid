using ErrorRazorEditorGrid.Grid.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErrorRazorEditorGrid.Grid
{
    public partial class GeckosGridRowRenderer<TableItem>
    {
        [CascadingParameter]
        public BaseGridComponent<TableItem> Container { get; set; }

        [Parameter]
        public int Index { get; set; }

        [Parameter]
        public BaseRowModel<TableItem> Element { get; set; }
    }
}


