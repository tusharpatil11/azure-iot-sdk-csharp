using Microsoft.Azure.Devices.E2ETests.attributes;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Microsoft.Azure.Devices.E2ETests
{
    public class IotHubAttribute : CategoryAttribute
    {
        public override string Type => "IotHub";
    }
}
