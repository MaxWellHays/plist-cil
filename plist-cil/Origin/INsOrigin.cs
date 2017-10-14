namespace Claunia.PropertyList.Origin
{
    public interface INsOrigin
  {
    OriginType OriginType { get; }

    int Location { get; }

    int Length { get; }
  }

  public enum OriginType
  {
    Binary,
    XmlText,
  }
}
