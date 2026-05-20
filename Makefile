.PHONY: help all clean restore build build-core build-ui test format lint pack

CONFIG ?= Debug
SLN := Cryptiklemur.RimLogging.sln

help:
	@echo "Targets:"
	@echo "  all              restore + build whole solution"
	@echo "  clean            remove bin/, obj/, Assemblies/"
	@echo "  restore          dotnet restore"
	@echo "  build            build whole solution"
	@echo "  build-core       build only Cryptiklemur.RimLogging"
	@echo "  build-ui         build only Cryptiklemur.RimLogging.UI"
	@echo "  test             run xunit suites"
	@echo "  format           dotnet format"
	@echo "  lint             dotnet format --verify-no-changes"
	@echo "  pack             create nuget packages (Release)"

all: restore build

clean:
	rm -rf Assemblies/*.dll Assemblies/*.pdb Assemblies/*.xml
	rm -rf Cryptiklemur.RimLogging/bin Cryptiklemur.RimLogging/obj
	rm -rf Cryptiklemur.RimLogging.UI/bin Cryptiklemur.RimLogging.UI/obj
	rm -rf Cryptiklemur.RimLogging.Tests/bin Cryptiklemur.RimLogging.Tests/obj
	rm -rf Cryptiklemur.RimLogging.UI.Tests/bin Cryptiklemur.RimLogging.UI.Tests/obj

restore:
	dotnet restore $(SLN)

build:
	dotnet build $(SLN) -c $(CONFIG) --nologo

build-core:
	dotnet build Cryptiklemur.RimLogging/Cryptiklemur.RimLogging.csproj -c $(CONFIG) --nologo

build-ui:
	dotnet build Cryptiklemur.RimLogging.UI/Cryptiklemur.RimLogging.UI.csproj -c $(CONFIG) --nologo

test:
	dotnet test $(SLN) -c $(CONFIG) --nologo

format:
	dotnet format $(SLN)

lint:
	dotnet format $(SLN) --verify-no-changes

pack:
	dotnet pack Cryptiklemur.RimLogging/Cryptiklemur.RimLogging.csproj -c Release --nologo -o out/nupkg
	dotnet pack Cryptiklemur.RimLogging.UI/Cryptiklemur.RimLogging.UI.csproj -c Release --nologo -o out/nupkg
