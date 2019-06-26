using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RsPackage.ReportingService;

namespace RsPackage.Execution
{
    public abstract class CatalogItemService : BaseService
    {
        private readonly string labelItem;
        private readonly string itemType;

        public CatalogItemService(string itemType, string labelItem)
            : base()
        {
            this.itemType = itemType;
            this.labelItem = labelItem;
        }

        public CatalogItemService(ReportingService2010 reportingService, string itemType, string labelItem)
            : base(reportingService)
        {
            this.itemType = itemType;
            this.labelItem = labelItem;
        }

        public Warning[] Create(string name, string parent, string path, string description, bool hidden)
        {
            if (!File.Exists(path))
            {
                OnError($"File '{path}' doesn't exist!");
                return null;
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
            OnInformation($"Creating {labelItem} '{name}' in '{parent}'");

            var properties = new List<Property>();
            properties.Add(new Property() { Name = "Description", Value = description });
            properties.Add(new Property() { Name = "Hidden", Value = hidden.ToString() });

            try
            {
                reportingService.CreateCatalogItem(itemType, name, parent, true, definition, properties.ToArray(), out warnings);
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }


            return warnings;
        }









    }
}
