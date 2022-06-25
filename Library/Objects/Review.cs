using OpenDSM.Lib.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDSM.Lib.Objects;

public class Review
{
    #region Fields

    public static byte MAX_STAR = 50;

    #endregion Fields

    #region Public Constructors

    public Review(string shortDescription, string longDescription, User writer, DateTime postDate, Product product, byte star)
    {
        ShortDescription = shortDescription;
        LongDescription = longDescription;
        Writer = writer;
        PostDate = postDate;
        Comments = Array.Empty<Review>();
        ProductID = product.ID;
        Star = star > MAX_STAR ? MAX_STAR : star;
    }

    #endregion Public Constructors

    #region Properties

    public Review[] Comments { get; set; }

    public string LongDescription { get; set; }

    public DateTime PostDate { get; set; }

    public long ProductID { get; set; }

    public string ShortDescription { get; set; }

    public byte Star { get; set; }

    public User Writer { get; set; }

    #endregion Properties

    #region Public Methods

    public Review LeaveComment(Review review)
    {
        List<Review> list = Comments.ToList();
        list.Add(review);
        Comments = list.ToArray();
        return review;
    }

    #endregion Public Methods
}