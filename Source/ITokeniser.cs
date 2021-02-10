using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RandomText
{
    public interface ITokeniser
    {
        public IEnumerable<String?> Shatter(StreamReader input, ShatteringOptions? options = null);
        public Task<IEnumerable<String?>> ShatterAsync(StreamReader input, ShatteringOptions? options = null);
    }
}
