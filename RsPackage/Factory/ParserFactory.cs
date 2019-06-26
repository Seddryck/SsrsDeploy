using RsPackage.Parser;
using RsPackage.Parser.NamingConventions;
using RsPackage.Parser.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RsPackage.Factory
{
    class ParserFactory
    {
        public ProjectParser GetXmlParser(Options options)
        {
            var serviceBuilder = new ServiceBuilder();
            serviceBuilder.Setup(options);
            serviceBuilder.Build();

            var rootPath = GetRootPath(options);
            var parentFolder = GetParentFolder(options);
            var namingConvention = GetNamingConvention(options);


            var parser = new ProjectParser()
            {
                ParentFolder = parentFolder,
                RootPath = rootPath,
                NamingConvention = namingConvention
            };

            var policyParser = new PolicyParser(serviceBuilder.GetPolicyService());
            var dataSourceParser = new DataSourceParser(serviceBuilder.GetDataSourceService());
            var dataSetParser = new SharedDatasetParser(serviceBuilder.GetDataSetService());
            var reportParser = new ReportParser(serviceBuilder.GetReportService(), new[] { policyParser });
            var folderParser = new FolderParser(serviceBuilder.GetFolderService(), new IParser[] { policyParser, dataSourceParser, reportParser, dataSetParser });

            parser.ChildParsers.Add(dataSourceParser);
            parser.ChildParsers.Add(dataSetParser);
            parser.ChildParsers.Add(reportParser);
            parser.ChildParsers.Add(folderParser);

            return parser;
        }

        protected virtual string GetRootPath(Options options)
        {
            var rootPath = options.ResourcePath ?? Path.GetDirectoryName(options.SourceFile);
            rootPath = rootPath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? rootPath : rootPath + Path.DirectorySeparatorChar;
            return rootPath;
        }

        protected virtual string GetParentFolder(Options options)
        {
            return options.ParentFolder;
        }

        protected virtual INamingConvention GetNamingConvention(Options options)
        {
            return new TitleToUnderscoreWord();
        }
    }
}
