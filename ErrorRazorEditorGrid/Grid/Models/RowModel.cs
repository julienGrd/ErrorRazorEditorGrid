
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ErrorRazorEditorGrid.Grid.Models
{ 
    public abstract class BaseRowModel<T>
    {
        //public RowGroupModel<T> Parent { get; set; }

        public bool CanShow => true;

        //protected bool CanManageDifferentHeight => typeof(IMeasurableItem).GetTypeInfo().IsAssignableFrom(typeof(T).Ge‌​tTypeInfo());

        public abstract double GetHeight(double widthTable, bool showSubLine, Func<double, double> remConverter);

        public abstract void ResetHeight();

        protected const double ItemHeight = 2.3;
    }

    public class RowModel<T> : BaseRowModel<T>
    {
        public T Element { get; set; }

        //public IMeasurableItem MeasurableElement => Element as IMeasurableItem;

        public override double GetHeight(double widthTable, bool showSubLine, Func<double, double> remConverter)
        {
            //if (CanManageDifferentHeight)
            //{
            //    return MeasurableElement.RealMesure ?? remConverter(MeasurableElement.GetLineHeight(widthTable, showSubLine));
            //}
            //else
            //{

            //}
            return remConverter(ItemHeight);
        }

        public override void ResetHeight()
        {
            //if (CanManageDifferentHeight)
            //{
            //    MeasurableElement.RealMesure = null;
            //}
        }
    }

    //public class RowGroupModel<T> : BaseRowModel<T>
    //{

    //    public void ManuallyExpand(bool pValue)
    //    {
    //        IsExpand = pValue;
    //        IsExpandManuallyChanged = true;
    //    }

    //    public bool IsExpand { get; set; }


    //    //on garde aussi ce booleen pour ne pas par inadvertance ouvrir ou fermer le groupe sur les logiques d'init si ce drnier a été volontairement fermé par le user
    //    public bool IsExpandManuallyChanged { get; set; }

    //    //public CollectionViewGroup<T> ViewGroup { get; set; }

    //    public override double GetHeight(double widthTable, bool showSubLine, Func<double, double> remConverter)
    //    {
    //        return ItemHeight;
    //    }

    //    public override void ResetHeight()
    //    {
    //       //rien a faire, c'est normal
    //    }
    //}
}
