using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Collections.Specialized;


namespace iCareFone.Controls
{
    public interface IChildInfo
    {
        Rect GetChildRect(int itemIndex);
    }

    public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo, IChildInfo
    {
        private TranslateTransform m_objTranslateTransform = new TranslateTransform();

        private ScrollViewer m_ScrollOwner = null;

        private bool m_HorizontalScrollVisible = true;

        private bool m_VerticalScrollVisible = true;

        private Size m_ExtentSize = new Size(0.0, 0.0);

        private Size m_ViewportSize = new Size(0.0, 0.0);

        private Point m_Offset;
        
        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.RegisterAttached("ItemWidth", typeof(double), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.RegisterAttached("ItemHeight", typeof(double), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.RegisterAttached("Orientation", typeof(Orientation), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
        

        public ScrollViewer ScrollOwner
        {
            get
            {
                return m_ScrollOwner;
            }
            set
            {
                m_ScrollOwner = value;
            }
        }

        public bool CanHorizontallyScroll
        {
            get
            {
                return m_HorizontalScrollVisible;
            }
            set
            {
                m_HorizontalScrollVisible = value;
            }
        }

        public bool CanVerticallyScroll
        {
            get
            {
                return m_VerticalScrollVisible;
            }
            set
            {
                m_VerticalScrollVisible = value;
            }
        }

        public double HorizontalOffset
        {
            get
            {
                return m_Offset.X;
            }
        }

        public double VerticalOffset
        {
            get
            {
                return m_Offset.Y;
            }
        }

        public double ExtentHeight
        {
            get
            {
                return m_ExtentSize.Height;
            }
        }

        public double ExtentWidth
        {
            get
            {
                return m_ExtentSize.Width;
            }
        }

        public double ViewportHeight
        {
            get
            {
                return m_ViewportSize.Height;
            }
        }

        public double ViewportWidth
        {
            get
            {
                return m_ViewportSize.Width;
            }
        }
        
        public double ItemWidth
        {
            get
            {
                return (double)GetValue(ItemWidthProperty);
            }
            set
            {
                SetValue(ItemWidthProperty, value);
            }
        }

        public double ItemHeight
        {
            get
            {
                return (double)GetValue(ItemHeightProperty);
            }
            set
            {
                SetValue(VirtualizingWrapPanel.ItemHeightProperty, value);
            }
        }

        public Size ItemSize
        {
            get
            {
                return new Size(this.ItemWidth, this.ItemHeight);
            }
        }

        public Orientation Orientation
        {
            get
            {
                return (Orientation)GetValue(OrientationProperty);
            }
            set
            {
                SetValue(OrientationProperty, value);
            }
        }
        
        public VirtualizingWrapPanel()
        {
            base.RenderTransform = this.m_objTranslateTransform;
        }

        private Size CalculateExtent(Size availableSize, int itemCount)
        {
            if (this.Orientation == Orientation.Horizontal)
            {
                int num = this.CalculateChildrenPerRow(availableSize);
                return new Size((double)num * this.ItemSize.Width, this.ItemSize.Height * System.Math.Ceiling((double)itemCount / (double)num));
            }
            int num2 = this.CalculateChildrenPerCol(availableSize);
            return new Size(this.ItemSize.Width * System.Math.Ceiling((double)itemCount / (double)num2), (double)num2 * this.ItemSize.Height);
        }

        private void GetVisibleRange(out int firstVisibleItemIndex, out int lastVisibleItemIndex)
        {
            if (this.Orientation == Orientation.Horizontal)
            {
                int num = this.CalculateChildrenPerRow(this.m_ExtentSize);
                firstVisibleItemIndex = (int)System.Math.Floor(this.m_Offset.Y / this.ItemSize.Height) * num;
                lastVisibleItemIndex = (int)System.Math.Ceiling((this.m_Offset.Y + this.m_ViewportSize.Height) / this.ItemSize.Height) * num - 1;
                ItemsControl itemsOwner = ItemsControl.GetItemsOwner(this);
                int num2 = itemsOwner.HasItems ? itemsOwner.Items.Count : 0;
                if (lastVisibleItemIndex >= num2)
                {
                    lastVisibleItemIndex = num2 - 1;
                    return;
                }
            }
            else
            {
                int num3 = this.CalculateChildrenPerCol(this.m_ExtentSize);
                firstVisibleItemIndex = (int)System.Math.Floor(this.m_Offset.X / this.ItemSize.Width) * num3;
                lastVisibleItemIndex = (int)System.Math.Ceiling((this.m_Offset.X + this.m_ViewportSize.Width) / this.ItemSize.Width) * num3 - 1;
                ItemsControl itemsOwner2 = ItemsControl.GetItemsOwner(this);
                int num4 = itemsOwner2.HasItems ? itemsOwner2.Items.Count : 0;
                if (lastVisibleItemIndex >= num4)
                {
                    lastVisibleItemIndex = num4 - 1;
                }
            }
        }

        private Rect GetChildRect(int itemIndex, Size finalSize)
        {
            if (this.Orientation == Orientation.Horizontal)
            {
                int num = this.CalculateChildrenPerRow(finalSize);
                int num2 = itemIndex / num;
                int num3 = itemIndex % num;
                return new Rect((double)num3 * this.ItemSize.Width, (double)num2 * this.ItemSize.Height, this.ItemSize.Width, this.ItemSize.Height);
            }
            int num4 = this.CalculateChildrenPerCol(finalSize);
            int num5 = itemIndex / num4;
            int num6 = itemIndex % num4;
            return new Rect((double)num5 * this.ItemSize.Width, (double)num6 * this.ItemSize.Height, this.ItemSize.Width, this.ItemSize.Height);
        }

        private void ArrangeChild(int itemIndex, UIElement child, Size finalSize)
        {
            child.Arrange(this.GetChildRect(itemIndex, finalSize));
        }

        private int CalculateChildrenPerRow(Size availableSize)
        {
            int result;
            if (availableSize.Width == double.PositiveInfinity)
            {
                result = base.Children.Count;
            }
            else
            {
                result = System.Math.Max(1, (int)System.Math.Floor(availableSize.Width / this.ItemSize.Width));
            }
            return result;
        }

        private int CalculateChildrenPerCol(Size availableSize)
        {
            if (availableSize.Height == double.PositiveInfinity)
            {
                return base.Children.Count;
            }
            return System.Math.Max(1, (int)System.Math.Floor(availableSize.Height / this.ItemSize.Height));
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            this.UpdateScrollInfo(availableSize);
            int num;
            int num2;
            this.GetVisibleRange(out num, out num2);
            UIElementCollection internalChildren = base.InternalChildren;
            IItemContainerGenerator itemContainerGenerator = base.ItemContainerGenerator;
            GeneratorPosition position = itemContainerGenerator.GeneratorPositionFromIndex(num);
            int num3 = (position.Offset == 0) ? position.Index : (position.Index + 1);
            using (itemContainerGenerator.StartAt(position, GeneratorDirection.Forward, true))
            {
                int i = num;
                while (i <= num2)
                {
                    try
                    {
                        bool flag;
                        UIElement uIElement = itemContainerGenerator.GenerateNext(out flag) as UIElement;
                        if (uIElement == null)
                        {
                            return availableSize;
                        }
                        if (flag)
                        {
                            if (num3 >= internalChildren.Count)
                            {
                                base.AddInternalChild(uIElement);
                            }
                            else
                            {
                                base.InsertInternalChild(num3, uIElement);
                            }
                            itemContainerGenerator.PrepareItemContainer(uIElement);
                        }
                        uIElement.Measure(this.ItemSize);
                    }
                    catch
                    {
                    }
                    i++;
                    num3++;
                }
            }
            this.CleanUpItems(num, num2);
            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            IItemContainerGenerator itemContainerGenerator = base.ItemContainerGenerator;
            this.UpdateScrollInfo(finalSize);
            for (int i = 0; i < base.Children.Count; i++)
            {
                UIElement child = base.Children[i];
                int itemIndex = itemContainerGenerator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));
                this.ArrangeChild(itemIndex, child, finalSize);
            }
            return finalSize;
        }

        private void CleanUpItems(int minDesiredGenerated, int maxDesiredGenerated)
        {
            UIElementCollection internalChildren = base.InternalChildren;
            IItemContainerGenerator itemContainerGenerator = base.ItemContainerGenerator;
            for (int i = internalChildren.Count - 1; i >= 0; i--)
            {
                GeneratorPosition position = new GeneratorPosition(i, 0);
                int num = itemContainerGenerator.IndexFromGeneratorPosition(position);
                if (num < minDesiredGenerated || num > maxDesiredGenerated)
                {
                    itemContainerGenerator.Remove(position, 1);
                    base.RemoveInternalChildRange(i, 1);
                }
            }
        }

        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    base.RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                    return;
                default:
                    return;
            }
        }

