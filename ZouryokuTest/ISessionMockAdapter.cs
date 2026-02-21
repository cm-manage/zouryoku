using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace ZouryokuTest
{
        // 拡張メソッド(Get, GetString, SetString)が呼ばれた場合にこのインスタンスを経由してMockのメソッドを呼び出す
        internal class ISessionMockAdapter(Mock<ISession> session) : ISession
        {
            public bool IsAvailable => session.Object.IsAvailable;

            public string Id => session.Object.Id;

            public IEnumerable<string> Keys => session.Object.Keys;

            public void Clear() => session.Object.Clear();

            public Task CommitAsync(CancellationToken cancellationToken = default) => session.Object.CommitAsync(cancellationToken);

            public Task LoadAsync(CancellationToken cancellationToken = default) => session.Object.LoadAsync(cancellationToken);

            public void Remove(string key) => session.Object.Remove(key);

            public void Set(string key, byte[] value) => session.Object.Set(key, value);
            public void SetString(string key, string value) => session.Object.Set(key, Encoding.UTF8.GetBytes(value));

            public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value) => session.Object.TryGetValue(key, out value);
            public string? GetString(string key)
            {
                var data = Get(key);
                if (data == null)
                {
                    return null;
                }
                return Encoding.UTF8.GetString(data);
            }

            /// <summary>
            /// Gets a byte-array value from <see cref="ISession"/>.
            /// </summary>
            /// <param name="session">The <see cref="ISession"/>.</param>
            /// <param name="key">The key to read.</param>
            public byte[]? Get(string key)
            {
                TryGetValue(key, out var value);
                return value;
            }
        }
}
