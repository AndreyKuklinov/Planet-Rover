public interface IRecolorable
{
    public ItemColorData ColorData { get; }
    public void Recolor(ItemColorData newColor);
    public bool CanBeRecolored(ItemColorData newColor);
}