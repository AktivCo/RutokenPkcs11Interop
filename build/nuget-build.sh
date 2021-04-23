#!/bin/bash

# Define paths to necessary directories
scriptfile=$(readlink -f "$0")
workingdir=$(dirname "${scriptfile}")
outputdir=${workingdir}/nuget-unsigned
projectdir=$(realpath ${workingdir}/..)

while true
do
	# Clean output directory
	rm -rf "$outputdir"
	mkdir -p "$outputdir"

	# Remove leftovers of any previous builds
	rm -rf "${projectdir}/src/RutokenPkcs11Interop/bin"
	rm -rf "${projectdir}/src/RutokenPkcs11Interop/obj"
	rm -rf "${projectdir}/src/RutokenPkcs11Interop.Tests/bin"
	rm -rf "${projectdir}/src/RutokenPkcs11Interop.Tests/obj"

	# Restore dependencies
	dotnet restore "${projectdir}/src/RutokenPkcs11Interop.sln" || break

	# Clean solution
	dotnet clean "${projectdir}/src/RutokenPkcs11Interop.sln" -c Release || break

	# Build solution
	dotnet build "${projectdir}/src/RutokenPkcs11Interop.sln" -c Release || break

	# Copy packages to output directory
	cp "${projectdir}/src/RutokenPkcs11Interop/bin/Release/"*.nupkg "$outputdir" || break
	cp "${projectdir}/src/RutokenPkcs11Interop/bin/Release/"*.snupkg "$outputdir" || break

	echo "*** BUILD SUCCESSFUL ***"
	exit 0
done

echo "*** BUILD FAILED ***"
exit 1