        private void UpdateScrollInfo(Size availableSize)
        {
            ItemsControl itemsOwner = ItemsControl.GetItemsOwner(this);
            int itemCount = itemsOwner.HasItems ? itemsOwner.Items.Count : 0;
            Size size = this.CalculateExtent(availableSize, itemCount);
            if (size != this.m_ExtentSize)
            {
                this.m_ExtentSize = size;
                if (this.m_ScrollOwner != null)
                {
                    this.m_ScrollOwner.InvalidateScrollInfo();
                }
            }
            if (availableSize != this.m_ViewportSize)
            {
                this.m_ViewportSize = availableSize;
                if (this.m_ScrollOwner != null)
                {
                    this.m_ScrollOwner.InvalidateScrollInfo();
                }
            }
        }

        public void LineUp()
        {
            this.SetVerticalOffset(this.VerticalOffset - 10.0);
        }

        public void LineDown()
        {
            this.SetVerticalOffset(this.VerticalOffset + 10.0);
        }

        public void PageUp()
        {
            this.SetVerticalOffset(this.VerticalOffset - this.m_ViewportSize.Height);
        }

        public void PageDown()
        {
            this.SetVerticalOffset(this.VerticalOffset + this.m_ViewportSize.Height);
        }

        public void MouseWheelUp()
        {
            this.SetVerticalOffset(this.VerticalOffset - this.m_ViewportSize.Height);
        }

