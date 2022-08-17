using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDSM.Core.Models;

public class ReviewModel
{

    #region Public Properties

    public DateTime Posted { get; set; }
    public ProductModel Product { get; set; }
    public int Rating { get; set; }
    public string Summery { get; set; }
    public UserModel User { get; set; }

    #endregion Public Properties

}
