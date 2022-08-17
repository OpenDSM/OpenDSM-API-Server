// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
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
        List<Tag> list = new();

        foreach (var (id, name) in TagDictionary)
        {
            list.Add(new(id, name));
        }

        tags = list.ToArray();
    }

    #endregion Private Constructors

    #region Public Methods

    public static Tag[] GetTags()
    {
        return Instance.tags;
    }

    #endregion Public Methods

}