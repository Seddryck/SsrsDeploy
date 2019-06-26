using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using RsPackage.ReportingService;
using RsPackage.Transform;
using System.Xml;

namespace RsPackage.Execution
{
    public class DataSourceService : BaseService
    {
        public DataSourceService()
        { }

        public DataSourceService(ReportingService2010 ReportingService)
            : base(ReportingService)
        { }

        public virtual void Create(string name, string parent, string path, bool overwrite)
        {
            //If file extension is not specied we need to check the existence of both
            if (Path.GetExtension(path) != ".rsds" && Path.GetExtension(path) != ".rds")
            {
                if (File.Exists($"{path}.rsds"))
                    path = $"{path}.rsds";
                else
                    path = $"{path}.rds";
            }

            //If file is not found then we must throw an error
            if (!File.Exists(path))
            {
                OnError($"File '{path}' doesn't exist!");
                return;
            }

            // If file is a Visual Studio connection-string then we need to transform it
            if (Path.GetExtension(path) == ".rds")
            {
                var document = new XmlDocument();
                document.Load(path);

                var transformer = new DataSourceTransformer();
                if (transformer.IsVisualStudio(document))
                {
                    var newDocument = transformer.ToReportingService(document);
                    path = Path.ChangeExtension(path, ".rsds");
                    newDocument.Save(path);
                }
                else
                    throw new InvalidOperationException();
            }

            //If it's a rsds file we need to ensure it's a valid Reporting Server connection string
            if (Path.GetExtension(path) == ".rsds")
            {
                var document = new XmlDocument();
                document.Load(path);

                var transformer = new DataSourceTransformer();
                if (!transformer.IsReportingService(document))
                    throw new InvalidOperationException();
            }

            Byte[] definition = null;
            try
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    definition = new Byte[stream.Length];
                    stream.Read(definition, 0, (int)stream.Length);
                }
            }
            catch (IOException e)
            {
                OnInformation(e.Message);
            }

            Warning[] warnings = null;
            OnInformation($"Creating DataSource '{name}' in '{parent}' with overwrite:{overwrite}");


            var fullPath = parent == "/" ? $"/{name}" : $"{parent}/{name}";

            if (overwrite == false && reportingService.GetItemType(fullPath) == "DataSource")
            {
                OnWarning($"DataSource '{name}' in '{parent}' already exist.");
            }
            else
            {
                reportingService.CreateCatalogItem("DataSource", name, parent, overwrite, definition, null, out warnings);
            }



            //try
            //{
            //    reportingService.CreateCatalogItem("DataSource", name, parent, overwrite, definition, null, out warnings);
            //}
            //catch (Exception e)
            //{
            //    //Not sure how to catch only Microsoft.ReportingServices.Diagnostics.Utilities.ItemAlreadyExistsException
            //    OnWarning(e.Message);
            //}

            if (warnings != null)
                foreach (var warning in warnings)
                    OnWarning(warning.Message);
        }
    }
}
