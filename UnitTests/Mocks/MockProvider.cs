using System;
using System.Collections.Generic;
using MainApp.Interfaces;

namespace UnitTests.Mocks
{
    public class MockProvider : IProvider
    {
        public MockProvider()
        {
            folders = new List<string>();
        }
        Tuple<IList<string>, int> IProvider.GetFolders()
        {
            return new Tuple<IList<string>, int>(folders, ErrorCode);
        }

        public void AddFolder(string name)
        {
            folders.Add(name);
        }

        public void GenerateSomeFolders()
        {
            //some code to generating folders
        }

        public IList<string> folders;
        public int ErrorCode { get; set; }
    }
}