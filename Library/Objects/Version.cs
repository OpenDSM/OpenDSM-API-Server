namespace OpenDSM.Lib.Objects;

public enum ReleaseType
{
    Release,
    Beta,
    Alpha
}

public enum SupportedOS
{
    Windows,
    WindowsARM,
    Linux,
    LinuxARM,
    MacIntel,
    MacApple
}

public class Version
{
    #region Public Constructors

    public Version(uint iD, string name, ReleaseType releaseType, params SupportedOS[] supportedOperatingSystems)
    {
        ID = iD;
        Name = name;
        ReleaseType = releaseType;
        SupportedOperatingSystems = supportedOperatingSystems;
    }

    #endregion Public Constructors

    #region Properties

    public uint Downloads { get; set; }
    public uint ID { get; set; }
    public string Name { get; set; }
    public ReleaseType ReleaseType { get; set; }
    public SupportedOS[] SupportedOperatingSystems { get; set; }

    #endregion Properties
}