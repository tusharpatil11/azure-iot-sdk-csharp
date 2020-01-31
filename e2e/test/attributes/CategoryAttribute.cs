using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Sdk;

namespace Microsoft.Azure.Devices.E2ETests.attributes
{
    [TraitDiscoverer("CustomXunitTrait.Tests.Infrastructure.CategoryDiscoverer", "CustomXunitTrait.Tests")]
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class CategoryAttribute : Attribute, ITraitAttribute
    {
        public abstract string Type { get; }
    }
}
