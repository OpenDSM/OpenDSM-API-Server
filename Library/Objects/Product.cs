using OpenDSM.Lib.Auth;

namespace OpenDSM.Lib.Objects;

public class Product
{
    #region Public Constructors

    public Product(Guid iD, User owner, string name, string description, string repo, params Version[] versions)
    {
        Description = description;
        ID = iD;
        Name = name;
        Owner = owner;
        Repo = repo;
        Versions = versions;
    }

    #endregion Public Constructors

    #region Properties

    public string Description { get; set; }
    public Guid ID { get; set; }
    public string Name { get; set; }
    public User Owner { get; set; }
    public string Repo { get; set; }

    public Version[] Versions { get; set; }

    #endregion Properties
}