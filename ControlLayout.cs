using Microsoft.Maui.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSample
{
    static public class Helper
    {
        static public int ColumnCount = 20;

        static public int RowCount = 20;

        static public double ColumnWidth = 100;
    }

    public class CustomContainer : ControlLayout
    {
        public CustomContainer()
        {
        }

        internal override Size MeasureContent(double widthConstraint, double heightConstraint)
        {
            if (this.Children.Count == 0)
            {
                for (int i = 0; i < Helper.RowCount; i++)
                {
                    this.Add(new CustomRow(i));
                }
            }

            foreach (var child in this.Children)
            {
                child.Measure(Helper.ColumnCount * 100, 50);
                (child as CustomRow)?.CrossPlatformMeasure(widthConstraint, heightConstraint);
            }

            return new SizeRequest(new Size(Helper.ColumnCount * 100, Helper.RowCount * 50));
        }

        internal override Size ArrangeContent(Rect bounds)
        {
            var dy = 0;
            foreach (var child in this.Children)
            {
                child.Arrange(new Rect(bounds.X, dy, Helper.ColumnCount * 100, 50));
                (child as CustomRow)?.CrossPlatformArrange(bounds);
                dy += 50;
            }

            return bounds.Size;
        }

        protected override ILayoutManager CreateLayoutManager()
        {
            return new ControlLayoutManager(this);
        }
    }

    public class CustomRow : ControlLayout
    {
        public CustomRow(int index)
        {
            this.Index = index;
        }

        internal int Index = 0;

        internal override Size MeasureContent(double widthConstraint, double heightConstraint)
        {
            if (this.Children.Count == 0)
            {
                for (int i = 0; i < Helper.ColumnCount; i++)
                {
                    this.Add(new CustomCell(i));
                }
            }

            for (int i = 0; i < this.Children.Count; i++)
            {
                var width = i == 0 ? Helper.ColumnWidth : 100;
                this.Children[i].Measure(width, 50);
            }

            return new SizeRequest(new Size(Helper.ColumnCount * 100, 50));
        }

        internal override Size ArrangeContent(Rect bounds)
        {
            double dx = 0;
            for (int i = 0; i < this.Children.Count; i++)
            {
                var width = i == 0 ? Helper.ColumnWidth : 100;
                this.Children[i].Arrange(new Rect(dx, bounds.Y, width, 50));
                dx += width;
            }

            return bounds.Size;

        }

        protected override ILayoutManager CreateLayoutManager()
        {
            return new ControlLayoutManager(this);
        }
    }

    public class CustomCell : ContentView
    {
        public CustomCell(int index)
        {
            this.Index = index;
            if(index == 0)
            {
                this.Content = new Label() { Text = "Index" + index.ToString(), Padding = new Thickness(10), BackgroundColor = Colors.LightBlue };
            }
            else
            {
                this.Content = new Label() { Text = "Index" + index.ToString(), Padding = new Thickness(10), BackgroundColor = Colors.LightPink };
            }
            if (index == 0)
            {
                var panGesture = new PanGestureRecognizer();
                panGesture.PanUpdated += OnPanUpdated;
                this.GestureRecognizers.Add(panGesture);
            }
        }

        internal int Index = 0;

        double oldX = 0;
        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (e.StatusType == GestureStatus.Running)
            {
                var delta = e.TotalX - oldX;
                double newWidth = Helper.ColumnWidth + delta;
                oldX = e.TotalX;
                if (newWidth > 0)
                {
                    Helper.ColumnWidth = newWidth;
                }
            }
            else if (e.StatusType == GestureStatus.Completed)
            {
                oldX = 0;
            }

            (this.Parent.Parent as IView)?.InvalidateMeasure();
        }
    }

    public abstract class ControlLayout : Layout
    {
        internal abstract Size ArrangeContent(Rect bounds);

        internal abstract Size MeasureContent(double widthConstraint, double heightConstraint);
    }

    internal class ControlLayoutManager : LayoutManager
    {
        ControlLayout layout;
        internal ControlLayoutManager(ControlLayout layout) : base(layout)
        {
            this.layout = layout;
        }

        public override Size ArrangeChildren(Rect bounds) => this.layout.ArrangeContent(bounds);

        public override Size Measure(double widthConstraint, double heightConstraint) => this.layout.MeasureContent(widthConstraint, heightConstraint);
    }
}
