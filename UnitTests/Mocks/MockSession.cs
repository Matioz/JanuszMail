using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UnitTests.Mocks
{
    public class MockSession : ISession
    {
        Dictionary<string, object> sessionStorage = new Dictionary<string, object>();

        public MockSession()
        {
        }

        public object this[string name]
        {
            get { return sessionStorage[name]; }
            set { sessionStorage[name] = value; }
        }

        string ISession.Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool ISession.IsAvailable
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        IEnumerable<string> ISession.Keys
        {
            get { return sessionStorage.Keys; }
        }

        void ISession.Clear()
        {
            sessionStorage.Clear();
        }

        Task ISession.CommitAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task ISession.LoadAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        void ISession.Remove(string key)
        {
            sessionStorage.Remove(key);
        }

        void ISession.Set(string key, byte[] value)
        {
            sessionStorage[key] = value;
        }

        bool ISession.TryGetValue(string key, out byte[] value)
        {
            try
            {
                value = sessionStorage[key] as byte[];
                return true;
            }
            catch (KeyNotFoundException)
            {
                sessionStorage.Add(key, null);
                value = null;
            }
            catch (NullReferenceException)
            {
                value = null;
            }
            return false;
        }
    }
}