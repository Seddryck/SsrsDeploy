using RsPackage;
using RsPackage.ReportingService;
using RsPackage.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RsPackage.Logging;

namespace RsPackage.Factory
{
    public class ServiceBuilder
    {
        private Options options;
        private ReportingService2010 rs;
        private ILogger logger;
        private Dictionary<Type, BaseService> services;
        private bool isBuilt;

        public ServiceBuilder()
        {
            this.logger = new NullLogger();
            this.services = new Dictionary<Type, BaseService>();
            this.isBuilt = false;
        }

        public void Setup(Options options)
        {
            this.options = options;
            this.isBuilt = false;
        }

        public void Build()
        {
            this.rs = BuildReportingService(options);
            this.logger = BuildLogger(options);
            AddService(new ReportService(rs));
            AddService(new FolderService(rs));
            AddService(new DataSourceService(rs));
            AddService(new PolicyService(rs));
            AddService(new SharedDatasetService(rs));
            this.isBuilt = true;
        }

        private void AddService(BaseService service)
        {
            services.Add(service.GetType(), service);
            service.MessageSent += logger.WriteMessage;
        }

        protected virtual ILogger BuildLogger(Options options)
        {
            var loggerFactory = new LoggerFactory();
            return loggerFactory.Build(options);
        }

        protected virtual ReportingService2010 BuildReportingService(Options options)
        {
            var rs = new ReportingService2010();
            var urlBuilder = new UrlBuilder();
            urlBuilder.Setup(options);
            urlBuilder.Build();
            rs.Url = urlBuilder.GetUrl();
            rs.Credentials = System.Net.CredentialCache.DefaultCredentials;
            return rs;
        }

        protected virtual BaseService GetService(Type type)
        {
            if (!isBuilt)
                throw new InvalidOperationException();

            if (services.ContainsKey(type))
                return services[type];
            else
                throw new ArgumentOutOfRangeException();
        }

        public ReportService GetReportService()
        {
            return (ReportService)GetService(typeof(ReportService));
        }

        public FolderService GetFolderService()
        {
            return (FolderService)GetService(typeof(FolderService));
        }

        public DataSourceService GetDataSourceService()
        {
            return (DataSourceService)GetService(typeof(DataSourceService));
        }

        public SharedDatasetService GetDataSetService()
        {
            return (SharedDatasetService)GetService(typeof(SharedDatasetService));
        }

        public PolicyService GetPolicyService()
        {
            return (PolicyService)GetService(typeof(PolicyService));
        }
    }
}
