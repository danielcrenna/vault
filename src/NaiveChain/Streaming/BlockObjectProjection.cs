using System.Collections.Generic;
using NaiveChain.Models;

namespace NaiveChain.Streaming
{
	public class BlockObjectProjection
    {
	    private readonly IBlockRepository _source;
	    private readonly IBlockObjectTypeProvider _typeProvider;

	    public BlockObjectProjection(IBlockRepository source, IBlockObjectTypeProvider typeProvider)
	    {
		    _source = source;
		    _typeProvider = typeProvider;
	    }

	    public IEnumerable<T> StreamForwards<T>(int startingAt = 0) where T : IBlockSerialized
	    {
			var type = _typeProvider.Get(typeof(T));
			if(!type.HasValue)
				yield break;

		    foreach (var item in _source.StreamAllBlockObjects())
		    {
			    if (!item.Type.HasValue || item.Type != type)
				    continue;

			    yield return (T) item.Data;
		    }
		}
    }
}
