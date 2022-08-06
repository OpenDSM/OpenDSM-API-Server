using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDSM.Core.Models;

public class ReviewModel
{
    public UserModel User { get; set; }
    public ProductModel Product { get; set; }
    public string Summery { get; set; }
    public DateTime Posted { get; set; }
    public int Rating { get; set; }
}
