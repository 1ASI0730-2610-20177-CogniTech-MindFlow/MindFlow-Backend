using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Encryption;

public sealed class EncryptedStringConverter(AesEncryptionService encryption)
    : ValueConverter<string, string>(
        v => encryption.Encrypt(v),
        v => encryption.DecryptSafe(v));