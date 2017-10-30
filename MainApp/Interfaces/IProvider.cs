using System;
using System.Collections.Generic;

namespace MainApp.Interfaces
{
    public interface IProvider
    {
        Tuple<IList<string>, int> GetFolders();
    }
}