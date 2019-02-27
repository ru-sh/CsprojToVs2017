using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Project2015To2017.Definition;

namespace Project2015To2017.Transforms
{
	public sealed class NuGetPackageTransformation
		: ITransformationWithTargetMoment, ILegacyOnlyProjectTransformation
	{
		public void Transform(Project definition)
		{
			if (definition.PackageConfiguration == null)
			{
				return;
			}

			var packageConfig = PopulatePlaceHolders(definition);

			ConstrainPackageReferences(definition.PackageReferences, packageConfig);

			definition.PackageConfiguration = packageConfig;
		}

		private static void ConstrainPackageReferences(IReadOnlyList<PackageReference> rawPackageReferences, PackageConfiguration packageConfig)
		{
			var dependencies = packageConfig.Dependencies;
			if (dependencies == null || rawPackageReferences == null)
			{
				return;
			}

			var packageIdConstraints = dependencies.Select(dependency => new
			{
				PackageId = dependency.Attribute("id").Value,
				Version = dependency.Attribute("version").Value
			}).ToArray();

			foreach (var packageReference in rawPackageReferences)
			{
				var matchingPackage = packageIdConstraints.SingleOrDefault(dependency => packageReference.Id.Equals(dependency.PackageId, StringComparison.OrdinalIgnoreCase));

				if (matchingPackage != null)
				{
					packageReference.Version = matchingPackage.Version;
				}
			}
		}

		private PackageConfiguration PopulatePlaceHolders(Project project)
		{
			var rawPackageConfig = project.PackageConfiguration ?? throw new ArgumentNullException("project.PackageConfiguration");
			var assemblyAttributes = project.AssemblyAttributes ?? throw new ArgumentNullException("project.AssemblyAttributes");

			var version = PopulatePlaceHolder("version", rawPackageConfig.Version, assemblyAttributes.InformationalVersion ?? assemblyAttributes.Version);
			var authors = PopulatePlaceHolder("author", rawPackageConfig.Authors, assemblyAttributes.Company);
			var description = PopulatePlaceHolder("description", rawPackageConfig.Description, assemblyAttributes.Description);
			var copyright = PopulatePlaceHolder("copyright", rawPackageConfig.Copyright, assemblyAttributes.Copyright);
			return new PackageConfiguration
			{
				//Id does not need to be specified in new project format if it is just the same as the assembly name
				Id = rawPackageConfig.Id == "$id$" ? null : rawPackageConfig.Id,
				Version = version,
				Authors = authors,
				Description = description,
				Copyright = copyright,
				LicenseUrl = rawPackageConfig.LicenseUrl,
				ProjectUrl = rawPackageConfig.ProjectUrl,
				IconUrl = rawPackageConfig.IconUrl,
				Tags = rawPackageConfig.Tags,
				ReleaseNotes = rawPackageConfig.ReleaseNotes,
				RequiresLicenseAcceptance = rawPackageConfig.RequiresLicenseAcceptance,
				Dependencies = rawPackageConfig.Dependencies,
				NuspecFile = rawPackageConfig.NuspecFile
			};
		}

		private string PopulatePlaceHolder(string placeHolder, string nuSpecValue, string assemblyAttributeValue)
		{
			if (nuSpecValue == $"${placeHolder}$")
			{
				return assemblyAttributeValue ?? nuSpecValue;
			}

			return nuSpecValue;
		}

		public TargetTransformationExecutionMoment ExecutionMoment =>
			TargetTransformationExecutionMoment.Early;
	}
}
