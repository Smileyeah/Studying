using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Interactivity;
using System.Windows.Media;

using TS.UI.Controls.Adorner;
using TS.UI.Presentations;


namespace TS.UI.Behaviours
{
    /// <summary>
    /// 实现DataGrid框选
    /// </summary>
    public class DataGridSelectionBehaviour : Behavior<DataGrid>
    {
        #region Property

        /// <summary>
        /// 间距
        /// </summary>
        public static readonly DependencyProperty MarginProperty =
            DependencyProperty.Register("Margin", typeof(Thickness), typeof(DataGridSelectionBehaviour), new PropertyMetadata(new Thickness(0.0, 0.0, 0.0, 0.0)));

        /// <summary>
        /// 时长
        /// </summary>
        public static readonly DependencyProperty OpacityProperty =
           DependencyProperty.Register("Opacity", typeof(double), typeof(DataGridSelectionBehaviour), new PropertyMetadata(1.0));

        /// <summary>
        /// 遮盖
        /// </summary>
        public static readonly DependencyProperty CoverProperty =
            DependencyProperty.Register("Cover", typeof(Brush), typeof(DataGridSelectionBehaviour), new PropertyMetadata(new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0x50, 0x00, 0x00, 0x00))));

        #endregion


        #region Memeber

        public Thickness Margin
        {
            get { return (Thickness)GetValue(MarginProperty); }
            set { SetValue(MarginProperty, value); }
        }

        public double Opacity
        {
            get { return (double)GetValue(OpacityProperty); }
            set { SetValue(OpacityProperty, value); }
        }

        public Brush Cover
        {
            get { return (Brush)GetValue(CoverProperty); }
            set { SetValue(CoverProperty, value); }
        }

        #endregion


        /// <summary>
        /// 遮罩
        /// </summary>
        private SelectionAdorner _Adorner = null;
        
        /// <summary>
        /// 数据容器
        /// </summary>
        private ItemsPresenter _Presenter = null;

        /// <summary>
        /// 滚动容器
        /// </summary>
        private ScrollViewer _Viewer = null;

        /// <summary>
        /// 竖直滚动条
        /// </summary>
        private ScrollBar _Roller = null;

        /// <summary>
        /// 选择容器
        /// </summary>
        private DataGrid _Selector = null;
        
        /// <summary>
        /// 框选起点
        /// </summary>
        private Point _Anchor = new Point(0.0, 0.0);

        /// <summary>
        /// 框选终点
        /// </summary>
        private Point _Focus = new Point(0.0, 0.0);

        /// <summary>
        /// 滚动单位像素
        /// </summary>
        private double SCROLL_UNIT_PIXEL = 1;

        /// <summary>
        /// 鼠标左键是否按下
        /// </summary>
        private bool _IsPressed = false;


        protected override void OnAttached()
        {
            this._Selector = this.AssociatedObject;
            if (null != this._Selector)
            {
                this._Selector.Loaded += this.Selector_Loaded;
                this._Selector.SizeChanged += this.Selector_SizeChanged;
            }

            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            do
            {
                if (null != this._Selector)
                {
                    this._Selector.PreviewMouseMove -= this.Selector_PreviewMouseMove;
                    this._Selector.PreviewMouseLeftButtonDown -= this.Selector_PreviewMouseLeftButtonDown;
                    this._Selector.PreviewMouseLeftButtonUp -= this.Selector_PreviewMouseLeftButtonUp;
                    this._Selector.SizeChanged -= this.Selector_SizeChanged;
                    this._Selector.Loaded -= this.Selector_Loaded;
                }

                if (null != this._Viewer)
                {
                    this._Viewer.ScrollChanged -= this.Viewer_ScrollChanged;
                }

                this._Roller = null;
                this._Viewer = null;
                this._Presenter = null;
                this._Selector = null;
            }
            while ( false );
            
            base.OnDetaching();
        }

        private void Selector_Loaded(object sender, RoutedEventArgs e)
        {
            do
            {
                if (null == this._Selector)
                {
                    break;
                }
                
                this._Viewer = VisualTreeHelperEx.GetChildObject<ScrollViewer>(this._Selector);
                if (null == this._Viewer)
                {
                    break;
                }

                var scp = VisualTreeHelperEx.GetChildObject<ScrollContentPresenter>(this._Selector);
                if (null == scp)
                {
                    return;
                }

                this._Presenter = VisualTreeHelperEx.GetChildObject<ItemsPresenter>(scp);
                if (null == this._Presenter)
                {
                    break;
                }

                var al = AdornerLayer.GetAdornerLayer(this._Presenter);
                if (null == al)
                {
                    break;
                }

                var sbs = VisualTreeHelperEx.GetDirectChildObjects<ScrollBar>(this._Viewer);
                if (null != sbs && sbs.Count > 0)
                {
                    foreach (var sb in sbs)
                    {
                        if (Orientation.Vertical == sb.Orientation)
                        {
                            this._Roller = sb;
                            break;
                        }
                    }
                }

                this._Adorner = new SelectionAdorner(this._Presenter);
                this._Adorner.Cover = this.Cover;
                this._Adorner.Opacity = this.Opacity;

                al.Width = this._Presenter.ActualWidth;
                al.Height = this._Presenter.ActualHeight;
                al.Add(this._Adorner);

                this._Selector.PreviewMouseLeftButtonDown += this.Selector_PreviewMouseLeftButtonDown;
                this._Selector.PreviewMouseLeftButtonUp += this.Selector_PreviewMouseLeftButtonUp;
                this._Selector.PreviewMouseMove += this.Selector_PreviewMouseMove;

                this._Viewer.ScrollChanged += this.Viewer_ScrollChanged;

                if (ScrollUnit.Item == VirtualizingPanel.GetScrollUnit(this._Selector))
                {
                    this.SCROLL_UNIT_PIXEL = this._Selector.RowHeight;
                }
            }
            while ( false );
        }
        
        private void Selector_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(null != this._Presenter)
            {
                var al = AdornerLayer.GetAdornerLayer(this._Presenter);
                if (null != al)
                {
                    al.Width = e.NewSize.Width;
                    al.Height = e.NewSize.Height;
                }
            }
        }
        
        private void Selector_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            do
            {
                if (null != this._Roller && this._Roller.IsMouseCaptureWithin)
                {
                    // 避免竖直滚动条影响
                    break;
                }

                if ( this.IsHeaderEvent(sender, e) )
                {
                    // 于列表表头处单击，不予处理
                    break;
                }

                this._IsPressed = true;
                this._Focus = e.GetPosition(this._Presenter);
                this._Anchor = new Point(this._Focus.X, this._Focus.Y + this._Viewer.VerticalOffset * this.SCROLL_UNIT_PIXEL);
                this._Adorner.StartPosition = this._Anchor;
                this._Adorner.EndPosition = this._Anchor;
            }
            while ( false );
        }

        private void Selector_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this._IsPressed = false;

            do
            {
                if ( !this._Adorner.IsDragging )
                {
                    if ( this.IsViewerEvent(sender, e) )
                    {
                        // 于空白处左键单击，需要取消勾选
                        this._Selector.UnselectAll();
                    }
                    break;
                }

                this._Presenter.ReleaseMouseCapture();
                this._Adorner.IsDragging = false;
                this._Anchor = new Point(0.0, 0.0);
                this._Adorner.StartPosition = new Point(0.0, 0.0);
                this._Adorner.EndPosition = new Point(0.0, 0.0);
            }
            while ( false );
        }

        private void Selector_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            do
            {
                if ( !this._IsPressed )
                {
                    // 非框选状态
                    break;
                }
                
                if (null != this._Roller && this._Roller.IsMouseCaptureWithin)
                {
                    // 避免竖直滚动条影响
                    break;
                }

                var tempFocus = e.GetPosition(this._Presenter);
                if ( tempFocus.Equals(this._Focus) )
                {
                    break;
                }

                if ( !this._Adorner.IsDragging )
                {
                    this._Adorner.IsDragging = true;
                    this._Presenter.CaptureMouse();

                    if ( this.IsViewerEvent(sender, e) )
                    {
                        // 于空白处启动框选，需要取消勾选
                        this._Selector.UnselectAll();
                    }
                }

                this._Focus = tempFocus;
                this.OnAdornerChanged();
            }
            while ( false );
        }

        private void Viewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if ( this._Adorner.IsDragging )
            {
                this.OnAdornerChanged();
            }
        }

        private void OnAdornerChanged()
        {
            double tempVerticalOffset = this._Viewer.VerticalOffset * this.SCROLL_UNIT_PIXEL;
            do
            {
                if (tempVerticalOffset >= this._Anchor.Y)
                {
                    this._Adorner.StartPosition = new Point(this._Anchor.X, 0.0);
                    break;
                }

                if (tempVerticalOffset + this._Presenter.ActualHeight <= this._Anchor.Y)
                {
                    this._Adorner.StartPosition = new Point(this._Anchor.X, this._Presenter.ActualHeight);
                    break;
                }

                this._Adorner.StartPosition = new Point(this._Anchor.X, this._Anchor.Y - tempVerticalOffset);

            }
            while ( false );

            this._Adorner.EndPosition = this._Focus;

            var items = VisualTreeHelperEx.GetDirectChildObjects<DataGridRow>(this._Presenter);
            if (null != items && items.Count > 0)
            {
                Point tempFocus = new Point(this._Focus.X, this._Focus.Y + tempVerticalOffset);
                foreach (var item in items)
                {
                    this.OnOptionChanged(item, this._Anchor, tempFocus, tempVerticalOffset);
                }
            }
        }
        
        private void OnOptionChanged(DataGridRow dgr, Point tempAnchor, Point tempFocus, double tempVerticalOffset)
        {
            Point tempLeftTop = dgr.TranslatePoint(new Point(0.0, 0.0), this._Presenter);
            Point tempRightDown = dgr.TranslatePoint(new Point(dgr.ActualWidth, dgr.ActualHeight), this._Presenter);

            tempLeftTop.Y += tempVerticalOffset;
            tempRightDown.Y += tempVerticalOffset;

            double tempMinX = 0;
            double tempMaxX = 0;
            double tempMinY = 0;
            double tempMaxY = 0;
            if (tempAnchor.X > tempFocus.X)
            {
                tempMinX = tempFocus.X;
                tempMaxX = tempAnchor.X;
            }
            else
            {
                tempMinX = tempAnchor.X;
                tempMaxX = tempFocus.X;
            }

            if (tempAnchor.Y > tempFocus.Y)
            {
                tempMinY = tempFocus.Y;
                tempMaxY = tempAnchor.Y;
            }
            else
            {
                tempMinY = tempAnchor.Y;
                tempMaxY = tempFocus.Y;
            }

            if (   tempMinX < tempRightDown.X - this.Margin.Right
                && tempMaxX > tempLeftTop.X + this.Margin.Left
                && tempMinY < tempRightDown.Y - this.Margin.Bottom
                && tempMaxY > tempLeftTop.Y + this.Margin.Top
                )
            {
                if ( !dgr.IsSelected )
                {
                    dgr.IsSelected = true;
                }
                return;
            }

            if ( dgr.IsSelected )
            {
                dgr.IsSelected = false;
            }
        }

        private bool IsHeaderEvent(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // 避免列表Header鼠标事件影响
            var obj = VisualTreeHelperEx.GetAncestorOrSelf<DataGridColumnHeader, DataGrid>(e.OriginalSource as DependencyObject);
            return (null != obj);
        }

        private bool IsViewerEvent(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // 避免列表Header鼠标事件影响
            bool isViewer = true;
            do
            {
                var objr = VisualTreeHelperEx.GetAncestorOrSelf<DataGridRow, DataGrid>(e.OriginalSource as DependencyObject);
                if (null != objr)
                {
                    isViewer = false;
                    break;
                }

                var objh = VisualTreeHelperEx.GetAncestorOrSelf<DataGridColumnHeader, DataGrid>(e.OriginalSource as DependencyObject);
                if (null != objh)
                {
                    isViewer = false;
                    break;
                }
            }
            while ( false );
            return isViewer;
        }

    }

}
