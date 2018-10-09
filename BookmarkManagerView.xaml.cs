using iCareFone.Controls;
using iCareFone.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TS.Foundation.Data.Bookmark;
using TS.UI.Presentations;
using Xceed.Wpf.Toolkit;

namespace iCareFone.View
{
    /// <summary>
    /// BookmarkManagerView.xaml 的交互逻辑
    /// </summary>
    public partial class BookmarkManagerView : UserControl
    {
        /// <summary>
        /// 框选定点
        /// </summary>
        private Point Start_Point;

        public BookmarkManagerView()
        {
            InitializeComponent(); 
        }

        /************************************************************************
         * 1、右键单个文件夹弹窗给新建、重命名和删除三个功能
         * 2、右键单个书签弹窗给编辑、移动到文件夹、删除和访问网站四个功能
         * 3、右键多个书签弹窗给编辑、移动到文件夹、删除和访问网站四个功能，编辑和访问网站灰色显示，功能不可用
         * 4、右键不同文件夹下的书签，弹窗给删除和刷新两个功能
         * 5、右键文件夹和其他文件夹内书签，弹窗给删除和刷新两个功能
         * 6、右键文件夹和文件夹下的书签，弹窗给删除和刷新两个功能
        ************************************************************************/
        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            List<BookmarkTreeNode> SelectedTreeNodes = (List<BookmarkTreeNode>)TreeViewExtensions.GetSelectedItems(host_Bookmarks);
            var treeViewItem = VisualTreeHelperEx.GetParentObject<TreeListViewItem>(e.OriginalSource as DependencyObject) as TreeListViewItem;
            //var treeViewItem = sender as TreeListViewItem;
            if (treeViewItem != null && SelectedTreeNodes != null)
            {
                if (SelectedTreeNodes.Count > 1)
                {
                    if (SelectedTreeNodes.FindAll(p => p.IsFolder).Count == SelectedTreeNodes.Count)
                    {
                        MenuItem_AddNode.Visibility = Visibility.Collapsed;
                        MenuItem_RenameNode.Visibility = Visibility.Collapsed;
                        MenuItem_EditNode.Visibility = Visibility.Collapsed;
                        MenuItem_MoveNode.Visibility = Visibility.Collapsed;
                        MenuItem_VisiteNode.Visibility = Visibility.Collapsed;
                        MenuItem_RefreshNode.Visibility = Visibility.Visible;
                    }
                    else if (SelectedTreeNodes.FindAll(p => !p.IsFolder).Count == SelectedTreeNodes.Count)
                    {
                        bool isSameParent = true;
                        string parent = SelectedTreeNodes.FirstOrDefault().ParentKey;
                        foreach (var item in SelectedTreeNodes)
                        {
                            if (item.ParentKey != parent && !item.IsFolder)
                            {
                                isSameParent = false;
                                break;
                            }
                        }
                        if (!isSameParent)
                        {
                            MenuItem_AddNode.Visibility = Visibility.Collapsed;
                            MenuItem_RenameNode.Visibility = Visibility.Collapsed;
                            MenuItem_EditNode.Visibility = Visibility.Collapsed;
                            MenuItem_MoveNode.Visibility = Visibility.Collapsed;
                            MenuItem_VisiteNode.Visibility = Visibility.Collapsed;
                            MenuItem_RefreshNode.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            MenuItem_AddNode.Visibility = Visibility.Collapsed;
                            MenuItem_RenameNode.Visibility = Visibility.Collapsed;
                            MenuItem_EditNode.Visibility = Visibility.Visible;
                            MenuItem_EditNode.IsEnabled = false;
                            MenuItem_MoveNode.Visibility = Visibility.Visible;
                            MenuItem_VisiteNode.Visibility = Visibility.Visible;
                            MenuItem_VisiteNode.IsEnabled = false;
                            MenuItem_RefreshNode.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        MenuItem_AddNode.Visibility = Visibility.Collapsed;
                        MenuItem_RenameNode.Visibility = Visibility.Collapsed;
                        MenuItem_EditNode.Visibility = Visibility.Collapsed;
                        MenuItem_MoveNode.Visibility = Visibility.Collapsed;
                        MenuItem_VisiteNode.Visibility = Visibility.Collapsed;
                        MenuItem_RefreshNode.Visibility = Visibility.Visible;
                    }
                }
                else if (SelectedTreeNodes.Count == 1 && SelectedTreeNodes.FirstOrDefault() == treeViewItem.Header)
                {
                    BookmarkTreeNode currentNode = treeViewItem.Header as BookmarkTreeNode;
                    if (currentNode.IsFolder)
                    {
                        MenuItem_AddNode.Visibility = Visibility.Visible;
                        MenuItem_RenameNode.Visibility = Visibility.Visible;
                        MenuItem_EditNode.Visibility = Visibility.Collapsed;
                        MenuItem_MoveNode.Visibility = Visibility.Collapsed;
                        MenuItem_VisiteNode.Visibility = Visibility.Collapsed;
                        MenuItem_RefreshNode.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MenuItem_AddNode.Visibility = Visibility.Collapsed;
                        MenuItem_RenameNode.Visibility = Visibility.Collapsed;
                        MenuItem_EditNode.Visibility = Visibility.Visible;
                        MenuItem_EditNode.IsEnabled = true;
                        MenuItem_MoveNode.Visibility = Visibility.Visible;
                        MenuItem_VisiteNode.Visibility = Visibility.Visible;
                        MenuItem_VisiteNode.IsEnabled = true;
                        MenuItem_RefreshNode.Visibility = Visibility.Collapsed;
                    }
                }
            }

            treeViewItem.Focus();
            e.Handled = true;
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(!(e.OriginalSource is Border))
            {
                return;
            }

            //var treeViewItem = VisualTreeHelperEx.GetParentObject<TreeListViewItem>(e.OriginalSource as DependencyObject) as TreeListViewItem;
            var treeViewItem = sender as TreeListViewItem;
            if (treeViewItem != null && treeViewItem.IsSelected)
            {
                BookmarkManagerViewModel vm = this.DataContext as BookmarkManagerViewModel;
                BookmarkTreeNode currentNode = treeViewItem.Header as BookmarkTreeNode;
                if (currentNode.IsFolder)
                {
                    vm.RenameBookmark.Execute(currentNode);
                }
                else
                {
                    vm.EditBookmarkCommand.Execute(currentNode);
                }
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                item.Focus();
                e.Handled = false;
            }
            return;
        }

