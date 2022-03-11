namespace web.Data;

public static class Users
{
    public static List<string> Names = new()
    {
        "Rod", "Todd", "Marge", "Bart", "Homer", "Maggie", "Lisa", "Ned", "Krusty"
    };

    public static List<string> Groups = new()
    {
        "Yellow", "Green", "Orange", "Ugly", "Pretty", "Stinky", "Smelly", "Dry", "Wet"
    };

    public static string GenerateUserName()
    {
        return $"{FindRandomString(Groups)}_{FindRandomString(Names)}";
    }

    private static string FindRandomString(IList<string> list)
    {
        var random = new Random();
        var next = random.Next(0, list.Count - 1);
        return list[next];
    }
}