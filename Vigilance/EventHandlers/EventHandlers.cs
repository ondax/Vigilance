using Vigilance.Events;

namespace Vigilance.EventHandlers
{
    public interface AnnounceDecontaminationHandler : EventHandler
    {
        void OnAnnounceDecontamination(AnnounceDecontaminationEvent ev);
    }

    public interface AnnounceNTFEntranceHandler : EventHandler
    {
        void OnAnnounceEntrance(AnnounceNTFEntrance ev);
    }
}
