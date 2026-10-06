using System.Security.Cryptography;

namespace SecurityService.BusinessLogic.Mfa;

public sealed class RecoveryCodeGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public IReadOnlyList<string> Generate(int count)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        var codes = new List<string>(count);
        for (var codeIndex = 0; codeIndex < count; codeIndex++)
        {
            var characters = new char[16];
            for (var characterIndex = 0; characterIndex < characters.Length; characterIndex++)
            {
                characters[characterIndex] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
            }

            codes.Add(new string(characters));
        }

        return codes;
    }
}