        private void TreeViewItem_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            List<BookmarkTreeNode> SelectedTreeNodes = (List<BookmarkTreeNode>)TreeViewExtensions.GetSelectedItems(host_Bookmarks);
            var treeViewItem = VisualTreeHelperEx.GetParentObject<TreeListViewItem>(e.OriginalSource as DependencyObject) as TreeListViewItem;
            //var treeViewItem = sender as TreeListViewItem;
            if (treeViewItem != null && SelectedTreeNodes != null)
            {
                if (SelectedTreeNodes.Count > 1)
                {
                    if (SelectedTreeNodes.FindAll(p => p.IsFolder).Count == SelectedTreeNodes.Count)
                    {
                        MenuItem_AddNode.Visibility = Visibility.Collapsed;
                        MenuItem_RenameNode.Visibility = Visibility.Collapsed;
                        MenuItem_EditNode.Visibility = Visibility.Collapsed;
                        MenuItem_MoveNode.Visibility = Visibility.Collapsed;
                        MenuItem_VisiteNode.Visibility = Visibility.Collapsed;
                        MenuItem_RefreshNode.Visibility = Visibility.Visible;
                    }
                    else if (SelectedTreeNodes.FindAll(p => !p.IsFolder).Count == SelectedTreeNodes.Count)
                    {
                        bool isSameParent = true;
                        string parent = SelectedTreeNodes.FirstOrDefault().ParentKey;
                        foreach (var item in SelectedTreeNodes)
                        {
                            if (item.ParentKey != parent && !item.IsFolder)
                            {
                                isSameParent = false;
                                break;
                            }
                        }
                        if (!isSameParent)
                        {
                            MenuItem_AddNode.Visibility = Visibility.Collapsed;
                            MenuItem_RenameNode.Visibility = Visibility.Collapsed;
                            MenuItem_EditNode.Visibility = Visibility.Collapsed;
                            MenuItem_MoveNode.Visibility = Visibility.Collapsed;
                            MenuItem_VisiteNode.Visibility = Visibility.Collapsed;
                            MenuItem_RefreshNode.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            MenuItem_AddNode.Visibility = Visibility.Collapsed;
                            MenuItem_RenameNode.Visibility = Visibility.Collapsed;
                            MenuItem_EditNode.Visibility = Visibility.Visible;
                            MenuItem_EditNode.IsEnabled = false;
                            MenuItem_MoveNode.Visibility = Visibility.Visible;
                            MenuItem_VisiteNode.Visibility = Visibility.Visible;
                            MenuItem_VisiteNode.IsEnabled = false;
                            MenuItem_RefreshNode.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        MenuItem_AddNode.Visibility = Visibility.Collapsed;
                        MenuItem_RenameNode.Visibility = Visibility.Collapsed;
                        MenuItem_EditNode.Visibility = Visibility.Collapsed;
                        MenuItem_MoveNode.Visibility = Visibility.Collapsed;
                        MenuItem_VisiteNode.Visibility = Visibility.Collapsed;
                        MenuItem_RefreshNode.Visibility = Visibility.Visible;
                    }
                }
                else if (SelectedTreeNodes.Count == 1 && SelectedTreeNodes.FirstOrDefault() == treeViewItem.Header)
                {
                    BookmarkTreeNode currentNode = treeViewItem.Header as BookmarkTreeNode;
                    if (currentNode.IsFolder)
                    {
                        MenuItem_AddNode.Visibility = Visibility.Visible;
                        MenuItem_RenameNode.Visibility = Visibility.Visible;
                        MenuItem_EditNode.Visibility = Visibility.Collapsed;
                        MenuItem_MoveNode.Visibility = Visibility.Collapsed;
                        MenuItem_VisiteNode.Visibility = Visibility.Collapsed;
                        MenuItem_RefreshNode.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MenuItem_AddNode.Visibility = Visibility.Collapsed;
                        MenuItem_RenameNode.Visibility = Visibility.Collapsed;
                        MenuItem_EditNode.Visibility = Visibility.Visible;
                        MenuItem_EditNode.IsEnabled = true;
                        MenuItem_MoveNode.Visibility = Visibility.Visible;
                        MenuItem_VisiteNode.Visibility = Visibility.Visible;
                        MenuItem_VisiteNode.IsEnabled = true;
                        MenuItem_RefreshNode.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
        private void host_Bookmarks_Selected(object sender, RoutedEventArgs e)
        {
            (e.OriginalSource as TreeViewItem).IsSelected = true;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            host_Bookmarks.Items.CurrentChanged -= Items_CurrentChanged;
            host_Bookmarks.Items.CurrentChanged += Items_CurrentChanged;
        }

        /// <summary>
        /// 书签 TreeView（host_Bookmarks）绑定的数据源被赋值时发生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Items_CurrentChanged(object sender, EventArgs e)
        {
            //if (!busyIndicator.IsBusy)
            {
                if (host_Bookmarks.Items.Count > 0)
                {
                    host_Bookmarks.Visibility = Visibility.Visible;
                    noFile_content.Visibility = Visibility.Collapsed;
                }
                if (host_Bookmarks.Items.Count == 0)
                {
                    host_Bookmarks.Visibility = Visibility.Collapsed;
                    if (noMatchFile_content.Visibility == Visibility.Visible)
                    {
                        noFile_content.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        noFile_content.Visibility = Visibility.Visible;
                    }
                }
            }
        }
        private void busyIndicator_BusyChanged(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender is BusyIndicator)
            {
                if ((sender as BusyIndicator).IsBusy)
                {
                    host_Bookmarks.Visibility = Visibility.Collapsed;
                    noFile_content.Visibility = Visibility.Collapsed;
                }
                else
                {
                    //Items_CurrentChanged(null, null);
                    //if (host_Bookmarks.Items.Count == 0)
                    //{
                    //    host_Bookmarks.Visibility = Visibility.Collapsed;
                    //    noFile_content.Visibility = Visibility.Visible;
                    //}
                    //else
                    //{
                    //    host_Bookmarks.Visibility = Visibility.Visible;
                    //    noFile_content.Visibility = Visibility.Collapsed;
                    //}
                }
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TS.Framework.Core.IoC.Get<BookmarkManagerViewModel>().QueryString = "";
        }

        private void host_Bookmarks_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if (host_Bookmarks.Items != null && host_Bookmarks.Items.Count > 0)
                {
                    foreach (var item in host_Bookmarks.Items)
                    {
                        var temp = item as BookmarkTreeNode;
                        temp.IsSelect = true;

                        if (temp.ChildNode.Count > 0)
                        {
                            SetNodeSelected(temp.ChildNode);
                        }
                    }
                }
                e.Handled = true;
            }
        }

        private void SetNodeSelected(List<BookmarkTreeNode> Items)
        {
            foreach (var item in Items)
            {
                item.IsSelect = true;

                if (item.ChildNode.Count > 0)
                {
                    SetNodeSelected(item.ChildNode);
                }
            }
        }

        private void TreeListViewItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var source = sender as TreeListViewItem;
            if (host_Bookmarks.Items != null && host_Bookmarks.Items.Count > 0)
            {
                FindTheNode(host_Bookmarks.ItemsSource as List<BookmarkTreeNode>, (source.Header as BookmarkTreeNode).NodeKey).IsSelect = true;
            }
        }


        //寻找指定节点，用于删除、添加、更改等
        private BookmarkTreeNode FindTheNode(List<BookmarkTreeNode> nodeList, string nodeId)
        {
            BookmarkTreeNode findedNode = null;
            foreach (BookmarkTreeNode node in nodeList)
            {
                if (node.ChildNode != null && node.ChildNode.Count > 0)
                {
                    if ((findedNode = FindTheNode(node.ChildNode, nodeId)) != null)
                    {
                        return findedNode;
                    }
                }
                if (node.NodeKey == nodeId)
                {
                    return node;
                }
            }
            return findedNode;
        }

        /// <summary>
        /// 框选（Test）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PART_ScrollContentPresenter_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }
            if (sender is TreeListView == false)
                return;
            TreeListView scroll = sender as TreeListView;
            // 避免在竖直滚动条因框选影响不起作用
            ScrollBar sb = VisualTreeHelperEx.GetChildObject<ScrollBar>(scroll, "PART_VerticalScrollBar");
            if (sb != null && sb.IsMouseCaptureWithin)
            {
                return;
            }
            Select_Area.Visibility = Visibility.Hidden;
            ///判断如果是单击了item之外的地方 要取消所有勾选
            bool result = false;
            var hitTestParams = new PointHitTestParameters(e.GetPosition(scroll));
            var resultCallback = new HitTestResultCallback(x => HitTestResultBehavior.Stop);
            var filterCallback = new HitTestFilterCallback(x =>
            {
                if (x is TreeListViewItem)
                {
                    if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                    {
                        if ((x as TreeListViewItem).HasItems)
                        {
                            return HitTestFilterBehavior.ContinueSkipSelf;
                        }
                        TreeViewExtensions.SetIsSelected(x, true);
                    }
                }

                if (x is GridViewRowPresenter)
                {
                    result = true;
                    return HitTestFilterBehavior.Stop;
                }
                
                return HitTestFilterBehavior.Continue;
            });

            VisualTreeHelper.HitTest(scroll, filterCallback, resultCallback, hitTestParams);
            if (result == false && Start_Point.Equals(new Point(0, 0)))
            {
                UnSelectAll();
            }

            Start_Point = new Point(0, 0);
            scroll.ReleaseMouseCapture();
        }

