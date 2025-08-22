set shell := ["pwsh", "-c"]


# Build the solution
build:
	dotnet build

# Run benchmarks (Release)
bench:
	dotnet run --configuration Release --project "perf/HttpUserAgentParser.Benchmarks/HttpUserAgentParser.Benchmarks.csproj" --framework net10.0

# Clean the solution
clean:
	dotnet clean
