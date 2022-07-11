using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDSM.Core.Models;

public class ProductModel
{
    public int Id { get; private set; }
    public int UserID { get; private set; }
    public string Name { get; private set; }
    public string About { get; private set; }
    public int TotalDownloads { get; private set; }
    public string VideoURL { get; private set; }
    public UserModel User { get; private set; }
}
