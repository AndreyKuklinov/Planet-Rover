public interface IRecolorable
{
    public ItemColorData CurrentColor { get; }
    public void Recolor(ItemColorData newColor);
    public bool CanBeRecolored(ItemColorData newColor);
}