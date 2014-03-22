using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Localization;
using System.Text.RegularExpressions;

namespace Orchard.Projections.Providers.Layouts {
    public class LayoutShapes : IDependency {
        public LayoutShapes() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [Shape]
        public void Grid(dynamic Display, TextWriter Output, HtmlHelper Html, string Id, bool Horizontal, IEnumerable<dynamic> Items, int Columns, string Tag, IEnumerable<string> Classes, IDictionary<string, string> Attributes, string RowTag, IEnumerable<string> RowClasses, IDictionary<string, string> RowAttributes, string CellTag, IEnumerable<string> CellClasses, IDictionary<string, string> CellAttributes, string EmptyCell) {
            if (Items == null)
                return;

            var items = Items.ToList();
            var itemsCount = items.Count;

            if (itemsCount < 1)
                return;

            var gridTag = String.IsNullOrEmpty(Tag) ? null : GetTagBuilder(Tag, Id, Classes, Attributes);
            var rowTag = String.IsNullOrEmpty(RowTag) ? null : GetTagBuilder(RowTag, string.Empty, RowClasses, RowAttributes);
            var cellTag = String.IsNullOrEmpty(CellTag) ? null : GetTagBuilder(CellTag, string.Empty, CellClasses, CellAttributes);

            if (gridTag != null) {
                Output.Write(gridTag.ToString(TagRenderMode.StartTag));
            }

            // resolves which item to display in a specific cell
            Func<int, int, int> seekItem = (row, col) => row*Columns + col;
            int maxRows = (itemsCount - 1) / Columns + 1;
            int maxCols = Columns;
            
            if (!Horizontal) {
                seekItem = (row, col) => col*Columns + row;
                maxCols = maxRows;
                maxRows = Columns;
            }

            for(int row=0; row < maxRows; row++) {

                if (rowTag != null) {
                    Output.Write(rowTag.ToString(TagRenderMode.StartTag));
                }

                for (int col = 0; col < maxCols; col++) {
                    int index = seekItem(row, col);

                    if (cellTag != null) {
                        Output.Write(cellTag.ToString(TagRenderMode.StartTag));
                    }

                    if (index < itemsCount) {
                        Output.Write(Display(items[index]));
                    }
                    else {
                        Output.Write(EmptyCell);
                    }

                    if (cellTag != null) {
                        Output.Write(cellTag.ToString(TagRenderMode.EndTag));
                    }
                }

                if (rowTag != null) {
                    Output.Write(rowTag.ToString(TagRenderMode.EndTag));
                }
            }

            if (gridTag != null) {
                Output.Write(gridTag.ToString(TagRenderMode.EndTag));
            }

        }

        [Shape]
        public void Carousel(dynamic Display, TextWriter Output, HtmlHelper Html, string Id, IEnumerable<dynamic> Items,
                            IEnumerable<string> OuterClasses, IDictionary<string, string> OuterAttributes,
                            IEnumerable<string> InnerClasses, IDictionary<string, string> InnerAttributes,
                            IEnumerable<string> FirstItemClasses, IDictionary<string, string> FirstItemAttributes, IEnumerable<string> ItemClasses, IDictionary<string, string> ItemAttributes)
        {
            if (Items == null) return;

            var items = Items.ToList();
            var itemsCount = items.Count;

            if (itemsCount < 1) return;

            var outerDivTag = GetTagBuilder("div", Id, OuterClasses, OuterAttributes);
            var innerDivTag = GetTagBuilder("div", string.Empty, InnerClasses, InnerAttributes);
            var firstItemTag = GetTagBuilder("div", string.Empty, FirstItemClasses, FirstItemAttributes);
            var itemTag = GetTagBuilder("div", string.Empty, ItemClasses, ItemAttributes);

            Output.Write(outerDivTag.ToString(TagRenderMode.StartTag));
            Output.Write(innerDivTag.ToString(TagRenderMode.StartTag));

            int i = 0;

            foreach (var item in items)
            {
                if (i == 0)
                    Output.Write(firstItemTag.ToString(TagRenderMode.StartTag));
                else
                    Output.Write(itemTag.ToString(TagRenderMode.StartTag));

                string tmpImageLink = Convert.ToString(Display(item));
                tmpImageLink = getImgString(tmpImageLink);

                Output.Write("<img src=\"{0}\"/>", tmpImageLink);
                Output.Write("<div class=\"carousel-caption\"><h4>It's a Test for Carousel......</h4><h4>And it looks great!</h4></div>");
                Output.Write(itemTag.ToString(TagRenderMode.EndTag));
                i++;
            }

            Output.Write(innerDivTag.ToString(TagRenderMode.EndTag));

            Output.Write("<a href=\"#{0}\" class=\"carousel-control left\" data-slide=\"prev\">&lsaquo;</a>", Id);
            Output.Write("<a href=\"#{0}\" class=\"carousel-control right\" data-slide=\"next\">&rsaquo;</a>", Id);

            Output.Write(outerDivTag.ToString(TagRenderMode.EndTag));

            Output.Write("<script>$(function () {$('" + Id + "').carousel();}); </script>");
            string temp = Output.ToString();
        }

        private string getSubstringByString(string head, string tail, string original)
        {
            original = original.Substring(original.IndexOf(head));
            original = original.Remove(original.IndexOf("</span>"));
            original = original.Substring(original.IndexOf("/") + 1);
            return original;
        }

        private string getImgString(string htmlSource)
        {
            List<string> links = new List<string>();
            string regexImgSrc = @"<img[^>]*?src\s*=\s*[""']?([^'"" >]+?)[ '""][^>]*?>";
            MatchCollection matchesImgSrc = Regex.Matches(htmlSource, regexImgSrc, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match m in matchesImgSrc)
            {
                string href = m.Groups[1].Value;
                links.Add(href);
            }
            return links.First();
        }

        static TagBuilder GetTagBuilder(string tagName, string id, IEnumerable<string> classes, IDictionary<string, string> attributes) {
            var tagBuilder = new TagBuilder(tagName);
            tagBuilder.MergeAttributes(attributes, false);
            foreach (var cssClass in classes ?? Enumerable.Empty<string>())
                tagBuilder.AddCssClass(cssClass);
            if (!string.IsNullOrWhiteSpace(id))
                tagBuilder.GenerateId(id);
            return tagBuilder;
        }

    }
}
