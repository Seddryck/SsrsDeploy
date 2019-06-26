using RsPackage.ReportingService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RsPackage.Execution
{
    public class ReportService : CatalogItemService
    {
        public ReportService()
            : base("Report", "report")
        { }

        public ReportService(ReportingService2010 reportingService)
            : base(reportingService, "Report", "report")
        { }

        public virtual void Create(string name, string parent, string path, string description, bool hidden, IDictionary<string, string> dataSources, IDictionary<string, string> sharedDatasets, IDictionary<string, string> localMap = null)
        {
            var warnings = Create(name, parent, path, description, hidden);
            if (warnings != null)
            {
                foreach (var warning in warnings)
                {
                    if (!(warning.Code == "rsDataSourceReferenceNotPublished" && dataSources != null && dataSources.ContainsKey(warning.ObjectName)))
                        OnWarning(warning.Message);
                }
            }

            ReferenceItem(name, parent, "DataSource", dataSources);
            ReferenceItem(name, parent, "DataSet", sharedDatasets, localMap);
        }

        protected virtual void ReferenceItem(string name, string parent, string referenceType, IDictionary<string, string> referencesOld, IDictionary<string, string> localMap = null)
        {
            var references = new Dictionary<string, string>(referencesOld, StringComparer.InvariantCultureIgnoreCase);

            var items = reportingService.GetItemReferences($"{parent}/{name}", referenceType);

            string label = string.Empty;
            switch (referenceType)
            {
                case "DataSet": label = "data set"; break;
                case "DataSource": label = "data source"; break;
            }

            if (items.Count() > 1)
                OnInformation($"Trying to reference {items.Count()} {label}s for report '{name}' in '{parent}'");

            var itemRefs = new List<ItemReference>();
            foreach (var item in items)
            {
                if (localMap != null && localMap.ContainsKey(item.Name))
                {
                    var itemRef = new ItemReference();
                    itemRef.Name = item.Name;
                    if (references != null && references.ContainsKey(localMap[item.Name]))
                    {
                        itemRef.Reference = references[localMap[item.Name]];
                        item.Reference = references[localMap[item.Name]];         //<--Extra Check???
                    }
                    itemRefs.Add(itemRef);
                    OnInformation($"Reference for {label} '{item.Name}' of report '{name}' found in LocalMaP and re-assigned.");
                }

                if (references != null && references.ContainsKey(item.Name))
                {
                    var itemRef = new ItemReference();
                    item.Reference = references[item.Name];         //<--Extra Check???
                    itemRef.Reference = references[item.Name];
                    itemRef.Name = item.Name;
                    itemRefs.Add(itemRef);
                    OnInformation($"Reference for {label} '{item.Name}' of report '{name}' found iN GlobalMap and re-assigned.");
                }
                else
                    OnWarning($"The {label} '{item.Name}' of the report '{name}' has not been re-assigned because this {label} is not defined in the deployment manifest.");
            }

            if (items != null && !items.Any(r => string.IsNullOrEmpty(r.Reference)))
                reportingService.SetItemReferences($"{parent}/{name}", itemRefs.ToArray());
            else
                OnError($"The {label}s of report '{name}' have not been redirected because some of them are not available in the manifest.");
        }



    }
}
