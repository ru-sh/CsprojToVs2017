using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Project2015To2017.Definition;

namespace Project2015To2017.Transforms
{
	public class EmptyGroupRemoveTransformation
		: ITransformationWithTargetMoment, ITransformationWithDependencies
	{
		public void Transform(Project definition)
		{
			definition.PropertyGroups = FilterNonEmpty(definition.PropertyGroups);
			definition.ItemGroups = FilterNonEmpty(definition.ItemGroups).ToList();
		}

		private static IReadOnlyList<XElement> FilterNonEmpty(IEnumerable<XElement> groups)
		{
			var list = groups.ToList();
			foreach (var gr in list)
			{
				var (remove, keep) = gr.Elements()
					.Split(el => el.Name.LocalName == "Compile" && el.Attributes("Link").Any(link => link.Value.Contains("AssemblyInfo")));

				foreach (var element in remove)
				{
					element.Remove();
				}
			}

			return list;
		}

		public TargetTransformationExecutionMoment ExecutionMoment =>
			TargetTransformationExecutionMoment.Late;

		public IReadOnlyCollection<string> DependOn => new[]
		{
			typeof(PrimaryProjectPropertiesUpdateTransformation).Name,
		};
	}
}