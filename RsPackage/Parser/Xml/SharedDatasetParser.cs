using RsPackage.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.IO;

namespace RsPackage.Parser.Xml
{
    public class SharedDatasetParser : IParser, IParserPathable
    {
        private SharedDatasetService sharedDatasetService;
        private IEnumerable<IParser> ChildrenParsers;

        public ProjectParser Root { get; set; }
        public string RootPath { get; set; }
        public IParser Parent { get; set; }
        public string ParentPath { get; set; }

        public SharedDatasetParser(SharedDatasetService sharedDatasetService)
        {
            this.sharedDatasetService = sharedDatasetService;
            ChildrenParsers = new List<IParser>();
        }

        public virtual void Execute(XmlNode node)
        {
            var sharedDatasetNodes = node.SelectNodes("./Dataset | ./SharedDataset");
            foreach (XmlNode sharedDatasetNode in sharedDatasetNodes)
            {
                var name = sharedDatasetNode.Attributes["Name"].Value;

                var path = sharedDatasetNode.SelectSingleNode("./Path")?.InnerXml;
                path = path ?? $"{Root.NamingConvention.Apply(name)}.rsd";
                if (!Path.IsPathRooted(path))
                    path = Path.Combine(RootPath ?? string.Empty, path);

                var description = sharedDatasetNode.SelectSingleNode("./Description")?.InnerXml;

                var hidden = bool.Parse(sharedDatasetNode.Attributes["Hidden"]?.Value ?? bool.FalseString);
                var dataSourceName = sharedDatasetNode.Attributes["DataSource"]?.Value ?? string.Empty;

                var overwrite = bool.Parse(sharedDatasetNode.Attributes["Overwrite"]?.Value ?? bool.TrueString);


                sharedDatasetService.Create(name, ParentPath, path, description, hidden, dataSourceName, Root?.DataSources);
                Root.SharedDatasets.Add(name, $"{ParentPath}/{name}");
            }
        }

    }
}
