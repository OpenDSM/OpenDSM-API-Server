using OpenDSM.SQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDSM.Core.Models;

public enum ReleaseType
{
    Release,
    Beta,
    Alpha,
    Unkown
}
public record PlatformVersion(Platform platform, string downloadUrl, int total_downloads, int weekly_downloads, long file_size);
public class VersionModel
{
    #region Public Constructors

    public VersionModel(long iD, int product_id, string name, ReleaseType type, List<PlatformVersion> platforms, string changelog, DateTime posted)
    {
        ID = iD;
        Name = name;
        Type = type;
        Platforms = platforms;
        ProductID = product_id;
        TotalDownloads = 0;
        WeeklyDownloads = 0;
        foreach (PlatformVersion platform in platforms)
        {
            TotalDownloads += platform.total_downloads;
            WeeklyDownloads += platform.weekly_downloads;
        }
        Changelog = changelog;
        Posted = posted;
    }

    #endregion Public Constructors

    #region Public Properties

    public string Changelog { get; }
    public long ID { get; }
    public string Name { get; }
    public List<PlatformVersion> Platforms { get; }
    public DateTime Posted { get; }
    public int ProductID { get; }
    public int TotalDownloads { get; }
    public ReleaseType Type { get; }
    public int WeeklyDownloads { get; }

    #endregion Public Properties
}
