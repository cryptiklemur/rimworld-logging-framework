.PHONY: help all clean restore build build-core test format lint pack

CONFIG ?= Debug
SLN := CryptikLemur.RimLogging.sln

help:
	@echo "Targets:"
	@echo "  all              restore + build whole solution"
	@echo "  clean            remove bin/, obj/, Assemblies/"
	@echo "  restore          dotnet restore"
	@echo "  build            build whole solution"
	@echo "  build-core       build only CryptikLemur.RimLogging"
	@echo "  test             run xunit suites"
	@echo "  format           dotnet format"
	@echo "  lint             dotnet format --verify-no-changes"
	@echo "  pack             create nuget packages (Release)"

all: restore build

clean:
	rm -rf Assemblies/*.dll Assemblies/*.pdb Assemblies/*.xml
	rm -rf CryptikLemur.RimLogging/bin CryptikLemur.RimLogging/obj
	rm -rf CryptikLemur.RimLogging.Tests/bin CryptikLemur.RimLogging.Tests/obj

restore:
	dotnet restore $(SLN)

build:
	dotnet build $(SLN) -c $(CONFIG) --nologo

build-core:
	dotnet build CryptikLemur.RimLogging/CryptikLemur.RimLogging.csproj -c $(CONFIG) --nologo

test:
	dotnet test $(SLN) -c $(CONFIG) --nologo

format:
	dotnet format $(SLN)

lint:
	dotnet format $(SLN) --verify-no-changes

pack:
	dotnet pack CryptikLemur.RimLogging/CryptikLemur.RimLogging.csproj -c Release --nologo -o out/nupkg
