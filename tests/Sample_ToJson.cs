using Jsonable;
using System;
using System.Collections.Generic;

#pragma warning disable IDE1006  // Naming Styles
#pragma warning disable CA1852  // Seal internal types

namespace Tests
{
    partial class Base
    {
        public int BasePublicProp { get; private set; }
        internal float BaseInternalProp { get; private set; }
    }

    [ToJson(IncludeInternals = false, ExcludeInherited = false)]
    partial class TypeHierarchyTest : Base
    {
        public int PublicProp { get; private set; }
        internal float InternalProp { get; private set; }
    }

    [ToJson]
    partial class CollectionTest
    {
        public int[] IntArrayProp { get; private set; } = Array.Empty<int>();
        public List<double> ListIntProp { get; private set; } = new();
        public Dictionary<string, float> DictStringIntProp { get; private set; } = new();
    }
}
