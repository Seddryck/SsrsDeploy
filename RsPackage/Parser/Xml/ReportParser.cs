using RsPackage.Execution;
using RsPackage.Parser.NamingConventions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace RsPackage.Parser.Xml
{
    public class ReportParser : IParser, IParserPathable
    {
        private ReportService reportService;
        private IEnumerable<IParser> ChildrenParsers;

        public ProjectParser Root { get; set; }

        public INamingConvention NamingConvention
        {
            get
            {
                return (Root?.NamingConvention) ?? new TitleToCamelCase();
            }
        }

        public string RootPath { get; set; }
        public IParser Parent { get; set; }
        public string ParentPath { get; set; }

        public ReportParser(ReportService reportService)
        {
            this.reportService = reportService;
            ChildrenParsers = new List<IParser>();
        }

        public ReportParser(ReportService reportService, IEnumerable<IParser> children)
        {
            this.reportService = reportService;
            ChildrenParsers = children.ToList();
        }

        public virtual void Execute(XmlNode node)
        {
            var reportNodes = node.SelectNodes("./Report");
            foreach (XmlNode reportNode in reportNodes)
            {
                var name = reportNode.Attributes["Name"].Value;

                var path = reportNode.SelectSingleNode("./Path")?.InnerXml;
                path = path ?? $"{NamingConvention.Apply(name)}.rdl";
                if (!Path.IsPathRooted(path))
                    path = Path.Combine(RootPath ?? string.Empty, path);

                var description = reportNode.SelectSingleNode("./Description")?.InnerXml;
                var hidden = bool.Parse(reportNode.Attributes["Hidden"]?.Value ?? bool.FalseString);

                var dataSetNodes = reportNode.SelectNodes("./DateSetMap/DataSet");

                var localDateSetMap = new Dictionary<string, string>();
                foreach (XmlNode dataSetNode in dataSetNodes)
                {
                    localDateSetMap.Add(dataSetNode.Attributes["Name"].Value, dataSetNode.Attributes["Reference"].Value);
                }

                reportService.Create(name, ParentPath, path, description, hidden, Root?.DataSources, Root?.SharedDatasets, localDateSetMap);

                foreach (var childParser in ChildrenParsers)
                {
                    childParser.ParentPath = $"{ParentPath}/{name}";
                    childParser.Execute(reportNode);
                }

            }
        }
    }
}
