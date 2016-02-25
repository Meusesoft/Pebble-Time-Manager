using System;
using System.Runtime.Serialization;
using P3bble.Helper;
using System.Collections.Generic;

namespace P3bble.Types
{
    [DataContract]
    internal class BundleManifest
    {
        [DataMember(Name = "manifestVersion", IsRequired = true)]
        public int ManifestVersion { get; private set; }

        public DateTime GeneratedAt
        {
            get
            {
                return this.GeneratedAtInternal.AsDateTime();
            }
        }

        [DataMember(Name = "generatedBy", IsRequired = true)]
        public string GeneratedBy { get; private set; }

        [DataMember(Name = "application", IsRequired = false)]
        public ApplicationManifest ApplicationManifest { get; private set; }

        [DataMember(Name = "firmware", IsRequired = false)]
        public FirmwareManifest Firmware { get; private set; }

        [DataMember(Name = "resources", IsRequired = false)]
        public ResourcesManifest Resources { get; private set; }

        [DataMember(Name = "type", IsRequired = true)]
        public string Type { get; private set; }

        [DataMember(Name = "generatedAt", IsRequired = true)]
        internal int GeneratedAtInternal { get; set; }
    }

    [DataContract]
    public class BundleAppinfo
    {
        [DataMember(Name = "shortName", IsRequired = true)]
        public string ShortName { get; private set; }

        [DataMember(Name = "capabilities")]
        public List<String> Capabilities { get; private set; }

        [DataMember(Name = "targetPlatforms")]
        public List<String> TargetPlatforms { get; private set; }

        [DataMember(Name = "appKeys")]
        public List<AppKey> AppKeys { get; private set; }
    }


    [DataContract]
    public class AppKey
    {
        public string name { get; private set; }
        public int id { get; private set; }
    }
}