        /// <summary>
        /// 框选（Test）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PART_ScrollContentPresenter_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            if (sender is TreeListView == false)
                return;
            TreeListView scroll = sender as TreeListView;
            // 避免在竖直滚动条因框选影响不起作用
            ScrollBar sb = VisualTreeHelperEx.GetChildObject<ScrollBar>(scroll, "PART_VerticalScrollBar");
            if (sb != null && sb.IsMouseCaptureWithin)
            {
                return;
            }
            Point now_Point = e.GetPosition(scroll);

            System.Diagnostics.Debug.WriteLine(now_Point);

            if (Start_Point.Equals(new Point(0, 0)))
            {
                Start_Point = e.GetPosition(scroll);
                Select_Area.Visibility = Visibility.Visible;
                Select_Area.Width = 0;
                Select_Area.Height = 0;
                Select_Area.Margin = new Thickness(Start_Point.X, Start_Point.Y, 0, 0);
                scroll.CaptureMouse();
            }
            double min_X = now_Point.X < Start_Point.X ? now_Point.X : Start_Point.X;
            double max_X = now_Point.X > Start_Point.X ? now_Point.X : Start_Point.X;
            double min_Y = now_Point.Y < Start_Point.Y ? now_Point.Y : Start_Point.Y;
            double max_Y = now_Point.Y > Start_Point.Y ? now_Point.Y : Start_Point.Y;
            Select_Area.Width = max_X - min_X;
            Select_Area.Height = max_Y - min_Y;
            Select_Area.Margin = new Thickness(min_X, min_Y, 0, 0);
            if (Select_Area.Width * Select_Area.Height > 30 && System.Windows.Forms.Control.ModifierKeys != System.Windows.Forms.Keys.Control
                && System.Windows.Forms.Control.ModifierKeys != System.Windows.Forms.Keys.Shift)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Select_Area.Width = {0}, Select_Area.Height = {1}", Select_Area.Width, Select_Area.Height));
                UnSelectAll();
            }

