using Vigilance.API;

namespace Vigilance.API.Commands
{
    public interface Command
    {
        string Usage { get; }
        bool OverwriteBaseGameCommand { get; }
        string OnCall(Player sender, string[] args);
    }
}
