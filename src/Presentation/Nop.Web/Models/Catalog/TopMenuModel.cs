using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Catalog
{
    public partial record TopMenuModel : BaseNopModel
    {
        public TopMenuModel()
        {
            Categories = new List<CategorySimpleModel>();
            Topics = new List<TopicModel>();
        }

        public IList<CategorySimpleModel> Categories { get; set; }
        public IList<TopicModel> Topics { get; set; }

        public bool BlogEnabled { get; set; }
        public bool NewProductsEnabled { get; set; }
        public bool ForumEnabled { get; set; }

        public bool DisplayHomepageMenuItem { get; set; }
        public bool DisplayNewProductsMenuItem { get; set; }
        public bool DisplayProductSearchMenuItem { get; set; }
        public bool DisplayCustomerInfoMenuItem { get; set; }
        public bool DisplayBlogMenuItem { get; set; }
        public bool DisplayForumsMenuItem { get; set; }
        public bool DisplayContactUsMenuItem { get; set; }

        public bool UseAjaxMenu { get; set; }

        public bool HasOnlyCategories => Categories.Any()
                       && !Topics.Any()
                       && !DisplayHomepageMenuItem
                       && !(DisplayNewProductsMenuItem && NewProductsEnabled)
                       && !DisplayProductSearchMenuItem
                       && !DisplayCustomerInfoMenuItem
                       && !(DisplayBlogMenuItem && BlogEnabled)
                       && !(DisplayForumsMenuItem && ForumEnabled)
                       && !DisplayContactUsMenuItem;

        public IEnumerable<TopicNodeModel> GetTopicHierarchy()
        {
            //var topics = new[]
            //{
            //    "Foo // Bar",
            //    "Foo // Baz",
            //    "Bar",
            //    "Bar // Foo // Bar // Baz",
            //    "Bar // Foo // Bar // Foo",
            //    "Bar // Foo // Baz // Bar",
            //}.Select(t => new TopicModel
            //{
            //    Id = Topics[0].Id,
            //    Name = t,
            //    SeName = Topics[0].SeName,
            //});

            var root = new TopicTreeBuilder("root");
            foreach (var t in Topics)
                root.Add(t);

            var node = root.Build();
            return node.Children;
        }

        #region Nested classes

        public record TopicModel : BaseNopEntityModel
        {
            public string Name { get; set; }
            public string SeName { get; set; }
        }

        public record CategoryLineModel : BaseNopModel
        {
            public int Level { get; set; }
            public bool ResponsiveMobileMenu { get; set; }
            public CategorySimpleModel Category { get; set; }
        }

        public record TopicNodeModel
        {
            public string Name { get; set; }
            public TopicModel Topic { get; set; }
            public ICollection<TopicNodeModel> Children { get; set; } = Array.Empty<TopicNodeModel>();
        }

        private class TopicTreeBuilder
        {
            private readonly IDictionary<string, TopicTreeBuilder> _children = new Dictionary<string, TopicTreeBuilder>(StringComparer.CurrentCultureIgnoreCase);
            private readonly string _name;
            private TopicModel _model;

            public TopicTreeBuilder(string name)
            {
                _name = name;
            }

            public void Add(TopicModel model, string[] localPath = null)
            {
                localPath ??= model.Name.Split("//")
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToArray();

                if (localPath.Length == 0)
                {
                    _model = model;
                }
                else
                {
                    var first = localPath[0];
                    if (!_children.TryGetValue(first, out var ch))
                        _children[first] = ch = new TopicTreeBuilder(first);

                    ch.Add(model, localPath.Skip(1).ToArray());
                }
            }

            public TopicNodeModel Build() => new TopicNodeModel
            {
                Name = _name,
                Topic = _model,
                Children = _children.Select(c => c.Value.Build()).ToList(),
            };
        }

        #endregion
    }
}