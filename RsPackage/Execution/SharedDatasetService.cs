using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using System.IO;
using RsPackage.ReportingService;

namespace RsPackage.Execution
{
    public class SharedDatasetService : CatalogItemService
    {
        public SharedDatasetService()
            : base("DataSet", "data set")
        { }

        public SharedDatasetService(ReportingService2010 reportingService)
            : base(reportingService, "DataSet", "data set")
        { }

        public virtual void Create(string name, string parent, string path, string description, bool hidden, string dataSourceName, IDictionary<string, string> dataSources)
        {

            description = "";

            Warning[] warnings = null;
            try
            {
                warnings = Create(name, parent, path, description, hidden);
            }
            catch (Exception e)
            {
                OnWarning(e.Message);
            }

            if (warnings != null)
            {
                foreach (var warning in warnings)
                {
                    if (!(warning.Code == "rsDataSourceReferenceNotPublished" && dataSources.ContainsKey(dataSourceName)))
                    {
                        OnWarning(warning.Message);
                    }

                    if (warning.Code == "rsDataSourceReferenceNotPublished" && dataSourceName == "")
                    {
                        var result = from Match match in Regex.Matches(warning.Message, "‘(.*?)’") select match.Groups[1].Value;
                        dataSourceName = result.ToArray()[1];
                    }
                }
            }
            ReferenceItem(name, parent, dataSourceName, dataSources);
        }

        protected virtual void ReferenceItem(string name, string parent, string reference, IDictionary<string, string> referencesOld)
        {
            var references = new Dictionary<string, string>(referencesOld, StringComparer.InvariantCultureIgnoreCase);

            var item = reportingService.GetItemReferences($"{parent}/{name}", "DataSource")[0];

            var dataSourceRef = new ItemReference();
            if (references != null && references.ContainsKey(reference))
            {
                dataSourceRef.Reference = references[reference];
                dataSourceRef.Name = item.Name;
                reportingService.SetItemReferences($"{parent}/{name}", Enumerable.Repeat(dataSourceRef, 1).ToArray());
                OnInformation($"Reference to data source '{reference}' in data set '{name}' found and re-assigned.");
            }
            else
                OnError($"The data source '{reference}' of the data set '{name}' has not been re-assigned because this data source is not defined in the deployment manifest.");
        }
    }
}
