namespace RoadSafety_backend.Application.Interfaces;

public interface IPasswordService
{
    public string Hash(string password);

    public bool Verify(string password, string hashedPassword);
}
