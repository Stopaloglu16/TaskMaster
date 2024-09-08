using System.Text;

namespace SharedUtilityTestMethods;

public class TextGenerator
{
    private static Random random = new Random();


    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }



    public static string GeneratePassword(int length)
    {
        if (length < 4) throw new ArgumentException("Password length must be at least 4 to include one of each character type.");

        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string numberChars = "0123456789";
        const string specialChars = "!@#$%^&*()_+=-{}[]:;<>?,./";

        // Ensure that the password contains at least one of each required character type
        var password = new StringBuilder();
        password.Append(upperChars[random.Next(upperChars.Length)]);
        password.Append(lowerChars[random.Next(lowerChars.Length)]);
        password.Append(numberChars[random.Next(numberChars.Length)]);
        password.Append(specialChars[random.Next(specialChars.Length)]);

        // Combine all possible characters for the remaining positions
        string allChars = upperChars + lowerChars + numberChars + specialChars;

        // Fill the rest of the password
        for (int i = 4; i < length; i++)
        {
            password.Append(allChars[random.Next(allChars.Length)]);
        }

        // Shuffle the characters to randomize their positions
        return new string(password.ToString().ToCharArray().OrderBy(x => random.Next()).ToArray());
    }

}
