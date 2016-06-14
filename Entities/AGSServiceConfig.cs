// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace AgsManager
{
    public class AGSServiceConfig
    {
        public string folderName { get; set; }
        public string serviceName { get; set; }
        public string type { get; set; }
        public string description { get; set; }
        public string status { get; set; }
        public string capabilities { get; set; }
        public string clusterName { get; set; }
        public string minInstancesPerNode { get; set; }
        public string maxInstancesPerNode { get; set; }
        public string instancesPerContainer { get; set; }
        public string maxWaitTime { get; set; }
        public string maxStartupTime { get; set; }
        public string maxIdleTime { get; set; }
        public string maxUsageTime { get; set; }
        public string loadBalancing { get; set; }
        public string isolationLevel { get; set; }
        public string configuredState { get; set; }
        public string recycleInterval { get; set; }
        public string recycleStartTime { get; set; }
        public string keepAliveInterval { get; set; }
        public string isDefault { get; set; }
        public AGSServiceExtension[] extensions { get; set; }
    }
}