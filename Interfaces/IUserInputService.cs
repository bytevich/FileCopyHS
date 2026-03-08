namespace FileCopyHS.Interfaces
{
    public interface IUserInputService
    {
        Tuple<string, string> ValidateUserInput();
    }
}