namespace SOTFEdit.Infrastructure;

public interface ICloseableWithResult
{
    public void Close(bool hasChanges);
}