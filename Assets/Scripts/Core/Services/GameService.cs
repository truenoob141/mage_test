namespace MageTest.Core.Services
{
    public class GameService
    {
        public bool IsValidGame { get; private set; }

        public void SetIsValidGame(bool isValid)
        {
            IsValidGame = isValid;
        }
    }
}