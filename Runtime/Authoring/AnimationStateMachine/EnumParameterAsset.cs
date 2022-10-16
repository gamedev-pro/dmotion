namespace DMotion.Authoring
{
    public class EnumParameterAsset : IntParameterAsset
    {
        [EnumTypeFilter]
        public SerializableType EnumType;
        public override string ParameterTypeName => EnumType.Type is { IsEnum: true } ? EnumType.Type.Name : "NONE";
    }
}