namespace Mindflow_backend.Journal.Domain.Model;

public enum JournalError
{
    JournalEntryNotFound,
    EntryTagNotFound,
    JournalEntryCreationFailed,
    JournalEntryUpdateFailed,
    JournalEntryDeletionFailed,
    UserIdMismatch
}