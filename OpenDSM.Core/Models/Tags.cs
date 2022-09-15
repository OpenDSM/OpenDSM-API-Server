// LFInteractive LLC. (c) 2021-2022 - All Rights Reserved
using System.Collections.Immutable;

namespace OpenDSM.Core.Models;

public class Tags
{

    #region Private Fields

    private static Tags Instance = Instance ??= new();
    private IReadOnlyDictionary<int, string> tags;

    #endregion Private Fields

    #region Private Constructors

    private Tags()
    {
        tags = SQL.Tags.GetTags();
    }

    #endregion Private Constructors

    #region Public Methods

    public static IReadOnlyDictionary<int, string> GetTags()
    {
        return Instance.tags;
    }

    #endregion Public Methods

}