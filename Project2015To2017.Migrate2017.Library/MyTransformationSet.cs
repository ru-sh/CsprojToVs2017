using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Project2015To2017.Transforms;

namespace Project2015To2017.Migrate2017
{
	public class MyTransformationSet: ITransformationSet
	{
		public IReadOnlyCollection<ITransformation> Transformations(ILogger logger, ConversionOptions conversionOptions)
		{
			return Vs15TransformationSet.TrueInstance.Transformations(logger, conversionOptions)
				.Concat(new[]
			{
				new LinkedAsmInfoRemoveTransformation()
			}).ToList();
		}
	}
}
