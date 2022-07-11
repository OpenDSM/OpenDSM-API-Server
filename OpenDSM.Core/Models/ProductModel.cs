using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDSM.Core.Models;

public class ProductModel
{
    public int Id { get; set; }
    public int UserID { get; set; }
    public string Name { get; set; }
    public string About { get; set; }
    public int TotalDownloads { get; set; }
    public string VideoURL { get; set; }
}
