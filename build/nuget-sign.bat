@setlocal

if "%~1"=="" goto :error_show_usage
if "%~2"=="" goto :error_show_usage

@rem Define paths to necessary directories
set workingdir=%~dp0
set inputdir=%workingdir%nuget-unsigned
set outputdir=%workingdir%nuget-signed

@rem Define paths to necessary tools
set NUGET=%1 
set SEVENZIP="c:\Program Files\7-Zip\7z.exe"
set SIGNTOOL="C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe"

@rem Define signing options
set CERTHASH=%2
set TSAURL=http://time.certum.pl/
set LIBNAME=RutokenPkcs11Interop
set LIBURL=https://github.com/AktivCo/RutokenPkcs11Interop

@rem Clean output directory
rmdir /S /Q %outputdir%
mkdir %outputdir% || goto :error

@rem Copy unsigned package to output directory
copy %inputdir%\*.nupkg %outputdir% || goto :error

@rem Extract unsigned package contents into the output directory
cd %outputdir% || goto :error
%SEVENZIP% x *.nupkg || goto :error
rmdir /S /Q _rels || goto :error
rmdir /S /Q package || goto :error
del /Q *.xml || goto :error
del /Q *.nupkg || goto :error

@rem Sign all assemblies using SHA256withRSA algorithm
%SIGNTOOL% sign /sha1 %CERTHASH% /fd sha256 /tr %TSAURL% /td sha256 /d %LIBNAME% /du %LIBURL% ^
lib\net40\RutokenPkcs11Interop.dll ^
lib\net45\RutokenPkcs11Interop.dll ^
lib\netstandard2.0\RutokenPkcs11Interop.dll || goto :error

@rem Create signed package with signed assemblies
%NUGET% pack RutokenPkcs11Interop.nuspec || goto :error
%NUGET% sign RutokenPkcs11Interop*.nupkg -CertificateFingerprint %CERTHASH% -Timestamper %TSAURL% || goto :error
%NUGET% verify -Signature RutokenPkcs11Interop*.nupkg || goto :error
copy %inputdir%\*.snupkg . || goto :error

@rem Clean up
rmdir /S /Q lib || goto :error
del /Q *.nuspec || goto :error

@echo *** SIGN SUCCESSFUL ***
@endlocal
@exit /b 0

:error_show_usage
@echo Usage:
@echo nuget-sign.bat NUGET_PATH CERT_HASH

:error
@echo *** SIGN FAILED ***
@endlocal
@exit /b 1
