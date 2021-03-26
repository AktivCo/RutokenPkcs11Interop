#!/bin/bash

# Define paths to necessary directories
workingdir=$(pwd)
outputdir=${workingdir}/nuget-unsigned

while true
do
	# Clean output directory
	rm -rf "$outputdir"
	mkdir -p "$outputdir"

	# Remove leftovers of any previous builds
	rm -rf ../src/RutokenPkcs11Interop/bin
	rm -rf ../src/RutokenPkcs11Interop/obj
	rm -rf ../src/RutokenPkcs11Interop.Tests/bin
	rm -rf ../src/RutokenPkcs11Interop.Tests/obj

	# Restore dependencies
	dotnet restore ../src/RutokenPkcs11Interop.sln || break

	# Clean solution
	dotnet clean ../src/RutokenPkcs11Interop.sln -c Release || break

	# Build solution
	dotnet build ../src/RutokenPkcs11Interop.sln -c Release || break

	# Copy packages to output directory
	cp ../src/RutokenPkcs11Interop/bin/Release/*.nupkg "$outputdir" || break
	cp ../src/RutokenPkcs11Interop/bin/Release/*.snupkg "$outputdir" || break

	echo "*** BUILD SUCCESSFUL ***"
	exit 0
done

echo "*** BUILD FAILED ***"
exit 1
