namespace OpenDSM.Core.Models;

public record Tag(int id, string name);

public class Tags
{
    #region Private Fields

    private static Tags Instance = Instance ??= new();
    private readonly Tag[] tags;

    #endregion Private Fields

    #region Private Constructors

    private Tags()
    {
        Dictionary<int, string> TagDictionary = SQL.Tags.GetTags();
        tags = new Tag[TagDictionary.Count];
        for (int i = 0; i < tags.Length; i++)
        {
            tags[i] = new Tag(i, TagDictionary[i]);
        }
    }

    #endregion Private Constructors

    #region Public Methods

    public static Tag[] GetTags()
    {
        return Instance.tags;
    }

    #endregion Public Methods
}