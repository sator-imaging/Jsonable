using Jsonable;

#pragma warning disable IDE1006  // Naming Styles
#pragma warning disable CA1852  // Seal internal types

namespace Tests
{
    [FromJson]
    [ToJson]
    partial class HashConflictTest
    {
        public int UniqueNameHash { get; set; }
        public int costarring { get; set; }
        public int liquid { get; set; }

        // longer property name should not use stackalloc in generated ToJson method
        public string costarring______________________________________________________________________ { get; set; } = "";
        public string liquid______________________________________________________________________ { get; set; } = "";
    }

    [FromJson]
    partial class LookupTableGeneration
    {
        public int costarring { get; set; }
        public int liquid { get; set; }
        public int P0 { get; set; }
        public int P1 { get; set; }
        public int P2 { get; set; }
        public int P3 { get; set; }
        public int P4 { get; set; }
        public int P5 { get; set; }
        public int P6 { get; set; }
        public int P7 { get; set; }
        public int P8 { get; set; }
        public int P9 { get; set; }
    }
}
