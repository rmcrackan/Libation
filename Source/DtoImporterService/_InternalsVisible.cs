// ApplicationServices.Tests rather than a DtoImporterService.Tests of its own: this project has no test
// assembly, and ApplicationServices references it, so its tests can already see the importers. A literal
// because the dependency runs the other way, leaving nothing here to take the name from.
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ApplicationServices.Tests")]
