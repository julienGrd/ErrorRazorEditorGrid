using Is.Geckos.Blazor.Client.Components.Framework.Grid.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Is.Geckos.Blazor.Client.Components.Framework.Grid
{
    public partial class GeckosHeaderGroupingGrid<TableItem> : IDisposable
    {
        [CascadingParameter]
        public GeckosGroupingGrid<TableItem> Container { get; set; }

        [Parameter]
        public RowGroupModel<TableItem> Group { get; set; }

        private void Expand()
        {
            this.Group.ManuallyExpand(!this.Group.IsExpand);
            this.Container.ReactGroupingChanged();
            this.Container.OnExpandManuallyChanged?.Invoke(this.Group);
        }

        protected RenderFragment GroupHeaderText()
        {
            if(this.Container.GetCustomHeader != null)
            {
                return this.Container.GetCustomHeader(this.Group.ViewGroup);
            }
            else
            {
                int i = 1;
                return builder =>
                {
                    builder.OpenElement(i++, "div");
                    builder.AddAttribute(i++, "style", $"display: flex;justify-content: right;align-items: center;{Container.StyleContentGroup}");
                    builder.OpenElement(i++, "span");
                    builder.AddContent(i++, $"{Group.ViewGroup.Key}");
                    builder.CloseElement();
                    builder.OpenElement(i++, "span");
                    builder.AddAttribute(i++, "class", "text-grey-small ml-2");
                    builder.AddContent(i++, $"({Group.ViewGroup.Items.Count()})");
                    builder.CloseElement();
                    builder.CloseElement();
                };
            }
        }


    }
}
