using System.ComponentModel;
using System.Configuration;
using System.Net;

namespace CallWall.Web.EventStore.Configuration
{
    public class CallWallEventStoreSection : ConfigurationSection
    {
        public static CallWallEventStoreSection GetConfig()
        {
            var section = ConfigurationManager.GetSection("CallWallEventStore") as CallWallEventStoreSection;
            return section;
        }

        [ConfigurationProperty("endpoint", IsRequired = true)]
        [TypeConverter(typeof(IpAddressConverter))]
        [CallbackValidator(Type = typeof(IPAddress), CallbackMethodName = "ValidateIpAddress")]
        public IPAddress Endpoint
        {
            get { return (IPAddress)this["endpoint"]; }
        }

        public static void ValidateIpAddress(object ipAddress)
        {
            if(ipAddress == null)
                throw new ConfigurationErrorsException("The configuration value for the EventStore IPAddress is null. It must be provided");
            IPAddress _;
            if (!IPAddress.TryParse(ipAddress.ToString(), out _))
            {
                throw new ConfigurationErrorsException(
                    "The configuration value for the EventStore IPAddress could not be parsed. It must be provided");
            }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        [TypeConverter(typeof(Int32Converter))]
        [CallbackValidator(Type = typeof(IPAddress), CallbackMethodName = "ValidatePort")]
        public int Port
        {
            get { return (int)this["port"]; }
        }

        public static void ValidatePort(object value)
        {
            if (value == null)
                throw new ConfigurationErrorsException("The configuration value for the EventStore port is null. It must be provided");
            int _;
            if (!int.TryParse(value.ToString(), out _))
            {
                throw new ConfigurationErrorsException("The configuration value for the EventStore port could not be parsed as an integer.");
            }

        }
    }
}