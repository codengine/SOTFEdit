namespace SOTFEdit.Model.Map.Static;

// ReSharper disable once ClassNeverInstantiated.Global
public class Teleport
{
    private readonly int _areaMask;
    private readonly float _x;
    private readonly float _y;
    private readonly float _z;

    public Teleport(float x, float y, float z, int areaMask)
    {
        _x = x;
        _y = y;
        _z = z;
        _areaMask = areaMask;
    }

    public Position ToPosition(AreaMaskManager areaMaskManager)
    {
        return new Position(_x, _y, _z)
        {
            Area = areaMaskManager.GetAreaForAreaMask(_areaMask)
        };
    }
}