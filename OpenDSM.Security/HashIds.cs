using ChaseLabs.CLConfiguration;
using HashidsNet;
using OpenDSM.Core.Handlers;

namespace OpenDSM.Security;

public class HashIds
{
    #region Public Fields

    public static HashIds Instance = Instance ??= new();

    #endregion Public Fields

    #region Public Methods

    public string Get(int id) => hash.Encode(id);

    public int Get(string key) => hash.DecodeSingle(key);

    #endregion Public Methods

    #region Private Constructors

    private HashIds()
    {
        Encryption enc = new("hash");
        hash = new Hashids(enc.SALT, 11);
    }

    #endregion Private Constructors

    #region Private Fields

    private readonly IHashids hash;

    #endregion Private Fields
}
