@echo off

cd ProtobufModel

echo --delete all files in bin\Release\ [删除旧的生成文件]
del /q bin\Release\*

echo --delete all files in obj\Release\ [删除旧的生成文件]
del /q obj\Release\*

echo --gen proto message to ProtobufModel.cs [生成解析代码,全部放到一个cs源码里]
cd ..\gen
call __genbat.bat

echo --compile ProtobufModel.cs [编译源码 生成DLL]
cd ..\ProtobufModel
C:\Windows\Microsoft.NET\Framework\v4.0.30319\Csc.exe /noconfig /nowarn:1701,1702 /nostdlib+ /errorreport:prompt /warn:4 /define:TRACE /reference:C:\Windows\Microsoft.NET\Framework\v2.0.50727\mscorlib.dll /reference:protobuf-net.dll /reference:C:\Windows\Microsoft.NET\Framework\v2.0.50727\System.dll /debug:pdbonly /filealign:512 /optimize+ /out:obj\Release\ProtobufModel.dll /target:library /utf8output ProtobufModel.cs Properties\AssemblyInfo.cs

echo --output to bin\Release\ [设置DLL输出路径]
copy obj\Release\ProtobufModel.dll bin\Release\ProtobufModel.dll
copy obj\Release\ProtobufModel.pdb bin\Release\ProtobufModel.pdb
copy protobuf-net.dll bin\Release\protobuf-net.dll

echo --precompile ProtobufModel.dll [生成专门序列化的DLL文件]
cd bin\Release
..\..\..\protobuf-net\Precompile\precompile.exe ProtobufModel.dll -o:ProtobufModelSerializer.dll -t:ProtobufModelSerializer

: 复制文件到指定文件夹
echo --copy dlls to unity project [复制文件到指定文件夹Bin]
copy ProtobufModel.dll ..\..\..\Bin\ProtobufModel.dll
copy ProtobufModelSerializer.dll ..\..\..\Bin\ProtobufModelSerializer.dll
copy protobuf-net.dll ..\..\..\Bin\protobuf-net.dll
copy ..\..\..\ProtobufModel\ProtobufModel.cs ..\..\..\..\Assets\Scripts\DataModel\ProtobufModel.cs


echo --completed!!![打完收工,请去当前文件夹Bin下复制三个DLL到你的U3D项目里]

pause
exit