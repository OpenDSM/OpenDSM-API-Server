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
public record PlatformVersion(Platform platform, string downloadUrl, int total_downloads, int weekly_downloads);
public class VersionModel
{
    public long ID { get; }
    public int ProductID { get; }
    public string Name { get; }
    public ReleaseType Type { get; }
    public int TotalDownloads { get; }
    public int WeeklyDownloads { get; }
    public List<PlatformVersion> Platforms { get; }

    public VersionModel(long iD, int product_id, string name, ReleaseType type, int total_downloads, int weekly_downloads, List<PlatformVersion> platforms)
    {
        ID = iD;
        Name = name;
        Type = type;
        Platforms = platforms;
        ProductID = product_id;
        TotalDownloads = total_downloads;
        WeeklyDownloads = weekly_downloads;
    }

    public static VersionModel CreateVersion(ProductModel product, string name, string changelog_url, ReleaseType type, List<PlatformVersion> platforms)
    {
        Versions.CreateVersion(product.Id, name, (byte)type, changelog_url, out long id);
        foreach (PlatformVersion platform in platforms)
        {
            Versions.CreatePlatformVersion(platform.platform, platform.downloadUrl, id);
        }
        return new VersionModel(id, product.Id, name, type, 0, 0, platforms);
    }
    public static VersionModel? GetVersionByID(long id, int product_id)
    {
        if (Versions.GetVersionByID(id, product_id, out string name, out byte releaseType, out byte[] platforms))
        {
            int version_total_downloads = 0, version_weekly_downloads = 0;
            List<PlatformVersion> platform_list = new();
            foreach (byte platform_id in platforms)
            {
                if (Versions.GetPlatformVersionByID(platform_id, id, out string download_url, out int total_downloads, out int weekly_dowloads))
                {
                    platform_list.Add(new((Platform)platform_id, download_url, total_downloads, weekly_dowloads));
                    version_total_downloads += total_downloads;
                    version_weekly_downloads += weekly_dowloads;
                }
            }
            return new(id, product_id, name, (ReleaseType)releaseType, version_total_downloads, version_weekly_downloads, platform_list);
        }
        return null;
    }
}
