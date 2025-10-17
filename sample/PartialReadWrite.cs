using Jsonable;

namespace Sample
{
    // partial read/write
    [ToJson(Property = nameof(Position))]
    [ToJson(Property = nameof(Rotation))]
    [ToJson(Property = nameof(Scale))]
    [ToJson(Property = nameof(Payload))]
    [ToJson, FromJson]
    partial record Composite
    {
        public float[]? Position { get; set; }
        public float[]? Rotation { get; set; }
        public float[]? Scale { get; set; }
        public Payload? Payload { get; set; }
    }

    [ToJson, FromJson]
    partial record struct Payload(int Id, string Name)
    {
        public Payload() : this(310, "SATOR") { }
    }

    [ToJson, FromJson] partial record struct PositionOnly(float[] Position) { }
    [ToJson, FromJson] partial record struct RotationOnly(float[] Rotation) { }
    [ToJson, FromJson] partial record struct ScaleOnly(float[] Scale) { }
    [ToJson, FromJson] partial record struct PayloadOnly(Payload Payload) { }
}
