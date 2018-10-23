NameSpace Piao
{
    public Class Beifang
    {
        /// <summary>
        /// 多项选中
        /// 父层——————子层1 ——————孙层1
        ///    |         |————————孙层2
        ///     ——————子层2 ——————孙层3
        ///              |————————孙层4
        /// 选择时如果跨越了父节点，该父节点之下的节点都会被选中。
        /// 选择的思想是：1、当孙层2 和孙层4 被选中：向上寻父，直至父节点为同一节点（父层）。
        ///              2、在父层的子节点中获取孙层2 和孙层4 的索引。选中孙层2 和孙层4 索引之间的节点，包括孙层4，但不包括孙层2。
        ///              3、FindLevel = 孙层2层级（选择节点）和父层层级（共同节点）之差 - 1。
        ///              4、选择孙层2（选择节点）在父节点中索引大于孙层2的所有节点，不包括孙层2。此步骤递归进行。
        ///              5、在4步骤完结时，选中孙层2（选择节点）。
        /// ***************************************应该还有BUG****************************************
        /// </summary>
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

                if (lowLevelItem == ancestor)
                {
                    this.Selected(lowLevelItem.Header as NewBookmarkTreeNode);
                    break;
                }

                int _level = 0;
                TreeListViewItem tempAncestor = null;
                for ( ; _level < Math.Min(anchorLevel, senderLevel); )
                {
                    tempAncestor = FindAncestor(ancestor, ++_level);
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

                var findLevel = tempAncestor == null ? -1 : tempAncestor.Level; // 用于存储向上寻根的等级
                var selecteItem = Math.Min(aboveIndexOfShared, underIndexOfShared) == aboveIndexOfShared ? highLevelItem : lowLevelItem;

                Action<int, TreeListViewItem> selecteAction = null;
                selecteAction = (index, treeItem) =>
                {
                    if (index >= selecteItem.Level - findLevel - 1)
                    {
                        this.Selected(selecteItem.Header as NewBookmarkTreeNode);

                        return;
                    }

                    var parent = FindAncestor(treeItem, 1);

                    var pHeader = parent.Header as NewBookmarkTreeNode;

                    int m = pHeader.ChildNode.IndexOf(treeItem.Header as NewBookmarkTreeNode);
                    for (; m < pHeader.ChildNode.Count - 1; m++)
                    {
                        this.Selected(pHeader.ChildNode[m + 1]);
                    }

                    selecteAction(index + 1, parent);
                };
                
                for (int k = Math.Min(aboveIndexOfShared, underIndexOfShared); k < Math.Max(aboveIndexOfShared, underIndexOfShared); ++k)
                {
                    this.Selected(treeViewSource[k + 1]);
                }

                selecteAction(0, selecteItem);
                
            }
            while (false);
        }
    }
}
