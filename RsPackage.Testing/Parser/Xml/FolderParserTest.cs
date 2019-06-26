using NUnit.Framework;
using Moq;
using RsPackage.Execution;
using RsPackage.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RsPackage.Parser.Xml;
using System.IO;
using System.Reflection;
using System.Xml;

namespace RsPackage.Testing.Parser.Xml
{
    [TestFixture]
    public class FolderParserTest
    {
        [Test]
        public void ParseFolderNode()
        {
            var mock = new Mock<FolderService>();
            mock.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            var service = mock.Object;

            var parser = new FolderParser(service);

            var xmlDoc = new XmlDocument();
            using (Stream stream = Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("RsPackage.Testing.Resources.BasicSample.xml"))
            using (StreamReader reader = new StreamReader(stream))
                xmlDoc.Load(reader);

            var root = xmlDoc.FirstChild.NextSibling;
            parser.Execute(root);

            Mock.Get(service).Verify(s => s.Create(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void ParseMultipleFolderNode()
        {
            var mock = new Mock<FolderService>();
            mock.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            var service = mock.Object;

            var parser = new FolderParser(service);

            var xmlDoc = new XmlDocument();
            using (Stream stream = Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("RsPackage.Testing.Resources.MultiLevelSample.xml"))
            using (StreamReader reader = new StreamReader(stream))
                xmlDoc.Load(reader);

            var root = xmlDoc.FirstChild.NextSibling;
            parser.Execute(root);

            Mock.Get(service).Verify(s => s.Create(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(4));
        }

        [Test]
        public void ParseFoldersWhenMultipleFolderNodeAndCreateThem()
        {
            var mockFolderService = new Mock<FolderService>();
            mockFolderService.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            var folderService = mockFolderService.Object;

            var stubReportService = new Mock<ReportService>();
            stubReportService.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<Dictionary<string, string>>(), null)).Verifiable();
            var reportService = stubReportService.Object;

            var stubReportParser = new Mock<ReportParser>(reportService);
            stubReportParser.Setup(s => s.Execute(It.IsAny<XmlNode>())).Verifiable();
            var childParser = (IParser)stubReportParser.Object;

            var folderParser = new FolderParser(folderService, Enumerable.Repeat(childParser, 1));

            var xmlDoc = new XmlDocument();
            using (Stream stream = Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("RsPackage.Testing.Resources.MultiLevelSample.xml"))
            using (StreamReader reader = new StreamReader(stream))
                xmlDoc.Load(reader);

            var root = xmlDoc.FirstChild.NextSibling;
            folderParser.Execute(root);

            Mock.Get(folderService).Verify(s => s.Create( "Analysis", null));
            Mock.Get(folderService).Verify(s => s.Create( "Low-level", null));
            Mock.Get(folderService).Verify(s => s.Create( "Realtime", "/Analysis"));
            Mock.Get(folderService).Verify(s => s.Create( "Confidential", "/Analysis/Realtime"));
        }

        [Test]
        public void ParseFolderWithPolicy()
        {
            var stubFolderService = new Mock<FolderService>();
            stubFolderService.Setup(s => s.Create(It.IsAny<string>(), It.IsAny<string>()));
            var folderService = stubFolderService.Object;

            var mockPolicyParser = new Mock<PolicyParser>();
            mockPolicyParser.Setup(p => p.Execute(It.IsAny<XmlNode>())).Verifiable();
            var policyParser = mockPolicyParser.Object;

            var parser = new FolderParser(folderService, new[] { policyParser });

            var xmlDoc = new XmlDocument();
            using (Stream stream = Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("RsPackage.Testing.Resources.MultiLevelSample.xml"))
            using (StreamReader reader = new StreamReader(stream))
                xmlDoc.Load(reader);

            var root = xmlDoc.FirstChild.NextSibling.SelectSingleNode("./Folder[@Name='Analysis']");
            parser.Execute(root);

            Mock.Get(policyParser).Verify(p => p.Execute(It.IsAny<XmlNode>()), Times.Exactly(2));
        }

    }
}
