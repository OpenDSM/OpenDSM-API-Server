namespace OpenDSM.Products.Models;

public struct ProductModel
{
    public static ProductModel Empty = new(-1, "", "", "n/a", -1);
    public int ID { get; }
    public string Name { get; }
    public string ShortDescription { get; }
    public string About { get; }
    public string Poster { get; }
    public string Banner { get; }
    public string Slug { get; }
    public int OwnerID { get; }
    public bool IsEmpty { get; }

    internal ProductModel(int id, string name, string short_description, string slug, int owner_id)
    {
        ID = id;
        Name = name;
        ShortDescription = short_description;
        Slug = slug;
        OwnerID = owner_id;
        Poster = Path.Combine(GetProductDirectory(id), "images", "poster.jpg");
        Banner = Path.Combine(GetProductDirectory(id), "images", "banner.jpg");
        IsEmpty = id == -1 || owner_id == -1;

        using FileStream fs = new(Path.Combine(GetProductDirectory(id), "about.md"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using StreamReader reader = new(fs);
        About = reader.ReadToEnd();
    }


}
