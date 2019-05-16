public class HTMLPaserHelper
    {
        private FileStream SourceStream;
        private HtmlNodeCollection HtmlNodeCollection
        {
            get;
            set;
        }

        public HTMLPaserHelper(FileStream fs)
        {
            SourceStream = fs;
        }

        public List<BookmarkTreeNode> HTMLPaser(string XPathExpression, string rootKey)
        {
            Dictionary<string, string> desDictionary = new Dictionary<string, string>();
            
            HtmlDocument _doc = new HtmlDocument();
            _doc.Load(SourceStream, Encoding.UTF8);

            HtmlNodeCollection hrefs = _doc.DocumentNode.SelectNodes(XPathExpression);

            return FindElements(hrefs, rootKey);
        }

        private List<BookmarkTreeNode> FindElements(HtmlNodeCollection hc, string nodekey)
        {
            if (hc == null)
            {
                return null;
            }

            List<BookmarkTreeNode> desNodeDictionary = new List<BookmarkTreeNode>();
            foreach (HtmlNode href in hc.Elements())
            {
                if (string.Equals(href.Name, "DT", StringComparison.CurrentCultureIgnoreCase))
                {
                    foreach (var item in href.ChildNodes)
                    {
                        BookmarkTreeNode root = new BookmarkTreeNode();
                        if (string.Equals(item.Name, "A", StringComparison.CurrentCultureIgnoreCase))
                        {
                            root.Name = item.InnerHtml;
                            root.Uri = item.Attributes["href"].Value;
                            root.NodeKey = "";
                            root.ParentKey = nodekey;
                            root.IsFolder = false;
                            root.Position = 0;
                        }

                        if (string.Equals(item.Name, "H3", StringComparison.CurrentCultureIgnoreCase))
                        {
                            root.NodeKey = Guid.NewGuid().ToString();
                            root.ParentKey = nodekey;
                            root.IsFolder = true;
                            root.Position = 0;
                            root.Name = item.InnerText;
                            root.Uri = null;
                            root.ChildNode = FindElements(href.SelectNodes("./dl"), root.NodeKey);
                        }

                        if (root.Name != null)
                        {
                            desNodeDictionary.Add(root);
                        }

                    }
                }
            }
            return desNodeDictionary;
        }
    }