            /***************************************************************************************************
             * 上面的判断是每次MouseMove事件都要进入的。每次选中的集合 都是先清空，然后重新选择出来的
             * 所以魔数30的意义不大，可以为10、20、100等等
             * 
             * **********************************************************
             * 下面的点击测试只会在最后一次才会完全定下TreeViewItem的选中集合。
             * 但是MouseMove期间需要展示选中效果，所以就必须进行点击测试
             ***************************************************************************************************/
            {
                var rect = new RectangleGeometry(new Rect(min_X, min_Y, Select_Area.Width, Select_Area.Height));
                var hitTestParams = new GeometryHitTestParameters(rect);
                var resultCallback = new HitTestResultCallback(x => HitTestResultBehavior.Continue);
                var filterCallback = new HitTestFilterCallback(x =>
                {
                    if (x is TreeListViewItem)
                    {
                        //(x as TreeListViewItem).IsSelected = true;
                        if((x as TreeListViewItem).HasItems)
                        {
                            return HitTestFilterBehavior.ContinueSkipSelf;
                        }
                        TreeViewExtensions.SetIsSelected(x, true);
                    }

                    return HitTestFilterBehavior.Continue;
                });
                VisualTreeHelper.HitTest(scroll, filterCallback, resultCallback, hitTestParams);
            }
        }

        private void UnSelectAll()
        {
            var items = GetExpandedTreeViewItems(host_Bookmarks);

            // 改进版
            foreach (var item in items)
            {
                if (!TreeViewExtensions.GetIsSelected(item))
                {
                    continue;
                }

                System.Diagnostics.Debug.WriteLine((item.Header as BookmarkTreeNode).Name);
                TreeViewExtensions.SetIsSelected(item, false);
            }

            // 原版
            //foreach (var item in items)
            //{
            //    System.Diagnostics.Debug.WriteLine((item.Header as BookmarkTreeNode).Name);
            //    TreeViewExtensions.SetIsSelected(item, false);
            //}
        }

        private IEnumerable<TreeListViewItem> GetExpandedTreeViewItems(ItemsControl tree)
        {
            foreach (var item in tree.Items)
            {
                var tvi = (TreeListViewItem)tree.ItemContainerGenerator.ContainerFromItem(item);
                if (tvi == null)
                {
                    continue;
                }
                yield return tvi;
                if (tvi.IsExpanded)
                {
                    foreach (var subItem in GetExpandedTreeViewItems(tvi))
                    {
                        yield return subItem;
                    }
                }
            }
        }
    }

    public class BookmarkIconTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null)
            {
                if (container != null && container is ContentPresenter)
                {
                    var connectionContent = VisualTreeHelperEx.GetParentObject<ContentControl>(container, "PART_BookmarkIcon");

                    if ((item as BookmarkTreeNode).IsFolder)
                    {
                        return (DataTemplate)connectionContent.TryFindResource("bookmarkFolder");
                    }
                    else
                    {
                        return (DataTemplate)connectionContent.TryFindResource("bookmark");
                    }
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}
