using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Net;

namespace CallWall.Web.EventStore.Configuration
{
    public sealed class IpAddressConverter : ConfigurationConverterBase
    {
        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type)
        {
            IPAddress result;
            if (value != null && IPAddress.TryParse(value.ToString(), out result))
            {
                return result;
            }
            return null;
        }

        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
        {
            return data == null ? null : data.ToString();
        }
    }
}