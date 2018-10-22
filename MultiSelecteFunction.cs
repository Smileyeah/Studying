NameSpace Piao
{
    public Class Beifang
    {
        /// <summary>
        /// 多项选中
        /// </summary>
        /// 还有好多BUG
        
        private void MultiSelected(TreeListViewItem treeViewItem)
        {
            List<NewBookmarkTreeNode> treeViewSource = this.UIItemSource;

            int aboveIndexOfShared = 0;
            int underIndexOfShared = 0;

            do
            {
                if (null == this.Anchor)
                {
                    int d = treeViewItem.Level;
                    TreeListViewItem obj = (0 == d) ? treeViewItem : this.FindAncestor(treeViewItem, d);

                    aboveIndexOfShared = 0;
                    underIndexOfShared = treeViewSource.IndexOf(obj.DataContext as NewBookmarkTreeNode);
                    break;
                }

                int anchorLevel = this.Anchor.Level;
                int senderLevel = treeViewItem.Level;

                //if(anchorLevel == 0 && senderLevel == 0)
                //{
                //    aboveIndexOfShared = treeViewSource.IndexOf(this.Anchor.Header as NewBookmarkTreeNode);
                //    underIndexOfShared = treeViewSource.IndexOf(treeViewItem.Header as NewBookmarkTreeNode);

                //    for (int k = Math.Min(aboveIndexOfShared, underIndexOfShared); k <= Math.Max(aboveIndexOfShared, underIndexOfShared); ++k)
                //    {
                //        this.Selected(treeViewSource[k]);
                //    }

                //    break;
                //}

                var highLevelItem = Math.Min(anchorLevel, senderLevel) == senderLevel ? this.Anchor : treeViewItem;
                var lowLevelItem = Math.Min(anchorLevel, senderLevel) != senderLevel ? this.Anchor : treeViewItem;

                var ancestor = this.FindAncestor(highLevelItem, Math.Abs(senderLevel - anchorLevel));

                int _level = 0;
                for ( ; _level <= Math.Min(anchorLevel, senderLevel); )
                {
                    if (lowLevelItem == ancestor)
                    {
                        treeViewSource = (ancestor.Header as NewBookmarkTreeNode).ChildNode;
                        break;
                    }

                    var tempAncestor = FindAncestor(ancestor, ++_level);
                    if (tempAncestor != null)
                    {
                        if (tempAncestor == FindAncestor(lowLevelItem, _level))
                        {
                            treeViewSource = (tempAncestor.Header as NewBookmarkTreeNode).ChildNode;
                            break;
                        }
                    }
                }


                aboveIndexOfShared = treeViewSource.IndexOf(FindAncestor(ancestor, _level - 1).Header as NewBookmarkTreeNode);
                underIndexOfShared = treeViewSource.IndexOf(FindAncestor(lowLevelItem, _level - 1).Header as NewBookmarkTreeNode);

                Action<int, TreeListViewItem> selecteAction = null;
                selecteAction = (index, treeItem) =>
                {
                    if (index >= _level - 1 || _level < 0)
                    {
                        if (index == 0)
                        {
                            this.Selected(treeItem.Header as NewBookmarkTreeNode);
                        }

                        return;
                    }

                    var parent = FindAncestor(treeItem, 1);

                    var pHeader = parent.Header as NewBookmarkTreeNode;

                    int m = pHeader.ChildNode.IndexOf(treeItem.Header as NewBookmarkTreeNode);
                    for (; m < pHeader.ChildNode.Count; m++)
                    {
                        this.Selected(pHeader.ChildNode[m]);
                    }

                    selecteAction(index + 1, parent);
                };
                
                for (int k = Math.Min(aboveIndexOfShared, underIndexOfShared); k < Math.Max(aboveIndexOfShared, underIndexOfShared); ++k)
                {
                    this.Selected(treeViewSource[k + 1]);
                }

                if(aboveIndexOfShared == -1 && underIndexOfShared == -1)
                {
                    selecteAction(0, lowLevelItem);
                }
                else
                {
                    selecteAction(0, Math.Min(aboveIndexOfShared, underIndexOfShared) == aboveIndexOfShared ? highLevelItem : lowLevelItem);
                }
                
            }
            while (false);
        }
    }
}