        public void MouseWheelDown()
        {
            this.SetVerticalOffset(this.VerticalOffset + this.m_ViewportSize.Height);
        }

        public void LineLeft()
        {
            this.SetHorizontalOffset(this.HorizontalOffset - 10.0);
        }

        public void LineRight()
        {
            this.SetHorizontalOffset(this.HorizontalOffset + 10.0);
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            return default(Rect);
        }

        public void MouseWheelLeft()
        {
            this.SetHorizontalOffset(this.HorizontalOffset - 10.0);
        }

        public void MouseWheelRight()
        {
            this.SetHorizontalOffset(this.HorizontalOffset + 10.0);
        }

        public void PageLeft()
        {
            this.SetHorizontalOffset(this.HorizontalOffset - this.m_ViewportSize.Width);
        }

        public void PageRight()
        {
            this.SetHorizontalOffset(this.HorizontalOffset + this.m_ViewportSize.Width);
        }

        public void SetHorizontalOffset(double offset)
        {
            if (offset < 0.0 || this.m_ViewportSize.Width >= this.m_ExtentSize.Width)
            {
                offset = 0.0;
            }
            else if (offset + this.m_ViewportSize.Width >= this.m_ExtentSize.Width)
            {
                offset = this.m_ExtentSize.Width - this.m_ViewportSize.Width;
            }
            this.m_Offset.X = offset;
            if (this.m_ScrollOwner != null)
            {
                this.m_ScrollOwner.InvalidateScrollInfo();
            }
            this.m_objTranslateTransform.X = -offset;
            base.InvalidateMeasure();
        }

        public void SetVerticalOffset(double offset)
        {
            if (offset < 0.0 || this.m_ViewportSize.Height >= this.m_ExtentSize.Height)
            {
                offset = 0.0;
            }
            else if (offset + this.m_ViewportSize.Height >= this.m_ExtentSize.Height)
            {
                offset = this.m_ExtentSize.Height - this.m_ViewportSize.Height;
            }
            this.m_Offset.Y = offset;
            if (this.m_ScrollOwner != null)
            {
                this.m_ScrollOwner.InvalidateScrollInfo();
            }
            this.m_objTranslateTransform.Y = -offset;
            base.InvalidateMeasure();
        }

        public Rect GetChildRect(int itemIndex)
        {
            return this.GetChildRect(itemIndex, this.m_ExtentSize);
        }
    }
}
