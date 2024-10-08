default: all

all:
	dotnet publish --ucr -c Release -o . -p:PublishSingleFile=true -p:AssemblyName=ipk24chat-client -p:DebugType=None -p:DebugSymbols=false

pack:
	zip -r xsalta01.zip ./attachments ./CHANGELOG.md ./Makefile ./README.md ./LICENSE ./*.csproj ./*.cs